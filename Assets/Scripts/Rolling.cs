using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rolling : MonoBehaviour {

    public Transform target;
    public Transform ghost; // object on which to base rotations
    public GameObject sphereController;
    Spheres sphereScript;

    Vector3 axis = -Vector3.up; // axis of rotation

    // for rotation
    Quaternion rot;
    Quaternion upTrans; // apply to align physical world with Unity world
    Quaternion savedOrientation;
    // for translation
    Quaternion tempRot;
    float rotAngle; // angle of rotation, in DEGREES
    Vector3 rotAxis; // axis of rotation
    Quaternion diff;
    Vector3 upDir = Vector3.up;
    Vector3 transDir;
    float ballRadius = 1f;
    float moveSpeed = 5f;

    // Use this for initialization
    void Start () {

        sphereScript = sphereController.GetComponent<Spheres>();

        // up trans
        upTrans = Quaternion.LookRotation(Vector3.up); // look up

        // make sure target is zeroed
        target.localPosition = Vector3.zero;
        savedOrientation = Quaternion.identity; // init
        target.localRotation = upTrans * savedOrientation;

    }
	
	// Update is called once per frame
	void Update () {

        ghost.Rotate(axis * Time.deltaTime);

        rot = ghost.rotation;

        // how rotate
        ballRadius = sphereScript.GetViewerSize();
        moveSpeed = sphereScript.GetWorldSize();

        savedOrientation = Quaternion.Slerp(savedOrientation, rot, Time.deltaTime * 2f);
        target.localRotation = upTrans * savedOrientation;

        target.localPosition += ballRadius * moveSpeed * Vector3.forward * Time.deltaTime / 1000f; 
    }
}
