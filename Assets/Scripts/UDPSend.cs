// http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

public class UDPSend : MonoBehaviour
{

    public TextMesh textMesh;
    public GameObject target;
    private static int localPort;

    // prefs
    string IP = "XXX.XXX.XXX.XXX";  // define in init
    public int port = 8000;  // define in init

    // for connection
    IPEndPoint remoteEndPoint;
    UdpClient client;

    public void Start()
    {

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();
    }
    
    public void Update()
    {
        Vector3 angles = target.transform.localRotation.eulerAngles;
        string message = angles.x.ToString() + " " + angles.y.ToString() + " " + angles.z.ToString();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
        client.Send(data, data.Length, remoteEndPoint); // send data
        textMesh.text = message;
    }
}
