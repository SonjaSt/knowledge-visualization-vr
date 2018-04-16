using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interfaceMenu : MonoBehaviour {
    
    public GameObject vis;
    private GraphVisualizer visualizer;
    public GameObject menu;
    private GameObject activeMenu;
    public Material mat;
    public Font f;

    private int hops;
    private int maxNodes;

    TextMesh[] meshes;

	// Use this for initialization
	void Start () {
    }

    private void OnEnable()
    {
        meshes = new TextMesh[4];
        visualizer = vis.GetComponent<GraphVisualizer>();
        hops = visualizer.getHops();
        maxNodes = visualizer.getMaxNodes();
        activeMenu = Instantiate(menu, this.gameObject.transform);
        activeMenu.transform.position = this.gameObject.transform.position;
        activeMenu.transform.rotation = activeMenu.transform.parent.rotation;
        activeMenu.transform.position = activeMenu.transform.forward * 5;
        Debug.Log("Tag: " + activeMenu.transform.tag);

        foreach (Transform innerChild in activeMenu.transform)
        {
            Debug.Log("In foreach for child");
            Debug.Log(innerChild.tag);
            if (innerChild.tag.Equals("hopCount"))
            {
                meshes[0] = innerChild.GetComponent<TextMesh>();
                meshes[0].text = hops.ToString();
            }
            if (innerChild.tag.Equals("hundert"))
            {
                meshes[1] = innerChild.GetComponent<TextMesh>();
                meshes[1].text = (maxNodes / 100).ToString();
            }
            if (innerChild.tag.Equals("zehn"))
            {
                meshes[2] = innerChild.GetComponent<TextMesh>();
                meshes[2].text = ((maxNodes % 100) / 10).ToString();
            }
            if (innerChild.tag.Equals("eins"))
            {
                meshes[3] = innerChild.GetComponent<TextMesh>();
                meshes[3].text = ((maxNodes % 100) % 10).ToString();
            }
        }
        
    }

    private void OnDisable()
    {
        Destroy(activeMenu);
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void hitButton(string tag)
    {
        if (tag.Equals("randomArticle"))
        {
            Debug.Log("activate random article");
        }
        if (tag.Equals("draw"))
        {
            Debug.Log("activate draw");
        }
        if (tag.Equals("hopsUp"))
        {
            if (hops < 3)
            {
                hops += 1;
                meshes[0].text = hops.ToString();
                visualizer.setHops(hops);
            }
        }
        if (tag.Equals("hopsDown"))
        {
            if (hops > 0)
            {
                hops -= 1;
                meshes[0].text = hops.ToString();
                visualizer.setHops(hops);
            }
        }
        if (tag.Equals("nodesUp1"))
        {
            if (maxNodes < 201)
            {
                maxNodes += 100;
                meshes[1].text = (maxNodes/100).ToString();
                visualizer.setMaxNodes(maxNodes);
            }
        }
        if (tag.Equals("nodesUp2"))
        {
            if (maxNodes < 291 && (maxNodes%100) < 90)
            {
                maxNodes += 10;
                meshes[2].text = ((maxNodes % 100) / 10).ToString();
                visualizer.setMaxNodes(maxNodes);
            }
        }
        if (tag.Equals("nodesUp3"))
        {
            if (maxNodes < 300 && ((maxNodes % 100) % 10) < 9)
            {
                maxNodes += 1;
                meshes[3].text = ((maxNodes % 100) % 10).ToString();
                visualizer.setMaxNodes(maxNodes);
            }
        }
        if (tag.Equals("nodesDown1"))
        {
            if (maxNodes > 99)
            {
                maxNodes -= 100;
                meshes[1].text = (maxNodes / 100).ToString();
                visualizer.setMaxNodes(maxNodes);
            }
        }
        if (tag.Equals("nodesDown2"))
        {
            if (maxNodes > 9 && (maxNodes % 100) > 9)
            {
                maxNodes -= 10;
                meshes[2].text = ((maxNodes % 100) / 10).ToString();
                visualizer.setMaxNodes(maxNodes);
            }
        }
        if (tag.Equals("nodesDown3"))
        {
            if (maxNodes > 1 && ((maxNodes % 100) % 10) > 0)
            {
                maxNodes -= 1;
                meshes[3].text = ((maxNodes % 100) % 10).ToString();
                visualizer.setMaxNodes(maxNodes);
            }
        }
        if (tag.Equals("randomArticle"))
        {
            StartCoroutine(visualizer.getRandomGraph(hops));
            //StartCoroutine(visualizer.getGraph("Roger Waters", hops));
            //obiger Befehl wurde zum Testen verwendet und nebenbei die Wikipediaseite im Browser geöffnet
        }
        if (tag.Equals("draw"))
        {
            Debug.Log("time to draw graph");
            visualizer.drawGraph();
        }
    }

    /**
     * ANHANG
     * *


    /*
 * Dies war eine ursprüngliche Methode zur Ersteulung von Buttons
*/
    /*
    private void instantiateButtons()
    {
        
        GameObject hops = new GameObject("hops");
        //GameObject hops = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //hops.tag = "hops";
        hops.transform.parent = this.gameObject.transform;
        hops.transform.rotation = hops.transform.parent.rotation;
        hops.transform.localScale = new Vector3(5f, 5f, 5f);
        hops.transform.position = hops.transform.forward * 100;
        TextMesh text1 = hops.AddComponent<TextMesh>();
        text1.font = f;
        text1.text = "Hops:";
        hops.GetComponent<MeshRenderer>().material = mat;

        BoxCollider col = hops.AddComponent<BoxCollider>();
        col.transform.localScale = new Vector3(2f, 3f, 0f);
        buttons.Add(hops);
        

        
        GameObject hops = Instantiate(buttons[2]);
        hops.transform.parent = this.gameObject.transform;
        hops.transform.rotation = hops.transform.parent.rotation;
        hops.transform.position = hops.transform.forward * 10;
        currentButtons.Add(hops);
        
    }
    */
}
