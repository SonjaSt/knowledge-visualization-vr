using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHTMLParser : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    /*
    public Graph.Information.Chapter parseHTML(string html, string name)
    {
        Graph.Information.Chapter chapter = new Graph.Information.Chapter();
        //note: this method expects an html-formatted string SPECIFICALLY from wikipedia.
        if (!html.Contains("<div id=\"mv-content-text\""))
        {
            Debug.Log("Could not identify text in article!");
            return null;
        }
        int bodyStart = html.IndexOf("<div id=\"mv-content-text\"");
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
            html = html.Remove(bodyEnd, bodyStart);
            //this should remove everything from where the ToC starts to where it ends
            //it starts at <div id="toc" (bodyEnd)
            //it ends at the second </div> after that
        }
        //leftover html consist of multiple <p>s, <hxx>s and <div...> classes.
        //<par>s indicate paragraphs and can be grouped together.
        //<hxx>s indicate chapter titles and their depth
        //h1 is the article title, h2 are the highest level titles/chapters in the article
        //h3s are contained in h2s and so on
        //so: parse for h2s first, then h3, then h4...
        //I am going to assume it shouldn't go deeper than 10, but who knows, honestly?
        //note: there ARE paragraphs before the ToC. These are the content of h1, basically
        //refer to the accompanying bachelor's thesis on visualization of node information here

        chapter.setTitle(name); //set the name passed as title as main chapter
        int[] counters = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        int chapterDepth = 0;
        //the level of depth is most likely not going to go above 10
        //in this while we will go through the file
        while (html.Contains("<p>") || html.Contains("<h"))
        {
            int nextPar = html.IndexOf("<p>");
            int nextChapter = html.IndexOf("<h");
            Graph.Information.Chapter c = chapter;
            if (nextPar < nextChapter && nextPar >= 0)
            {
                //parse paragraph
                //every <p> has a </p>
                int beginP = html.IndexOf("<p>");
                int endP = html.IndexOf("</p>");
                if (chapterDepth == 0)
                {
                    //set Content of main chapter to new content
                    string tmpContent = html.Substring(beginP + 3, endP - (beginP + 3));
                    Debug.Log(tmpContent);
                    c.setContent(c.getContent() + tmpContent + "\n ");
                    html = html.Remove(0, endP + 4);
                }
                else
                {
                    
                    //else, access subchapters
                    //for every point in chapterDepth, go a level deeper
                    for (int i = 0; i < chapterDepth; i++)
                    {
                        int counter = counters[i];
                        c = c.getSubchapters()[counter];
                        //first call gets subChapters of main at [counter]
                        //we keep track of our amount of subchapters with counter
                        //this is risky, but should not lead to errors
                        //as subchapters are initiated as soon as titles are found
                        //so: first, depth = 0; no need to call subchapters
                        //then: <hx> is found, subchapters are initiated
                        //Note that we work on an instance of our main chapter!
                        if (i == (chapterDepth - 1))
                        {
                            string tmpContent = html.Substring(beginP + 3, endP - (beginP + 3));
                            Debug.Log(tmpContent);
                            c.setContent(c.getContent() + tmpContent + "\n ");
                            html = html.Remove(0, endP + 4);
                        }
                    }
                }

            }
            else
            {
                //parse chapter
                if (nextChapter >= 0)
                {
                    int hNumber = html.IndexOf("<h");
                    string numberS = html.Substring(hNumber + 2, 1);
                    int number = 0;
                    System.Int32.TryParse(numberS, out number);
                    
                    //get title and, depending on level, put into mainchapter
                    string divide = "class=\"mw-headline\">";
                    int startH = html.IndexOf(divide);
                    startH = html.IndexOf(">", startH);
                    int endH = html.IndexOf("</span>", startH);
                    string title = html.Substring(startH + 1, endH);

                    
                    for (int i = 0; i < chapterDepth; i++)
                    {
                        int counter = counters[i];
                        c = c.getSubchapters()[counter];
                        if (i == chapterDepth - 1)
                        {

                            if (counter == 0)
                            {
                                c.setSubchapters(new List<Graph.Information.Chapter>());
                            }
                            Graph.Information.Chapter chap = new Graph.Information.Chapter();
                            chap.setTitle(title);
                            c = chap;
                        }
                    }

                    html = html.Remove(0, endH + 7);

                    chapterDepth = number - 1;
                    counters[chapterDepth - 1]++;

                }
            }


        }

        return null;
    }
    */

    public Graph makeGraphFromNode(Graph.Node node)
    {
        if (!node.hasContent())
        {
            Debug.Log("Given node has no content!");
            return null;
        }
        //build a graph based on the internal Structure of an article
        //go through chapters for this, they indicate links
        return null;
    }

    //this assumes almost raw content
    //meaning there are no 
    public string removeHTMLTags(string input)
    {
        while (input.Contains("<") && input.Contains(">"))
        {
            int startIndex = input.IndexOf("<");
            int endIndex = input.IndexOf(">", startIndex);
            input.Remove(startIndex, endIndex - (startIndex)+1);
            //this should remove everything between < and >
            //note: this isn't safe at all and really just a means to an end...
        }
        return input;
    }
}
