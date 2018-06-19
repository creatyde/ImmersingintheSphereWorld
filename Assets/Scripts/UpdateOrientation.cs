using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateOrientation : MonoBehaviour {

    public TextMesh textMesh;
    public GameObject target;
    public GameObject UDPRec;
    UDPReceive script;
    Quaternion data;

    public Quaternion GetRotation()
    {
        return data;
    }

    // Use this for initialization
    void Start () {
        script = UDPRec.GetComponent<UDPReceive>();
    }

	// Update is called once per frame
	void Update () {
        data = script.getQuat();
        target.transform.rotation = data;
        textMesh.text = data.ToString();
    }
}
