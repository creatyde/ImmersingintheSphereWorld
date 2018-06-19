// C#
// http://www.aidanlawrence.com/diy-arduino-unity-motion-controller/
// using some of the Bluetooth/Unity code

using UnityEngine;
using System.IO.Ports;
using System;
using TechTweaking.Bluetooth;
using MathNet.Numerics.LinearAlgebra;
using System.Collections;

public class StreamInput : MonoBehaviour
{
    float time;

    public Transform target; // the camera holder, item we want to affect with our accelerometer
    public TextMesh statusText; // the place we show the BT message    
    public GameObject sphereController;
    Spheres sphereScript;

    private BluetoothDevice device;
    private float checkTime;
    private int attempt;

    SerialPort sp;
    string[] stringDelimiters = new string[] { ":", "R", }; //items we want to ignore in strings.
    string macAddress = "XX:XX:XX:XX:XX:XX";

    // raw data
    string cmd;
    Vector<float> savedBuffer;
    bool start = true;
    float diffData = 0.3f; // accepted magnitude of difference in data

    // for rotation
    Quaternion rot;
    Quaternion upTrans; // apply to align physical world with Unity world
    Quaternion firstRec; // first received quaternion, inverted, from which to "zero" the world
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


    //** BLUETOOTH **//

    private void connect()
    {
        device.MacAddress = macAddress;
        device.Name = "HC-06";
        device.setEndByte(10);
        device.connect();
    }

    void HandleOnBluetoothStateChanged(bool isBtEnabled)
    {
        if (isBtEnabled)
        {
            connect();
            //We now don't need our receivers
            BluetoothAdapter.OnBluetoothStateChanged -= HandleOnBluetoothStateChanged;
            BluetoothAdapter.stopListenToBluetoothState();
        }
    }

    public void disconnect()
    {
        if (device != null)
            device.close();
    }

    private void tryConnect()
    {
        if (BluetoothAdapter.isBluetoothEnabled())
        {
            connect();
        }
        else
        {
            BluetoothAdapter.OnBluetoothStateChanged += HandleOnBluetoothStateChanged;
            BluetoothAdapter.listenToBluetoothState(); // if you want to listen to the following two events  OnBluetoothOFF or OnBluetoothON
            BluetoothAdapter.askEnableBluetooth(); // ask user to enable Bluetooth
        }
    }

    // Read the rotation command string and set "rot" if passes test
    void ParseAccelerometerDataG(string data)
    {
        try
        {
            string[] splitResult = data.Split(stringDelimiters, StringSplitOptions.RemoveEmptyEntries);

            float w = float.Parse(splitResult[0]);
            float x = float.Parse(splitResult[1]);
            float y = float.Parse(splitResult[2]);
            float z = float.Parse(splitResult[3]);

            // https://docs.unity3d.com/ScriptReference/Quaternion-ctor.html

            bool passed = false;
            // make vector
            Vector<float> temp = Vector<float>.Build.Dense(4);
            temp[0] = x; temp[1] = y; temp[2] = z; temp[3] = w;

            // check against last data entry
            if (start)
            {
                passed = true;
                start = false;
                firstRec = Quaternion.Inverse(new Quaternion(x, y, z, w)); // new inverted quaternion from the first
            }
            else
            {
                passed = Math.Abs((savedBuffer.Subtract(temp)).L2Norm()) < diffData;
            }
            savedBuffer = temp; // always save, for next round

            // passed test
            if (passed)
            {
                rot = new Quaternion(x, y, z, w);
            }
            else
            {
                statusText.text = "Problem";
            }

        }
        catch { Debug.Log("Malformed Serial Transmisison"); }
    }

    public string CheckForReceivedData()
    {
        string content = "";
        if (device.IsReading & device.IsDataAvailable)
        {
            byte[] msg = device.read();
            if (msg != null && msg.Length > 0)
            {
                content = System.Text.ASCIIEncoding.ASCII.GetString(msg);
            }
        }
        return content;
    }

    private void Awake()
    {
        device = new BluetoothDevice();
        tryConnect();

        sphereScript = sphereController.GetComponent<Spheres>();

        // up trans
        upTrans = Quaternion.LookRotation(Vector3.up); // look up

        // make sure target is zeroed
        target.localPosition = Vector3.zero;
        savedOrientation = Quaternion.identity;
        target.localRotation = upTrans * firstRec * savedOrientation;
    }

    void Update()
    {
        time += Time.deltaTime;
        // update moveSpeed, move in accordance with how big the world is
        ballRadius = sphereScript.GetViewerSize();
        moveSpeed = 1f * sphereScript.GetWorldSize();

        if (!device.IsReading)
        {
            attempt++;
            tryConnect();
        }

        cmd = CheckForReceivedData();
        if (cmd.StartsWith("R"))
        { //Got a rotation command
            // save raw rotation in rot
            ParseAccelerometerDataG(cmd);
        }

        // set new rotation (angular velocity)
        tempRot = target.localRotation; // save old rotation
        savedOrientation = Quaternion.Slerp(savedOrientation, rot, Time.deltaTime * 2f);
        target.localRotation = upTrans * firstRec * savedOrientation;

        // applying translation
        diff = tempRot * Quaternion.Inverse(target.localRotation); // difference in rotation
        diff.ToAngleAxis(out rotAngle, out rotAxis); // calculate how sphere moved
        // rotAxis *= -1;

        // find vector orthogonal to up and rotation-axis vector (which means it never travels along Z-axis this way)
        transDir = Vector3.Cross(rotAxis, upDir).normalized;
        // compute magnitude of translation = angular fraction * circumference
        transDir *= rotAngle / (360f) * (2f * Mathf.PI * ballRadius);
        target.localPosition += transDir * Time.deltaTime * moveSpeed;

        statusText.text = rot + "\n" + target.localPosition + "\n" + target.localRotation.eulerAngles;
    }
}
