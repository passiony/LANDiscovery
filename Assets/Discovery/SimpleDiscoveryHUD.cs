using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SimpleDiscoveryHUD : MonoBehaviour
{
    [SerializeField]
    private SimpleDiscovery discovery;

    [SerializeField]
    private string serverName = "My Server";

    private bool isServer = false;
    private bool isFinding = false;
    private List<DiscoveredServer> discoveredServers = new List<DiscoveredServer>();

    [System.Serializable]
    public class DiscoveredServer
    {
        public string ipAddress;
        public string serverData;
    }

    public UnityEvent<string, string> onFoundServer;

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 350, 500));
        GUILayout.BeginVertical("box");

        // Title
        GUILayout.Label("Network Discovery", new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(10);

        if (!isServer && !isFinding)
        {
            // Server Settings
            GUILayout.Label("Server Settings:");
            serverName = GUILayout.TextField(serverName, GUILayout.ExpandWidth(true));
            GUILayout.Space(5);

            // Start Server Button
            if (GUILayout.Button("Start Server", GUILayout.Height(30)))
            {
                StartServer();
            }

            GUILayout.Space(10);
            GUILayout.Label("Client Settings:");
            GUILayout.Space(5);

            // Find Servers Button
            if (GUILayout.Button("Find Servers", GUILayout.Height(30)))
            {
                FindServers();
            }
        }
        else if (isFinding)
        {
            GUILayout.Label("Finding servers...", new GUIStyle(GUI.skin.label) { fontSize = 16 });
            GUILayout.Space(10);
            GUILayout.Label("(Make sure firewall allows UDP port 47777)");

            if (GUILayout.Button("Cancel", GUILayout.Height(30)))
            {
                CancelFind();
            }
        }
        else if (isServer)
        {
            GUILayout.Label("Server Running:", new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(10);
            GUILayout.Label($"Server Name: {serverName}");
            GUILayout.Label("Listening for clients...");

            if (GUILayout.Button("Stop Server", GUILayout.Height(30)))
            {
                StopServer();
            }
        }

        // Display discovered servers
        if (discoveredServers.Count > 0)
        {
            GUILayout.Space(20);
            GUILayout.Label($"Discovered Servers ({discoveredServers.Count}):", new GUIStyle(GUI.skin.label) { fontSize = 16 });
            GUILayout.Space(5);

            foreach (var server in discoveredServers)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{server.serverData} ({server.ipAddress})");
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void StartServer()
    {
        if (discovery == null)
        {
            discovery = GetComponent<SimpleDiscovery>();
            if (discovery == null)
            {
                discovery = gameObject.AddComponent<SimpleDiscovery>();
            }
        }

        isServer = true;
        discovery.StartServer(serverName);
        Debug.Log("Server started with name: " + serverName);
    }

    private void StopServer()
    {
        isServer = false;
        Destroy(discovery);
        discovery = null;
        Debug.Log("Server stopped");
    }

    private void FindServers()
    {
        if (discovery == null)
        {
            discovery = GetComponent<SimpleDiscovery>();
            if (discovery == null)
            {
                discovery = gameObject.AddComponent<SimpleDiscovery>();
            }
        }

        isFinding = true;
        discoveredServers.Clear();
        discovery.FindServers((ip, data) => OnServerFound(ip, data));
        Debug.Log("Started finding servers...");
    }

    private void CancelFind()
    {
        isFinding = false;
        Destroy(discovery);
        discovery = null;
        Debug.Log("Cancelled finding servers");
    }

    private void OnServerFound(string ipAddress, string serverData)
    {
        isFinding = false;
        discoveredServers.Add(new DiscoveredServer { ipAddress = ipAddress, serverData = serverData });
        onFoundServer?.Invoke(ipAddress, serverData);
        Debug.Log("Found server: " + serverData + " at " + ipAddress);
    }

}