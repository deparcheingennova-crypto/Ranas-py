using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("UDP")]
    public int port = 5053;
    public bool startReceiving = true;
    public bool printToConsole = false;

    [NonSerialized] public string data;

    private Thread _receiveThread;
    private UdpClient _client;
    private volatile bool _running; // guards the loop
    private readonly object _lock = new object();

    private void Awake()
    {
        // Singleton guard
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        StartReceiver();
    }

    private void Start()
    {
        // StartReceiver();
    }

    private void OnDisable()
    {
        StopReceiver();
    }

    private void OnApplicationQuit()
    {
        StopReceiver();
    }

    private void StartReceiver()
    {
        if (!startReceiving) return;

        // Prevent duplicate starts
        if (_receiveThread != null && _receiveThread.IsAlive) return;

        try
        {
            // Create UdpClient with ReuseAddress so Play Mode restarts don't collide
            _client = new UdpClient(AddressFamily.InterNetwork);
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // On Windows, also disable exclusive address use
            try { _client.Client.ExclusiveAddressUse = false; } catch { /* Not supported on all platforms */ }

            // Bind explicitly to Any:port
            var localEp = new IPEndPoint(IPAddress.Any, port);
            _client.Client.Bind(localEp);

            _running = true;
            _receiveThread = new Thread(ReceiveLoop) { IsBackground = true, Name = "UDP Receive Thread" };
            _receiveThread.Start();
        }
        catch (SocketException se)
        {
            Debug.LogError($"UDP bind failed on port {port}: {se.Message}");
            CleanupSocket();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to start receiver: {ex}");
            CleanupSocket();
        }
    }

    private void StopReceiver()
    {
        _running = false;

        // Closing the client will unblock Receive()
        try { _client?.Close(); } catch { /* ignore */ }

        // Join the thread (short timeout to avoid editor hang)
        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            if (!_receiveThread.Join(200))
            {
                // As a last resort, abort is discouraged; better to let it die naturally.
                Debug.LogWarning("UDP receive thread did not stop in time.");
            }
        }

        _receiveThread = null;
        CleanupSocket();
    }

    private void CleanupSocket()
    {
        try { _client?.Dispose(); } catch { /* ignore */ }
        _client = null;
    }

    private void ReceiveLoop()
    {
        var anyIP = new IPEndPoint(IPAddress.Any, 0);

        while (_running)
        {
            try
            {
                // This blocks until a datagram arrives or socket is closed
                byte[] dataBytes = _client.Receive(ref anyIP);

                string msg = Encoding.UTF8.GetString(dataBytes);
                lock (_lock)
                {
                    data = msg;
                }

                if (printToConsole)
                    Debug.Log(msg);
            }
            catch (ObjectDisposedException)
            {
                // Happens when _client.Close() is called; exit loop
                break;
            }
            catch (SocketException se)
            {
                // 10004/WSAEINTR or similar when closing; break out if we're stopping
                if (!_running) break;

                Debug.LogWarning($"UDP socket exception: {se.SocketErrorCode} - {se.Message}");
                // Small backoff to avoid tight error loop
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                Debug.LogError($"UDP receive error: {ex}");
                // Optional: decide whether to break or continue
                Thread.Sleep(10);
            }
        }
    }
}



//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using System.Threading;
//using System.Net.Sockets;
//using System.Net;
//using System.Text;
//
//public class DataManager : MonoBehaviour
//{
//    private Thread reciveThread;
//    private UdpClient client;
//    public int port = 5053;
//    public bool startRecieving = true;
//    public bool printToConsole = false;
//    public string data;
//
//    public static DataManager instance;
//
//    public void Start()
//    {
//        instance = this;
//
//        reciveThread = new Thread(new ThreadStart(ReciveData));
//        reciveThread.IsBackground = true;
//        reciveThread.Start();
//    }
//
//    private void ReciveData()
//    {
//       client = new UdpClient(port);
//            while (startRecieving)
//            {
//                try
//                {
//                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
//                    byte[] dataByte = client.Receive(ref anyIP);
//                    data = Encoding.UTF8.GetString(dataByte);
//
//                    if (printToConsole)
//                        print(data);
//                }
//                catch (Exception err)
//                {
//                    print(err.ToString());
//                }
//
//            }
//        
//    }
//}
//