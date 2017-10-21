using UnityEngine;
using System.Text;

public class UdpSocketManagerUsageExample : MonoBehaviour {

    public static bool isActive = false;

    private UdpSocketManager _udpSocketManager;
    private bool _isListenPortLogged = false;

    void Start() {
        isActive = true;
        _udpSocketManager = new UdpSocketManager("127.0.0.1", 55056);
        StartCoroutine(_udpSocketManager.initSocket());
    }

    // Update is called once per frame
    void Update() {
        if (!_udpSocketManager.isInitializationCompleted()) {
            return;
        }
        if(!_isListenPortLogged) {
            Debug.Log("UdpSocketManager, listen port: " + _udpSocketManager.getListenPort());
            _isListenPortLogged = true;
        }
        foreach (byte[] recPacket in _udpSocketManager.receive()) {
            string receivedMsg = Encoding.UTF8.GetString(recPacket);
            if(receivedMsg == "Tik") {
                _udpSocketManager.send(Encoding.UTF8.GetBytes("Taak"));
                Debug.Log("UdpSocketManager, Tik have received and Taak have sent");
            }
        }
    }

    private void OnDestroy() {
        if (_udpSocketManager != null) {
            _udpSocketManager.closeSocketThreads();
        }
    }
}
