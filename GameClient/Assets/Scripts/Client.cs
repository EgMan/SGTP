// Code developed with help from the teachings of
// Tom Weiland's blog on unity integrated client-server architecture
// https://tomweiland.net/networking-tutorial-server-client-connection/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 12321;
    public int myId = 0;
    public TCP tcp;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("Instance already exists.  Destroying object.");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    public void ConnectToServer()
    {

        Debug.Log($"Attempting to connect to {instance.ip}/{instance.port}...");
        tcp.Connect();
    }

    public class TCP
    {
        TcpClient socket;
        private NetworkStream stream;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize,
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, OnTCPConnect, socket);
        }

        private void OnTCPConnect(IAsyncResult result)
        {
            socket.EndConnect(result);
            
            if (!socket.Connected)
            {
                return;
            }

            Debug.Log($"Succesfully connected to {instance.ip}/{instance.port}");

            stream = socket.GetStream();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, OnTCPReceive, null);
        }

        private void OnTCPReceive(IAsyncResult result)
        {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        // todo disconnect
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);
                    // todo handle data
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, OnTCPReceive, null);
                }
                catch(Exception e)
                {
                    Debug.Log($"Error receiving TCP data: {e}");
                    // todo disconnect
                }
        }
    }
}
