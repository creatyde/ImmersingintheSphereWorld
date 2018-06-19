using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;

public class CameraStabilizer : MonoBehaviour
{

    TrackedPoseDriver driver;

    // Use this for initialization
    void Start()
    {
        driver = this.GetComponent<TrackedPoseDriver>(); // gets driver of camera
        driver.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        driver.transform.localRotation = Quaternion.identity;
    }
}
