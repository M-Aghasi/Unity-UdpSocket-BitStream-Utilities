# Unity-UdpSocket-BitStream-Utilities
This project provides source codes and a UnityPackage for using c# udp sockets and bitwise memory streams.

## Udp sockets
Although Unity provides a networking api and protocol for socket programming and multiplayer game implementation, sometimes you need to establish your own bare sockets and communication protocol.

As at the time i didn't find a ready to use, clear and multiThread example of c# sockets usage in unity i started to write a simple one for udp communications.

There is a *UdpSocketManager* class in this package which is responsible for udp communications and has below methods:
* **UdpSocketManager(string serverIp, int serverPort)** receives serverIp & serverPort for further use
* **IEnumerator initSocket()** Resets UdpSocketManager state to default and starts Send & Receive threads
* **bool isInitializationCompleted()** Returns true if listen port has bound successfully
* **int getListenPort()** Returns current client UDP listen port
* **void send(byte[] data)** Adds an array of bytes to Queue for sending to server
* **IList<byte[]> receive()** Fetches received byte arrays from queue and return them as a list
* **void closeSocketThreads()** Closes Send & Receive threads and ends listening

Also there is a *UdpSocketManagerUsageExample* MonoBehavior script in the package which demonestrates a simple udp communication.
