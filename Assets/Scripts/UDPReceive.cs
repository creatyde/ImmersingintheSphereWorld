// http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC

using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{

    // receiving thread
    Thread receiveThread;

    // udpclient object
    UdpClient client;

    // public
    string IP = "XXX.XXX.XXX.XXX";
    public int port = 8000;

    // data
    string message;
    Quaternion rot;

    // start from unity3d
    public void Start()
    {
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }
    
    // abort the thread
    void OnApplicationQuit()
    {
        receiveThread.Abort();
        if (client != null)
            client.Close();
    }

    // receive thread
    private void ReceiveData() {
        client = new UdpClient(port);
        // set source
        IPAddress ip;
        IPEndPoint anyIP; // IP used
        if (IPAddress.TryParse(IP, out ip)) // ip has parsed data
        {
            anyIP = new IPEndPoint(ip, port);
        }
        else
        {
            anyIP = new IPEndPoint(IPAddress.Broadcast, port);
        }
        while (true)
        {
            try {
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                message = text;
                string[] breakString = text.Split();
                Vector3 newAngles = new Vector3(
                    System.Convert.ToSingle(breakString[0]),
                    System.Convert.ToSingle(breakString[1]),
                    System.Convert.ToSingle(breakString[2]));
                rot = Quaternion.Euler(newAngles);
                Debug.Log("Get: " + rot);
            }
            catch (Exception err)
            {
                    message = err.ToString();
            }
        }
    }

    // get quat
    public Quaternion getQuat()
    {
        return rot;
    }
}
