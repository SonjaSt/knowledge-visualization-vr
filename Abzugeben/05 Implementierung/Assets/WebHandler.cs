using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

//This class handles all outgoing and ingoing traffic with the web
//It take URIs and extracts information from web pages
//Information is to be put into nodes in the Graph
public class WebHandler : MonoBehaviour
{
    string content = "";
    string[] sectionedContent;

    public bool isSectionedDone = false;

    List<string> neighbors = null;
    bool finished = true;
    string message = "https://de.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&explaintext=&titles="; //this needs +title
    string random = "https://de.wikipedia.org/w/api.php?action=query&list=random&format=json&rnnamespace=0&rnlimit=1";
    string allLinks = "https://de.wikipedia.org/w/api.php?action=query&format=json&prop=links&plnamespace=0&pllimit=max&titles="; //this needs +title
    //getting data for more than one page is not easy, therefore.. don't do it.

    //Note: pageviews have not been used in this version,
    //however the functionality is there and content retrieved by the
    //requestPageviews Coroutine can be parsed and fit to appropriate titles.
    string pageviews = "https://de.wikipedia.org/w/api.php?action=query&format=json&prop=pageviews&pvipdays=1&titles=";
    //only ever request pageviews of AS MANY PAGES AS POSSIBLE AT THE SAME TIME.

