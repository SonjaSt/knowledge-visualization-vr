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
    string allLinks = "https://en.wikipedia.org/w/api.php?action=query&prop=links&plnamespace=0&pllimit=max&titles="; //this needs +title
    public IEnumerator requestContent(string apiRequest)
    {
        finished = false;
        using (UnityWebRequest request = UnityWebRequest.Get(apiRequest))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) Debug.Log(request.error);
            else
            {
                content = request.downloadHandler.text;
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

    //WIP
    public IEnumerator requestNeighbors(string title)
    {
        finished = false;
        neighbors = new List<string>();
        yield return StartCoroutine(requestContent(allLinks));
        //this "ONLY" returns first 500 elements. usually this is enough, however some articles have more than 500 links.
        //check if the request has to be continued (see WikiMedia API on continue)
        //this is the case if there is a plcontinue in the json result or a continue or there is no "batchcomplete".
        //parse list to fit needs
        //split along "links". everything that follows are links on a given page, preceded by "title"
        neighbors = list;
        finished = true;
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
