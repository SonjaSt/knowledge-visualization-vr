using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//This class handles all outgoing and ingoing traffic with the web
//It take URIs and extracts information from web pages
//Information is to be put into nodes in the Graph
public class WebHandler {
    string content = "";
    public IEnumerator requestContent(string page)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(page))
        {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) Debug.Log(request.error);
            else
            {
                content = request.downloadHandler.text;
            }
        }
    }

    public string[] getSectionedContent()
    {
        //\\n\\n is necessary for this to work. Check for the beginning of the JSON file and get rid of it.
        string[] separator = new string[] {"\\n\\n"};
        string[] sections = content.Split(separator, System.StringSplitOptions.None);
        return sections;
    }

    public string getContent()
    {
        return content;
    }
}
