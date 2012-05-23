using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour 
{
    public string ServerIP = "192.168.0.1";
    public string ClientIP = "192.168.0.2";
    public const int Port = 10000;

    NetworkPeerType PeerType;
    bool waitingForClient;
    string errorMessage;
    string IP;

    public bool LocalMode;

    void Awake()
    {
        IP = Dns.GetHostAddresses(Dns.GetHostName()).First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
        Debug.Log("Current IP is : " + IP);

        if (IP == ServerIP)
            CreateServer();
        else if (IP == ClientIP)
            ConnectToServer();
        else
        {
            PeerType = NetworkPeerType.Disconnected;
            errorMessage = "IP address invalid : " + IP + "; starting local mode";

            LocalMode = true;

            CreateServer();
            TimeKeeper.Instance.audio.Play();
            TaskManager.Instance.WaitUntil(_ => TerrainGrid.Instance.Summoners.ContainsKey(TerrainGrid.ClientPlayerId))
                .Then(() => { TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].IsFakeAI = true; });
        }
    }

    void OnGUI()
    {
        switch (PeerType)
        {
            case NetworkPeerType.Disconnected:
                GUILayout.Label("Disconnected : " + errorMessage);
                break;

            case NetworkPeerType.Connecting:
                GUILayout.Label("Connecting...");
                break;

            case NetworkPeerType.Server:
                if (waitingForClient && !LocalMode)
                {
                    GUILayout.Label("Waiting for client...");
                }
                break;

            case NetworkPeerType.Client:
                // Nothing?
                break;
        }
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
        }
    }

    void OnPlayerConnected(NetworkPlayer player) 
    {
        waitingForClient = false;
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
        var result = Network.Connect(ServerIP, Port);
        if (result == NetworkConnectionError.NoError)
            PeerType = NetworkPeerType.Connecting;
        else
        {
            PeerType = NetworkPeerType.Disconnected;
            errorMessage = "Couldn't connect to server (reason : " + result + ")";
        }
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        PeerType = NetworkPeerType.Disconnected;
        errorMessage = "Couldn't connect to server (reason : " + error + ") -- will retry in 3 seconds...";

        TaskManager.Instance.WaitFor(3).Then(ConnectToServer);
    }

    void OnConnectedToServer()
    {
        PeerType = NetworkPeerType.Client;
    }

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        PeerType = NetworkPeerType.Disconnected;
        errorMessage = "Disconnected from server (reason : " + info;
        // TODO : prompt for reconnection?

        Application.Quit();
    }

    #endregion
}
