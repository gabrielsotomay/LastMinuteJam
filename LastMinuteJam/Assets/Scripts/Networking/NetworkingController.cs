using Netick.Unity;
using NUnit.Framework;
using StinkySteak.NShooter.Netick.Transport;
using UnityEngine;
using System.Collections.Generic;

public class NetworkingController : NetworkEventsListener
{
    public static NetworkingController Instance;
    public GameObject SandboxPrefab;
    public NetworkTransportProvider Transport;
    public StartMode Mode = StartMode.MultiplePeers;
    [UnityEngine.Range(1, 5)]
    public int Clients = 4;
    public bool StartServerInMultiplePeersMode = true;

    public bool AutoStart;
    public bool AutoConnect;

    [Header("Network")]
    [UnityEngine.Range(0, 65535)]
    public int Port;
    public string ServerIPAddress = "127.0.0.1";

    [Header("Headless Server FPS")]
    public bool Cap = true;
    public int FPS = 450;

    [Header("UI")]
    public bool ShowDisconnectButton = true;
    public bool ShowConnectButton = true;
    public Vector2 Offset = new Vector2(36, 0);
    public List<PlayerData> playerData = new();
    public string myName = "";
    public string mapName = "";
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }



    public void StartHost()
    {        
        Network.StartAsHost(Transport, NetickUnityTransport.Allocation.ServerEndpoints[0].Port, SandboxPrefab);
    }
    public void StartClient()
    {
        var sandbox = Network.StartAsClient(Transport, NetickUnityTransport.JoinAllocation.ServerEndpoints[0].Port, SandboxPrefab);
        sandbox.Connect(NetickUnityTransport.JoinAllocation.ServerEndpoints[0].Port, NetickUnityTransport.JoinAllocation.ServerEndpoints[0].Host); 
    }
}
