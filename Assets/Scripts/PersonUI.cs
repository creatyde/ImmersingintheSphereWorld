using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonUI : MonoBehaviour {

    public GameObject scriptObject;
    public GameObject ind;
    public Text textUI;
    RectTransform mRect;

    private Quaternion ballRotation;
    private Vector2 controller;
    private Vector3 up = Vector3.up; // axis of rotation

    // script sources
    UpdateOrientation uo;
    // StreamInputUI uo;
    ControllerExamine ce;

    // Use this for initialization
    void Start () {
        uo = scriptObject.GetComponent<UpdateOrientation>();
        ce = scriptObject.GetComponent<ControllerExamine>();
        mRect = ind.GetComponent<RectTransform>();
    }
	
	// Update is called once per frame
	void Update () {
        // update values
        ballRotation = uo.GetRotation(); // where person is looking at (forward)
        controller = ce.GetVector();

        // get axis of orientation, the "down-vector" in local, where the person looks up 
        Vector3 axis = -(ballRotation * Vector3.forward).normalized;
        // treat the controller as if it is a circle is facing you
        Vector3 cont = new Vector3(controller.x, controller.y, 0);
        // using ballRotation, transform normal to get normal in "world space",
        // now treating "up" as the axis of the ball
        Vector3 normal = (ballRotation * -cont).normalized;
        // vector orthogonal to both the normal and the axis
        Vector3 b = Vector3.Cross(normal, axis);
        Vector3 pos3D = Vector3.Cross(b, up);
        Vector2 pos2D = new Vector2(pos3D.x, pos3D.z).normalized;

        mRect.anchoredPosition = 120 * pos2D;
        textUI.text = (pos3D.magnitude* pos3D.magnitude).ToString() // accuracy
            + "\n" + controller.magnitude.ToString(); // desire
    }
}
