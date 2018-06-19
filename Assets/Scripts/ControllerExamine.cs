using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerExamine : MonoBehaviour {

    public GameObject ind;
    public TextMesh text;
    RectTransform mRect;

    Vector2 refLine = new Vector2(1f, 0f);
    Vector2 v = new Vector2(1f, 0f);

    /** GET **/
    public Vector2 GetVector()
    {
        return v;
    }

    // Use this for initialization
    void Start () {
        mRect = ind.GetComponent<RectTransform>();
    }

	// Update is called once per frame
	void Update () {
        v = GvrControllerInput.TouchPosCentered;
        float theta = Vector2.SignedAngle(refLine, v);
        text.text = theta.ToString();
        mRect.anchoredPosition = 80*v;
    }
}
