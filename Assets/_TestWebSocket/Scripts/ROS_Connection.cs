using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Threading;
using System.Net.WebSockets;

namespace TestWebSocket
{
    public class ROS_Connection : MonoBehaviour
    {
        private Thread _socketThread;
        private volatile bool _isKeepReading = false;
        private Socket _listener;
        private Socket _handler;

        // Use this for initialization
        void Start()
        {
            Application.runInBackground = true;
            StartServer();
        }

        void StartServer()
        {
            _socketThread = new Thread(Connect);
            _socketThread.IsBackground = true;
            _socketThread.Start();
        }

        private string GetIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }

            }
            return localIP;
        }

        void Connect()
        {
            string data;

            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // host running the application.
            Debug.Log("Ip " + GetIPAddress().ToString());
            IPAddress[] ipArray = Dns.GetHostAddresses(GetIPAddress());
            IPEndPoint localEndPoint = new IPEndPoint(ipArray[0], 1755);

            // Create a TCP/IP socket.
            _listener = new Socket(ipArray[0].AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and 
            // listen for incoming connections.

            try
            {
                _listener.Bind(localEndPoint);
                _listener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    _isKeepReading = true;

                    // Program is suspended while waiting for an incoming connection.
                    Debug.Log("Waiting for Connection");     //It works

                    _handler = _listener.Accept();
                    Debug.Log("Client Connected");     //It doesn't work
                    data = null;

                    // An incoming connection needs to be processed.
                    while (_isKeepReading)
                    {
                        bytes = new byte[1024];
                        int bytesRec = _handler.Receive(bytes);
                        Debug.Log("Received from Server");

                        if (bytesRec <= 0)
                        {
                            _isKeepReading = false;
                            _handler.Disconnect(true);
                            break;
                        }

                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }

                        Thread.Sleep(1);
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        void StopServer()
        {
            _isKeepReading = false;

            //stop thread
            if (_socketThread != null)
            {
                _socketThread.Abort();
            }

            if (_handler != null && _handler.Connected)
            {
                _handler.Disconnect(false);
                Debug.Log("Disconnected!");
            }
        }

        void OnDisable()
        {
            StopServer();
        }
    }
}
