using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nodeMenu : MonoBehaviour {

    public Material mat;
    public Font font;

    public GameObject vis;
    private GraphVisualizer visualizer;
    public GameObject menu;
    private GameObject activeMenu;

    private bool fetchingData = false;

    string currentNode;
    private bool isdrawn = false;
    private bool waitForGraph = false;

    private GameObject activeNode;
    private bool nodeShown = false;
    private bool structShown = false;

    private GameObject[] visContent;

    string[] nodeContent;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (!visualizer.getGraphTitle().Equals("")) currentNode = visualizer.getGraphTitle();
        if (visualizer.isSectionedDone() && !isdrawn)
        {
            Debug.Log("getSectioned not empty");
            nodeContent = visualizer.getSectioned();
            visContent = new GameObject[nodeContent.Length];
            for (int i = 0; i < visContent.Length; i++)
            {
                visContent[i] = new GameObject();
                TextMesh txt = visContent[i].AddComponent<TextMesh>();
                
                MeshRenderer rend = visContent[i].GetComponent<MeshRenderer>();
                rend.material = mat;
                txt.font = font;
                txt.text = nodeContent[i];
                Vector3 centerPos = this.transform.position + this.transform.forward * 100;
                visContent[i].transform.position = centerPos + -transform.up * 5 * (i - (visContent.Length/2));
                visContent[i].transform.rotation = Quaternion.LookRotation(visContent[i].transform.position - transform.position);
            }
            isdrawn = true;
        }
        
	}

    private void OnEnable()
    {
        visualizer = vis.GetComponent<GraphVisualizer>();
        activeMenu = Instantiate(menu, this.gameObject.transform);
        activeMenu.transform.position = this.gameObject.transform.position;
        activeMenu.transform.rotation = activeMenu.transform.parent.rotation;
        activeMenu.transform.position = activeMenu.transform.forward * 5;
        Debug.Log("Tag: " + activeMenu.transform.tag);
        if (!visualizer.getGraphTitle().Equals("")) currentNode = visualizer.getGraphTitle();
    }

    private void OnDisable()
    {
        Destroy(activeMenu);
    }

    public void hitButton(string tag, GameObject node)
    {
        activeNode = node;
        if (tag.Equals("showNode"))
        {
            if (activeNode != null)
            {
                Debug.Log("showing contents of node...");
                //show Content of node
                visualizer.sectionedContent(activeNode.transform.name);
                nodeShown = true;
            }
            Debug.Log("hit showNode");
        }
        if (tag.Equals("hideNode"))
        {
            if (nodeShown == true)
            {
                for (int i = 0; i < visContent.Length; i++)
                {
                    Destroy(visContent[i]);
                }
                visContent = null;
                visualizer.setSectioned();
                nodeContent = null;
                nodeShown = false;
                isdrawn = false;
            }
            Debug.Log("hit hideNode");
        }
        if (tag.Equals("showStructure"))
        {
            if (activeNode != null)
            {
                Debug.Log("showing structure");
                StartCoroutine(visualizer.getGraphFromNode(activeNode.transform.name));
                structShown = true;
            }
            Debug.Log("hit showStructure");
        }
        if (tag.Equals("hideStructure"))
        {
            if (structShown)
            {
                Debug.Log("time to hide structure");
                StartCoroutine(visualizer.getGraph(currentNode, visualizer.getHops()));
                structShown = false;
            }
            Debug.Log("hit hideStructure");
        }
        if (tag.Equals("drawNew"))
        {
            if (activeNode != null)
            {
                StartCoroutine(visualizer.getGraph(activeNode.transform.name, visualizer.getHops()));
            }
            Debug.Log("hit drawNew");
        }
    }
}
