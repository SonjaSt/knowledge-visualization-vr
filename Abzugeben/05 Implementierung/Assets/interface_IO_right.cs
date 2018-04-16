using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interface_IO_right : MonoBehaviour {

    private SteamVR_TrackedController controller;
    private GameObject mainCamera;
    private LineRenderer line;

    public GameObject vis;

    private GameObject hitNode;
    private MeshRenderer hitMesh;

    private bool isTriggerDown = false;
    private bool isPadDown = false;
    private bool menuActive = false;

    //set this in Unity to get the parent of the graph structure for manipulation (!)
    public GameObject GraphContainer;

    private Vector3 lastPos;

    private void Start()
    {
        lastPos = this.transform.position;
        line = this.gameObject.AddComponent<LineRenderer>();
        foreach (Transform child in this.transform.parent)
        {
            if (child.tag.Equals("MainCamera"))
            {
                mainCamera = child.gameObject;
            }
        }
    }

    private void Update()
    {
        if (isTriggerDown)
        {
            rotateGraphContainer();
        }
        if (isPadDown)
        {
            showRaycast();
        }
        lastPos = this.transform.position;
    }

    private void OnEnable()
    {
        controller = GetComponent<SteamVR_TrackedController>();
        controller.TriggerClicked += activateTrigger;
        controller.TriggerUnclicked += deactivateTrigger;
        controller.MenuButtonClicked += activateMenu;
        controller.PadClicked += activateRaycast;
        controller.PadUnclicked += deactivateRaycast;
    }

    private void OnDisable()
    {
        controller.TriggerClicked -= activateTrigger;
        controller.TriggerUnclicked -= deactivateTrigger;
        controller.MenuButtonClicked -= activateMenu;
        controller.PadClicked -= activateRaycast;
        controller.PadUnclicked -= deactivateRaycast;
    }

    private void activateTrigger(object sender, ClickedEventArgs e)
    {
        Debug.Log("Trigger activated");
        isTriggerDown = true;
    }

    private void deactivateTrigger(object sender, ClickedEventArgs e)
    {
        Debug.Log("Trigger deactivated");
        isTriggerDown = false;
    }

    //this rotation is not accurate to real life,
    //however rotations are very finnicky and most ideas I tested wouldn't work.
    //so here I stick to an approach that isn't realistic, but useable
    private void rotateGraphContainer()
    {
        float difX = -(this.transform.position.y - lastPos.y) * 100.0f;
        float difY = (this.transform.position.x - lastPos.x) * 100.0f;
        GraphContainer.transform.Rotate(difX, difY, 0.0f, Space.World);
    }

    private void activateMenu(object sender, ClickedEventArgs e)
    {
        //add Menu functionality
        if (!menuActive)
        {
            mainCamera.GetComponent<interfaceMenu>().enabled = true;
            menuActive = true;
        }
        else
        {
            mainCamera.GetComponent<interfaceMenu>().enabled = false;
            menuActive = false;
        }
    }

    private void activateRaycast(object sender, ClickedEventArgs e)
    {
        isPadDown = true;
        line.enabled = true;
    }

    private void deactivateRaycast(object sender, ClickedEventArgs e)
    {
        isPadDown = false;
        line.enabled = false;
        RaycastHit hit;
        Ray direction = new Ray(this.transform.position, this.transform.forward);
        if (Physics.Raycast(direction, out hit, 500.0f))
        {
            if (hit.transform.tag.Equals("node"))
            {
                if (hitNode != null)
                {
                    if (!hitNode.transform.name.Equals(hit.transform.name))
                    {
                        Debug.Log("hit a node");
                        hitMesh.material.color = Color.white;
                        hitNode = GameObject.Find(hit.transform.name);
                        hitMesh = hitNode.GetComponent<MeshRenderer>();
                        hitMesh.material.color = Color.red;
                    }
                }
                else
                {
                    Debug.Log("still hit a node");
                    hitNode = GameObject.Find(hit.transform.name);
                    hitMesh = hitNode.GetComponent<MeshRenderer>();
                    hitMesh.material.color = Color.red;
                }
            }

            if(hitNode!=null) Debug.Log("hitnode: "+hitNode.ToString());

            if(mainCamera.GetComponent<interfaceMenu>().enabled) mainCamera.GetComponent<interfaceMenu>().hitButton(hit.transform.tag);
            if(mainCamera.GetComponent<nodeMenu>().enabled) mainCamera.GetComponent<nodeMenu>().hitButton(hit.transform.tag, hitNode);
            if (hit.transform.tag.Equals("test"))
            {
                GraphVisualizer visualize = vis.GetComponent<GraphVisualizer>();
                StartCoroutine(visualize.parseContentFromHTML("Roger Waters"));
            }
        }

    }
    
    private void showRaycast()
    {
        line.SetPosition(0, this.transform.position+this.transform.forward*0.1f);
        line.SetPosition(1, this.transform.forward * 1000);
        line.material.color = Color.red;
        line.SetWidth(0.02f, 0.02f);
    }
}
