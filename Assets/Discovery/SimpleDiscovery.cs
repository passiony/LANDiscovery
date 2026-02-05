using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class SimpleDiscovery : MonoBehaviour
{
    private UdpClient udpClient;
    private const int Port = 47777; // 自定义端口
    private const string HandshakeHeader = "NGO_DISCOVERY_PING";

    #if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject multicastLock;
    #endif

    // Server: 开启监听
    public async void StartServer(string serverData)
    {
        AcquireMulticastLock();
        udpClient = new UdpClient(Port);
        Debug.Log("Discovery Server Started...");

        while (udpClient != null)
        {
            var result = await udpClient.ReceiveAsync();
            string message = Encoding.UTF8.GetString(result.Buffer);

            if (message == HandshakeHeader)
            {
                byte[] response = Encoding.UTF8.GetBytes("NGO_DISCOVERY_PONG|" + serverData);
                await udpClient.SendAsync(response, response.Length, result.RemoteEndPoint);
            }
        }
    }

    // Client: 寻找 Server
    public async void FindServers(Action<string, string> onFound)
    {
        AcquireMulticastLock();
        UdpClient client = new UdpClient();
        client.EnableBroadcast = true;

        byte[] request = Encoding.UTF8.GetBytes(HandshakeHeader);
        await client.SendAsync(request, request.Length, new IPEndPoint(IPAddress.Broadcast, Port));

        // 监听回包
        while (true)
        {
            var result = await client.ReceiveAsync();
            string response = Encoding.UTF8.GetString(result.Buffer);
            if (response.StartsWith("NGO_DISCOVERY_PONG"))
            {
                string data = response.Split('|')[1];
                onFound?.Invoke(result.RemoteEndPoint.Address.ToString(), data);
                break; 
            }
        }
    }

    private void AcquireMulticastLock()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
        {
            multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "ngo_lock");
            multicastLock.Call("acquire");
        }
        #endif
    }

    private void OnDestroy()
    {
        udpClient?.Close();
        #if UNITY_ANDROID && !UNITY_EDITOR
        multicastLock?.Call("release");
        #endif
    }
}