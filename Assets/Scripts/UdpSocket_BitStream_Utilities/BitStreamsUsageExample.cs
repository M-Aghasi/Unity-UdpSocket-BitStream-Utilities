using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BitStreamsUsageExample : MonoBehaviour {

    private UdpSocketManager _udpSocketManager;
    private bool _isListenPortLogged = false;

    void Start() {
        _udpSocketManager = new UdpSocketManager("127.0.0.1", 55056);
        StartCoroutine(_udpSocketManager.initSocket());
        StartCoroutine(sendAndReceiveStream());

    }

    IEnumerator sendAndReceiveStream() {
        while (!_udpSocketManager.isInitializationCompleted()) {
            yield return null;
        }

        if (UdpSocketManagerUsageExample.isActive) {
            Debug.LogWarning("UdpSocketManagerUsageExample and BitStreamsUsageExample scripts couldn't be used concurrently!");
            yield break;
        }

        if (!_isListenPortLogged) {
            Debug.Log("UdpSocketManager, listen port: " + _udpSocketManager.getListenPort());
            _isListenPortLogged = true;
        }

        BitwiseMemoryOutputStream outStream = new BitwiseMemoryOutputStream();
        outStream.writeBool(true);
        outStream.writeByte(0xfa);
        outStream.writeDouble(1.2);
        outStream.writeFloat(81.12f);
        outStream.writeInt(7, 3);
        outStream.writeLong(8, 4);
        outStream.writeSigned(-7, 3);
        outStream.writeSignedLong(-8, 4);
        outStream.writeString("Hello World!");
        Debug.Log("UdpSocketManager, stream have sent!");

        _udpSocketManager.send(outStream.getBuffer());

        IList<byte[]> recPackets = _udpSocketManager.receive();

        while (recPackets.Count < 1) {
            yield return null;
            recPackets = _udpSocketManager.receive();
        }

        byte[] echoPacket = recPackets[0];

        BitwiseMemoryInputStream inStream = new BitwiseMemoryInputStream(echoPacket);
        Debug.Assert(inStream.readBool() == true);
        Debug.Assert(inStream.readByte() == 0xfa);
        Debug.Assert(inStream.readDouble() == 1.2);
        Debug.Assert(inStream.readFloat() == 81.12f);
        Debug.Assert(inStream.readInt(3) == 7);
        Debug.Assert(inStream.readLong(4) == 8);
        Debug.Assert(inStream.readSignedInt(3) == -7);
        Debug.Assert(inStream.readSignedLong(4) == -8);
        Debug.Assert(inStream.readString() == "Hello World!");
        Debug.Log("UdpSocketManager, stream have received!");
    }

    private void OnDestroy() {
        if(_udpSocketManager != null) {
            _udpSocketManager.closeSocketThreads();
        }
    }
}
