using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/**
 * This class wraps a C# UdpClient, creates two threads for send & receive
 * and provides methods for sending, receiving data and closing threads.
 */
public class UdpSocketManager {

    private readonly object _sendQueueLock = new object();
    private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();
    private readonly AutoResetEvent _sendQueueSignal = new AutoResetEvent(false);

    private readonly object _receiveQueueLock = new object();
    private readonly Queue<byte[]> _receiveQueue = new Queue<byte[]>();

    private Thread _receiveThread;
    private Thread _sendThread;

    private UdpClient _udpClient;
    private readonly object _udpClientLock = new object();

    private volatile int _listenPort = 0;
    private volatile bool _shouldRun = true;

    private IPEndPoint _localIpEndPoint = null;
    private readonly object _localIpEndPointLock = new object();

    private readonly string _serverIp;
    private readonly int _serverPort;
    private readonly int _clientPort;

    // this field is always used in _udpClientLock blocks, so it doesn't need a seperate lock
    private IAsyncResult _currentAsyncResult = null;





    public UdpSocketManager(string serverIp, int serverPort) {
        _serverIp = serverIp;
        _serverPort = serverPort;
        _clientPort = 0;
    }


    public UdpSocketManager(string serverIp, int serverPort, int clientPort) {
        _serverIp = serverIp;
        _serverPort = serverPort;
        _clientPort = clientPort;
    }


    /**
     * Resets SocketManager state to default and starts Send & Receive threads
     */
    public IEnumerator initSocket() {

        // check whether send & receive threads are alive, if so close them first
        if ((_sendThread != null && _sendThread.IsAlive) || (_receiveThread != null && _receiveThread.IsAlive)) {
            closeSocketThreads();
            while ((_sendThread != null && _sendThread.IsAlive) || (_receiveThread != null && _receiveThread.IsAlive)) {
                yield return null;
                // wait until udp threads closed
            }
        }

        // reset SocketManager state
        _sendQueue.Clear();
        _receiveQueue.Clear();
        _udpClient = null;
        _listenPort = 0;
        _shouldRun = true;

        // start Send & receive threads
        _receiveThread = new Thread(
            new ThreadStart(ReceiveThread));
        _receiveThread.IsBackground = true;
        _receiveThread.Start();

        _sendThread = new Thread(
            new ThreadStart(SendThread));
        _sendThread.IsBackground = true;
        _sendThread.Start();
    }


    /**
     * Adds an array of bytes to Queue for sending to server
     */
    public void send(byte[] data) {

        lock (_sendQueueLock) {
            _sendQueue.Enqueue(data);
        }
        _sendQueueSignal.Set();
    }


    /**
     * Reads received byte arrays from queue and return them as a list
     */
    public IList<byte[]> receive() {

        IList<byte[]> res = new List<byte[]>();
        lock (_receiveQueueLock) {
            while (_receiveQueue.Count > 0) {
                res.Add(_receiveQueue.Dequeue());
            }
        }
        return res;
    }

    /**
     * Returns current client UDP listen port
     */
    public int getListenPort() {
        return _listenPort;
    }

    /**
     * Returns true if listen port has bound successfully
     */
    public bool isInitializationCompleted() {
        return (_listenPort > 0);
    }

    /**
     * Closes Send & Receive threads and ends connection
     */
    public void closeSocketThreads() {
        _shouldRun = false;
        _sendQueueSignal.Set();

        lock (_udpClientLock) {
            if (_udpClient != null) {
                _udpClient.Close();
            }
        }
    }


    private void SendThread() {
        while (true) {
            bool isLocalIpSet;
            lock (_localIpEndPointLock) {
                isLocalIpSet = (_localIpEndPoint != null);
            }
            if (isLocalIpSet) {
                break;
            }
            Debug.Log("UnityUdpSockets, wait for connection establishment and getting client listen port.");
            Thread.Sleep(200);
        }
        lock (_localIpEndPointLock) {
            _listenPort = _localIpEndPoint.Port;
        }
        while (_shouldRun) {
            _sendQueueSignal.WaitOne();
            byte[] item = null;
            do {
                item = null;
                lock (_sendQueueLock) {
                    if (_sendQueue.Count > 0)
                        item = _sendQueue.Dequeue();
                }
                if (item != null) {
                    lock (_udpClientLock) {
                        _udpClient.Send(item, item.Length, _serverIp, _serverPort);
                    }
                }
            }
            while (item != null); // loop until there are items to collect
        }
    }


    // i putted UdpClient creation in a seperate thread because im not sure if Bind() method is non-blocking
    // and if Bind() is Blocking, it could block Unity's thread
    private void ReceiveThread() {
        lock (_udpClientLock) {
            _udpClient = new UdpClient();
            _udpClient.ExclusiveAddressUse = false;
            _udpClient.Client.SetSocketOption(
                SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, _clientPort);
            _udpClient.Client.Bind(localEp);
            var s = new UdpState(localEp, _udpClient);
            _currentAsyncResult = _udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), s);
            lock (_localIpEndPointLock) {
                _localIpEndPoint = ((IPEndPoint)_udpClient.Client.LocalEndPoint);
            }
        }
    }

    private void ReceiveCallback(IAsyncResult asyncResult) {
        lock (_udpClientLock) {
            if (asyncResult == _currentAsyncResult) {
                UdpClient uClient = ((UdpState)(asyncResult.AsyncState)).uClient;
                IPEndPoint ipEndPoint = ((UdpState)(asyncResult.AsyncState)).ipEndPoint;

                byte[] data = uClient.EndReceive(asyncResult, ref ipEndPoint);
                if (data != null && data.Length > 0) {
                    lock (_receiveQueueLock) {
                        _receiveQueue.Enqueue(data);
                    }
                }

                UdpState s = new UdpState(ipEndPoint, uClient);
                _currentAsyncResult = _udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), s);
            }
        }
    }

    private class UdpState {
        public UdpState(IPEndPoint ipEndPoint, UdpClient uClient) { this.ipEndPoint = ipEndPoint; this.uClient = uClient; }
        public IPEndPoint ipEndPoint;
        public UdpClient uClient;
    }
}
