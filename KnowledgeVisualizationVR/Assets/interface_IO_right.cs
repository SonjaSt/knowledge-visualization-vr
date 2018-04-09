using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class interface_IO_right : MonoBehaviour {

    private SteamVR_TrackedController controller;

    private bool isTriggerDown = false;

    //set this in Unity to get the parent of the graph structure for manipulation (!)
    public GameObject GraphContainer;
    public GameObject testObject;

    private Vector3 lastPos;

    private void Start()
    {
        lastPos = this.transform.position;
    }

    private void Update()
    {
        if (isTriggerDown)
        {
            Debug.Log("reached isTriggerDown in Update");
            Debug.Log("difference x, calculated: " + (this.transform.position.x - lastPos.x));
            //Rotation um x-Achse: y+z
            //Rotation um y-Achse: x+z
            //Rotation um z-Achse: x+y
            float difX = -(this.transform.position.y - lastPos.y) * 100.0f;
            float difY = (this.transform.position.x - lastPos.x) * 100.0f;
            //float difX = ((this.transform.position.y - lastPos.y) + (this.transform.position.z - lastPos.z)) * 100.0f;
            //float difY = ((this.transform.position.x - lastPos.x) + (this.transform.position.z - lastPos.z)) * 100.0f;
            //float difZ = ((this.transform.position.x - lastPos.x) + (this.transform.position.y - lastPos.y)) * 100.0f;
            
            testObject.transform.Rotate(difX, difY, 0.0f, Space.World);
        }

        lastPos = this.transform.position;
    }

    private void OnEnable()
    {
        controller = GetComponent<SteamVR_TrackedController>();
        controller.TriggerClicked += activateTrigger;
        controller.TriggerUnclicked += deactivateTrigger;
    }

    private void OnDisable()
    {
        controller.TriggerClicked -= activateTrigger;
        controller.TriggerUnclicked -= deactivateTrigger;
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
}
