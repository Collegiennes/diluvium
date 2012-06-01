using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour 
{
    public string ServerIP = "192.168.0.2";
    public string WanIP;
    public string LanIP;
    public bool IsServer { get; set; }
    public const int Port = 10000;

    NetworkPeerType PeerType;
    bool waitingForClient;
    string errorMessage;

    public bool LocalMode;

    public static NetworkBootstrap Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        WanIP = GetIP();
        LanIP = Dns.GetHostAddresses(Dns.GetHostName()).First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
    }

    void Update()
    {
        if (GameFlow.State == GameState.ReadyToConnect)
        {
            if (ServerIP.Trim() == string.Empty)
                IsServer = true;

            if (IsServer)
            {
                CreateServer();
                GameFlow.State = GameState.WaitingOrConnecting;
            }
            else if (ServerIP == "LOCAL")
            {
                PeerType = NetworkPeerType.Disconnected;

                LocalMode = true;
                IsServer = true;

                CreateServer();
                TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].IsFakeAI = true;
                TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].MarkReady();
                GameFlow.State = GameState.ReadyToPlay;
            }
            else
            {
                ConnectToServer();
            }
        }
    }

    void OnGUI()
    {
        //switch (PeerType)
        //{
        //    case NetworkPeerType.Disconnected:
        //        GUILayout.Label("Disconnected : " + errorMessage);
        //        break;

        //    case NetworkPeerType.Connecting:
        //        GUILayout.Label("Connecting...");
        //        break;

        //    case NetworkPeerType.Server:
        //        if (waitingForClient && !LocalMode)
        //        {
        //            GUILayout.Label("Waiting for client...");
        //        }
        //        break;

        //    case NetworkPeerType.Client:
        //        // Nothing?
        //        break;
        //}
    }

    static string GetIP()
    {
        string strIP;
        using (var wc = new WebClient())
        {
            strIP = wc.DownloadString("http://checkip.dyndns.org");
            strIP = (new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b")).Match(strIP).Value;
        }
        return strIP;
    }

    #region Server

    void CreateServer()
    {
        var result = Network.InitializeServer(2, Port, false);
        if (result == NetworkConnectionError.NoError)
        {
            PeerType = NetworkPeerType.Server;
            waitingForClient = true;
        }
        else
        {
            PeerType = NetworkPeerType.Disconnected;
            errorMessage = "Couldn't create server (reason : " + result + ")";
            Debug.Log(errorMessage);
        }
    }

    void OnPlayerConnected(NetworkPlayer player) 
    {
        waitingForClient = false;
        GameFlow.State = GameState.ReadyToPlay;

        Debug.Log("Player connected : game can start!");
    }

    void OnPlayerDisconnected(NetworkPlayer player) 
    {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);

        waitingForClient = true;
        // TODO : make sure that the game ends? (done in each component?)

        Application.Quit();
    }

    #endregion

    #region Client

    void ConnectToServer()
    {
        Debug.Log("connection attempt will start");

        var result = Network.Connect(ServerIP, Port);
        if (result == NetworkConnectionError.NoError)
            PeerType = NetworkPeerType.Connecting;
        else
        {
            PeerType = NetworkPeerType.Disconnected;
            errorMessage = "Couldn't connect to server (reason : " + result + ") -- will retry in 2 seconds...";
            TaskManager.Instance.WaitFor(2).Then(() => GameFlow.State = GameState.ReadyToConnect);
            Debug.Log(errorMessage);
        }

        GameFlow.State = GameState.WaitingOrConnecting;
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        PeerType = NetworkPeerType.Disconnected;
        errorMessage = "Couldn't connect to server (reason : " + error + ") -- will retry in 2 seconds...";
        Debug.Log(errorMessage);

        TaskManager.Instance.WaitFor(2).Then(() => GameFlow.State = GameState.ReadyToConnect);
    }

    void OnConnectedToServer()
    {
        PeerType = NetworkPeerType.Client;
        GameFlow.State = GameState.ReadyToPlay;
        Debug.Log("Connection successful : game starting!");
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        PeerType = NetworkPeerType.Disconnected;
        errorMessage = "Disconnected from server (reason : " + info + ")";
        // TODO : prompt for reconnection?

        Application.Quit();
    }

    #endregion
}
