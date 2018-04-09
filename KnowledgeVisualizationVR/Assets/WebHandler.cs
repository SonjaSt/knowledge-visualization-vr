using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//This class handles all outgoing and ingoing traffic with the web
//It take URIs and extracts information from web pages
//Information is to be put into nodes in the Graph
public class WebHandler : MonoBehaviour{
    string content = "";
    List<string> neighbors = null;
    bool finished = true;
    string message = "https://de.wikipedia.org/w/api.php?action=query&format=json&prop=extracts&explaintext=&titles="; //this needs +title
    string random = "https://de.wikipedia.org/w/api.php?action=query&list=random&format=json&rnnamespace=0&rnlimit=1";
    string allLinks = "https://de.wikipedia.org/w/api.php?action=query&format=json&prop=links&plnamespace=0&pllimit=max&titles="; //this needs +title
    //getting data for more than one page is not easy, therefore.. don't do it.

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
                Debug.Log("Time to retrieve data from Wikimedia API: "+ (System.Environment.TickCount - timeElapsed));
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
            content = content.Remove(pos2, content.Length-pos2);
            content = content.Remove(0, pos1);
            finished = true;
            //Debug.Log("content: " + content);
        }
    }

    //If this looks complicated, don't worry!
    //It absolutely is and annoys me to no end.
    //Stupid dynamic responses.
    public IEnumerator requestNeighbors(string title)
    {
        bool finishedReading = false;
        finished = false;
        neighbors = new List<string>();
        yield return StartCoroutine(requestContent(allLinks+title));
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
            int continueIndex = content.IndexOf("plcontinue")+13;
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

    public IEnumerator requestPageviews(List<string> toCheck)
    {
        int elementsToCheck = toCheck.Count;
        string AllToCheck = "";
        for (int i = 0; i < elementsToCheck; i += 50)
        {
            for (int j = 0; j < 50; j++)
            {
                if (toCheck.Count > 0)
                {
                    AllToCheck = AllToCheck + toCheck[0] + '|';
                    toCheck.RemoveAt(0);
                }
            }
            AllToCheck = AllToCheck.Remove(AllToCheck.Length-1);
            Debug.Log(pageviews + AllToCheck);
            //this below data needs parsing
            yield return StartCoroutine(requestContent(pageviews+AllToCheck));
            Debug.Log(content);
            AllToCheck = "";
        }
    }

    //This method takes whatever is in "content" and sections it
    //ATTENTION: content needs to be the content of an actual wikipedia article for this to make sense!
    public string[] getSectionedContent()
    {
        //\\n\\n is necessary for this to work. Check for the beginning of the JSON file and get rid of it.
        string[] separator = new string[] {"\\n\\n"};
        string[] sections = content.Split(separator, System.StringSplitOptions.None);
        return sections;
    }

    //this is made specifically for the "requestNeighbors" function
    public List<string> getLinksInJsonFromResponse(string response)
    {
        List<string> currentNeighbors = new List<string>();
        response = response.Substring(response.IndexOf("query")+8);
        //there is now no batchcomplete or continue in the response string
        response = response.Substring(response.IndexOf("title")+8);
        //the first phrase in this substring is now a title. we now split along quotation marks to avoid
        //accidentally splitting in a title that contains the word title or link!
        response = response.Substring(response.IndexOf('"'));
        //now the first word is "links" this is a list of classes with data "ns" and "title"
        //the first occurence of the word "title" is now always actually the name of the data "title"
        //therefore splitting along "title" and then along quotation marks should be safe
        while (response.IndexOf("title") >= 0)
        {
            //let the splitting commence! this while gets all titles and puts them in a list
            response = response.Substring(response.IndexOf("title")+8);
            currentNeighbors.Add(response.Substring(0, response.IndexOf('"')));
            //get the next neighbor title and put it in currentNeighbors
            response = response.Substring(response.IndexOf('"'));
        }
        return currentNeighbors;
    }

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
