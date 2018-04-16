using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interface_IO_left : MonoBehaviour {

    private SteamVR_TrackedController controller;

    //set this in Unity to get the parent of the graph structure for manipulation (!)
    public GameObject GraphContainer;
    private GameObject mainCamera;

    public GameObject logicHandler;

    private Vector3 lastPos;

    private bool isTriggerDown = false;
    private bool menuActive = false;

    private void Start()
    {
        lastPos = this.transform.position;
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
            GraphContainer.transform.position += (this.transform.position - lastPos) *10.0f;
        }

        lastPos = this.transform.position;
    }

    private void OnEnable()
    {
        controller = GetComponent<SteamVR_TrackedController>();
        controller.MenuButtonClicked += openMenu;
        controller.TriggerClicked += activateTrigger;
        controller.TriggerUnclicked += deactivateTrigger;
        controller.MenuButtonClicked += activateMenu;
    }

    private void OnDisable()
    {
        controller.MenuButtonClicked -= openMenu;
        controller.TriggerClicked -= activateTrigger;
        controller.TriggerUnclicked -= deactivateTrigger;
        controller.MenuButtonClicked -= activateMenu;
        //add menu button click -> enables Menu
    }

    private void openMenu(object sender, ClickedEventArgs e)
    {
        var logicScript = logicHandler.GetComponent<GraphVisualizer>();
        logicScript.setNumberOfIterations(42);
    }

    private void activateTrigger(object sender, ClickedEventArgs e)
    {
        isTriggerDown = true;
    }

    private void deactivateTrigger(object sender, ClickedEventArgs e)
    {
        isTriggerDown = false;
    }

    private void activateMenu(object sender, ClickedEventArgs e)
    {
        //add Menu functionality
        if (!menuActive)
        {
            mainCamera.GetComponent<nodeMenu>().enabled = true;
            menuActive = true;
        }
        else
        {
            mainCamera.GetComponent<nodeMenu>().enabled = false;
            menuActive = false;
        }
    }
}
