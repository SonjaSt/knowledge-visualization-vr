using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interface_IO_left : MonoBehaviour {

    private SteamVR_TrackedController controller;

    //set this in Unity to get the parent of the graph structure for manipulation (!)
    public GameObject GraphContainer;
    public GameObject testObject;

    public GameObject logicHandler;

    private Vector3 lastPos;

    private bool isTriggerDown = false;

    private void Start()
    {
        lastPos = this.transform.position;
    }

    private void Update()
    {
        if (isTriggerDown)
        {
            testObject.transform.position += this.transform.position - lastPos;
        }

        lastPos = this.transform.position;
    }

    private void OnEnable()
    {
        controller = GetComponent<SteamVR_TrackedController>();
        controller.MenuButtonClicked += openMenu;
        controller.TriggerClicked += activateTrigger;
        controller.TriggerUnclicked += deactivateTrigger;
    }

    private void OnDisable()
    {
        controller.MenuButtonClicked -= openMenu;
        controller.TriggerClicked -= activateTrigger;
        controller.TriggerUnclicked -= deactivateTrigger;
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
}