    //Internet communication is extremely slow!
    //Note: you can only enter 50 titles, divided by "|"
    public IEnumerator requestContent(string apiRequest)
    {
        finished = false;
        using (UnityWebRequest request = UnityWebRequest.Get(apiRequest))
        {
            int timeElapsed = System.Environment.TickCount;
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) Debug.Log(request.error);
            else
            {
                content = request.downloadHandler.text;
                //content = cleanUp(content);
                finished = true;
            }
        }
    }

    //This method puts the name of a random article in "content"
    //This method works. DO NOT CHANGE UNLESS YOU ARE ABSOLUTELY CERTAIN YOU NEED TO.
    public IEnumerator requestRandomArticle()
    {
        finished = false;
        yield return StartCoroutine(requestContent(random));
        if (content.Equals("")) { Debug.Log("Received empty string"); }
        else
        {
            int pos1 = content.IndexOf("title");
            pos1 = pos1 + 8; //8 == |titles":"|
            int pos2 = content.LastIndexOf("\"");
            //Debug.Log("pos1: " + pos1 + " pos 2: " + pos2);
            content = content.Remove(pos2, content.Length - pos2);
            content = content.Remove(0, pos1);
            //content = cleanUp(content);
            finished = true;
            //Debug.Log("content: " + content);
        }
    }

    //If this looks complicated, don't worry!
    //It absolutely is and annoys me to no end.
    //Stupid dynamic responses.

    //if a batch is not complete, use the "batchcomplete" statement to retrieve more data.
    public IEnumerator requestNeighbors(string title)
    {
        bool finishedReading = false;
        finished = false;
        neighbors = new List<string>();
        yield return StartCoroutine(requestContent(allLinks + title));
        //content = cleanUp(content);
        if (content.Contains("batchcomplete"))
        {
            neighbors = getLinksInJsonFromResponse(content);
            //Debug.Log("contains batchcomplete");
            finishedReading = true;
        }

        while (!content.Contains("batchcomplete"))
        {
            //Debug.Log("batch is (still) not complete!");
            //get more results while batch is not complete. An uncomplete batch does not contain the phrase "batchcomplete"
            int continueIndex = content.IndexOf("plcontinue") + 13;
            string subContinue = content.Substring(continueIndex);
            int firstQuotationMarks = subContinue.IndexOf('"');

            //this is the part where content gets parsed, meaning links are put into the
            //neighborhood list
            //we use firstQuotationMarks to make sure we don't accidently split in the
            //content of the plcontinue!
            string contentToParse = content.Substring(firstQuotationMarks);
            neighbors.AddRange(getLinksInJsonFromResponse(contentToParse));
            string[] tmp = neighbors.ToArray();
            //Debug.Log(tmp[tmp.Length-1]);
            subContinue = subContinue.Substring(0, firstQuotationMarks);
            //Debug.Log(subContinue);
            //after this step, subContinue contains the string to continue the Wiki API request

            yield return StartCoroutine(requestContent(allLinks + title + "&plcontinue=" + subContinue));
            //Debug.Log(content);
        }

        if (!finishedReading)
        {
            //this is the last thing we do after continuing multiple times
            neighbors.AddRange(getLinksInJsonFromResponse(content));
        }
        //this "ONLY" returns first 500 elements. usually this is enough, however some articles have more than 500 links.
        //check if the request has to be continued (see WikiMedia API on continue)
        //this is the case if there is a plcontinue in the json result or a continue or there is no "batchcomplete".
        //parse list to fit needs
        //split along "links". everything that follows are links on a given page, preceded by "title"
        string[] temporary = neighbors.ToArray();
        //Debug.Log(temporary[temporary.Length - 1]);
        finished = true;
    }

    //Note: this method is not yet used by this implementation
    //Content needs to be parsed and for this it would
    //make sense to overhaul the entire parsing system(!)
    public IEnumerator requestPageviews(List<string> toCheck)
    {
        int time = System.Environment.TickCount;
        int elementsToCheck = toCheck.Count;
        string AllToCheck = "";
        for (int i = 0; i < elementsToCheck; i += 50)
        {
            for (int j = 0; j < 50; j++)
            {
                //Debug.Log("j is " + j);
                if (toCheck.Count > 0)
                {
                    AllToCheck = AllToCheck + toCheck[0] + '|';
                    toCheck.RemoveAt(0);
                }
            }
            AllToCheck = AllToCheck.Remove(AllToCheck.Length - 1);
            Debug.Log(pageviews + AllToCheck);

            //this below data needs parsing
            yield return StartCoroutine(requestContent(pageviews + AllToCheck));
            Debug.Log(content);
            AllToCheck = "";
        }
        time = System.Environment.TickCount - time;
        Debug.Log("retrieved pageviews for " + elementsToCheck + " Nodes in " + time);
    }

    //this method is just a helper-method
    public IEnumerator requestSectionedContent(string title)
    {
        isSectionedDone = false;
        content = "";
        yield return StartCoroutine(requestContent(message+title));
        sectionedContent = getSectionedContent();
        isSectionedDone = true;
    }

    //This method takes whatever is in "content" and sections it
    //ATTENTION: content needs to be the content of an actual wikipedia article for this to make sense!
    public string[] getSectionedContent()
    {
        //\\n\\n is necessary for this to work. Check for the beginning of the JSON file and get rid of it.
        string[] separator = new string[] { "\\n\\n" };
        string[] sections = content.Split(separator, System.StringSplitOptions.None);
        return sections;
    }

    //this is made specifically for the "requestNeighbors" function
    public List<string> getLinksInJsonFromResponse(string response)
    {
        //response = cleanUp(response);
        List<string> currentNeighbors = new List<string>();
        response = response.Substring(response.IndexOf("query") + 8);
        //there is now no batchcomplete or continue in the response string
        response = response.Substring(response.IndexOf("title") + 8);
        //the first phrase in this substring is now a title. we now split along quotation marks to avoid
        //accidentally splitting in a title that contains the word title or link!
        response = response.Substring(response.IndexOf('"'));
        //now the first word is "links" this is a list of classes with data "ns" and "title"
        //the first occurence of the word "title" is now always actually the name of the data "title"
        //therefore splitting along "title" and then along quotation marks should be safe
        while (response.IndexOf("title") >= 0)
        {
            //let the splitting commence! this while gets all titles and puts them in a list
            response = response.Substring(response.IndexOf("title") + 8);
            currentNeighbors.Add(response.Substring(0, response.IndexOf('"')));
            //get the next neighbor title and put it in currentNeighbors
            response = response.Substring(response.IndexOf('"'));
        }
        return currentNeighbors;
    }


    public Graph.Information.Chapter parseHTML(string html, string name)
    {
        Graph.Information.Chapter chapter = new Graph.Information.Chapter(name, null);
        Debug.Log(html);
        //note: this method expects an html-formatted string SPECIFICALLY from wikipedia.
        if (!html.Contains("<div id=\"mw-content-text\""))
        {
            Debug.Log("Could not identify text in article!");
            return null;
        }
        int bodyStart = html.IndexOf("<div id=\"mw-content-text\"");
        bodyStart = html.IndexOf('>', bodyStart);
        html = html.Remove(0, bodyStart);
        //we are now left with everything past the beginning of the content
        int bodyEnd = html.IndexOf("<div class=\"printfooter\""); //find end of content for safety
        html = html.Remove(bodyEnd, html.Length - bodyEnd);
        //now we only have the body. next step is to check for ToC, because this can eff up parsing
        //ToC is marked with <div id="toc"...
        if (html.Contains("<div id=\"toc\""))
        {
            //if ToC exists, just cut it out. it's easier to ignore it..
            //I checked: there is another "<div..." in the ToC.
            //Ignore everything IN BETWEEN the <div id... and the second </div>
            bodyEnd = html.IndexOf("<div id=\"toc\"");

            bodyStart = html.IndexOf("</div>", bodyEnd); //make sure to start searching at ToC, not before
            bodyStart = html.IndexOf("</div>", bodyStart + 1); //get the second </div>
            bodyStart = html.IndexOf(">", bodyStart);
            html = html.Remove(bodyEnd, bodyStart-bodyEnd);
            //this should remove everything from where the ToC starts to where it ends
            //it starts at <div id="toc" (bodyEnd)
            //it ends at the second </div> after that
        }
        Debug.Log("body without toc");
        Debug.Log(html);
        //leftover html consist of multiple <p>s, <hxx>s and <div...> classes.
        //<par>s indicate paragraphs and can be grouped together.
        //<hxx>s indicate chapter titles and their depth
        //h1 is the article title, h2 are the highest level titles/chapters in the article
        //h3s are contained in h2s and so on
        //so: parse for h2s first, then h3, then h4...

        //note: there ARE paragraphs before the ToC. These are the content of h1, basically
        //refer to the accompanying bachelor's thesis on visualization of node information here
        
        int chapterDepth = 0;
        //the level of depth is most likely not going to go above 10
        //in this while we will go through the file

        Graph.Information.Chapter c = chapter;
        //change c to parent or child, depending on next level

        while ((html.Contains("<p>") || html.Contains("<h")))
        {
            int nextPar = html.IndexOf("<p>");
            int nextChapter = html.IndexOf("<h");
            
            if (nextPar < nextChapter && nextPar >= 0)
            {
                Debug.Log("Found paragraph, currently in depth " + chapterDepth);
                //parse paragraph
                //every <p> has a </p>
                int beginP = html.IndexOf("<p>");
                int endP = html.IndexOf("</p>");

                //set Content of main chapter to new content
                string tmpContent = html.Substring(beginP + 3, endP - (beginP + 3));
                Debug.Log(tmpContent);
                //this function does not work properly
                //tmpContent = removeHTMLTags(tmpContent);
                c.setContent(c.getContent() + tmpContent + "\n ");
                html = html.Remove(0, endP + 4);
                
            }
            else
            {
                //parse chapter
                if (nextChapter >= 0)
                {
                    Debug.Log("Found chapter, currently in depth " + chapterDepth);
                    int hNumber = html.IndexOf("<h");
                    string numberS = html.Substring(hNumber + 2, 1);
                    int number = 0;
                    System.Int32.TryParse(numberS, out number);
                    number -= 1;
                    

                    //get title and, depending on level, put into mainchapter
                    string divide = "class=\"mw-headline\"";
                    int startH = html.IndexOf(divide);
                    Debug.Log("divide: " + divide + ", startH: " + startH);
                    startH = html.IndexOf(">", startH);
                    int endH = html.IndexOf("</span>", startH);
                    string title = html.Substring(startH + 1, endH-(startH+1));

                    Debug.Log("Headline title: " + title);

                    if (number > chapterDepth)
                    {
                        Debug.Log("number > depth");
                        if (c.getSubchapters() == null) c.setSubchapters(new List<Graph.Information.Chapter>());
                        Graph.Information.Chapter m = new Graph.Information.Chapter(title, c);
                        c.getSubchapters().Add(m);
                        Debug.Log("Title of current: " + c.getTitle());
                        c = m;
                        Debug.Log("Title of next: " + c.getTitle());
                        chapterDepth = number;
                    }
                    else
                    {
                        if (number < chapterDepth)
                        {
                            Debug.Log("number < depth");
                            if (c.getParent() != null)
                            {
                                if (c.getParent().getSubchapters() == null) c.getParent().setSubchapters(new List<Graph.Information.Chapter>());
                            }
                            Graph.Information.Chapter m = new Graph.Information.Chapter(title, c.getParent().getParent());
                            c.getParent().getParent().getSubchapters().Add(m);
                            Debug.Log("Title of current: " + c.getTitle());
                            c = c.getParent();
                            Debug.Log("Title of next: " + c.getTitle());
                            chapterDepth = number;
                        }
                        else
                        {
                            if (c.getSubchapters() == null) c.setSubchapters(new List<Graph.Information.Chapter>());
                            Graph.Information.Chapter m = new Graph.Information.Chapter(title, c.getParent());
                            c.getParent().getSubchapters().Add(m);
                            Debug.Log("number == depth");
                            Debug.Log("Title of current: " + c.getTitle());
                            Debug.Log("Title of current: " + c.getTitle());
                            chapterDepth = number;
                        }
                    }

                    html = html.Remove(0, endH + 7);
                }
            }
        }
        
        return chapter;
    }

    //this assumes almost raw content
    //this method does not work safely
    public string removeHTMLTags(string input)
    {
        while (input.Contains("<") && input.Contains(">"))
        {
            int startIndex = input.IndexOf("<");
            int endIndex = input.IndexOf(">", startIndex);
            input.Remove(startIndex, endIndex - (startIndex) + 1);
            //this should remove everything between < and >
            //note: this isn't safe at all and really just a means to an end...
        }
        return input;
    }

    /*
     * This method was supposed to format a string
     * However it doesn't work properly
    private string cleanUp(string text)
    {
        
        string cleaned = "";
        byte[] stringBytes = System.Text.Encoding.Unicode.GetBytes(text);
        stringBytes = System.Text.Encoding.Convert(System.Text.Encoding.Unicode, System.Text.Encoding.UTF8, stringBytes);
        char[] textC = new char[System.Text.Encoding.UTF8.GetCharCount(stringBytes, 0, stringBytes.Length)];
        System.Text.Encoding.UTF8.GetChars(stringBytes, 0, stringBytes.Length, textC, 0);
        cleaned = new string(textC);
        
        return text;
    }
    */

    public bool isFinished()
    {
        return finished;
    }

    public string getContent()
    {
        return content;
    }
    public List<string> getNeighbors()
    {
        return neighbors;
    }
}
