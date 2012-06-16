using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Mono.Nat;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviour
{
    const string Version = "1.1";

    public string ServerName;

    //public string WanIP;
    //public string LanIP;

    public bool IsClient { get; set; }
    public bool IsServer { get; set; }
    public const int Port = 10000;

    public string GameName;

    NetworkPeerType PeerType;
    bool waitingForClient;
    string errorMessage;
    public bool LocalMode;

    INatDevice natDevice;
    Mapping udpMapping, tcpMapping;

    public static NetworkBootstrap Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        //try
        //{
        //    LanIP = Dns.GetHostAddresses(Dns.GetHostName()).First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
        //}
        //catch (Exception ex)
        //{
        //    Debug.Log("Failed to get internal IP :\n" + ex.ToString());
        //    LanIP = "UNKNOWN";
        //}

        //WanIP = GetIP();
    }

    void Start()
    {
        NatUtility.DeviceFound += (s, ea) =>
        {
            natDevice = ea.Device;

            //string externalIp;
            //try
            //{
            //    externalIp = natDevice.GetExternalIP().ToString();
            //}
            //catch (Exception ex)
            //{
            //    Debug.Log("Failed to get external IP :\n" + ex.ToString());
            //    externalIp = "UNKNOWN";
            //}

            //if (WanIP == "UNKNOWN")
            //{
            //    Debug.Log("Reverted to UPnP device's external IP");
            //    WanIP = externalIp;
            //}
        };
        NatUtility.DeviceLost += (s, ea) => { natDevice = null; };

        NatUtility.StartDiscovery();
    }

    void OnApplicationQuit()
    {
        if (natDevice != null)
        {
            try
            {
                if (udpMapping != null)
                    natDevice.DeletePortMap(udpMapping);
                if (tcpMapping != null)
                    natDevice.DeletePortMap(tcpMapping);
                tcpMapping = udpMapping = null;
                Debug.Log("Deleted port mapping");
            }
            catch (Exception ex)
            {
                Debug.Log("Failed to delete port mapping");
            }
        }
        NatUtility.StopDiscovery();

        CloseServer();

        natDevice = null;
    }

    void Update()
    {
        if (GameFlow.State == GameState.ReadyToConnect)
        {
            if (LocalMode)
            {
                PeerType = NetworkPeerType.Disconnected;

                IsClient = false;
                IsServer = true;
                CreateServer();
                TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].IsFakeAI = true;
                TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].MarkReady();
                GameFlow.State = GameState.ReadyToPlay;
            }
            else if (IsServer)
            {
                IsClient = false;
                CreateServer();
                GameFlow.State = GameState.WaitingOrConnecting;
            }
            else
            {
                ConnectToServer();
                IsClient = true;
            }
        }
    }

    #region Server

    public void CloseServer()
    {
        if (Network.isServer)
        {
            Network.Disconnect();
            MasterServer.UnregisterHost();
        }
        IsServer = false;
    }

    static string GetIP()
    {
        try
        {
            string strIP;
            using (var wc = new WebClient())
            {
                strIP = wc.DownloadString("http://checkip.dyndns.org");
                strIP = (new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b")).Match(strIP).Value;
            }
            return strIP;
        }
        catch (Exception)
        {
            return "UNKNOWN";
        }
    }

    void MapPort()
    {
        try
        {
            Debug.Log("Mapping port...");

            udpMapping = new Mapping(Protocol.Udp, Port, Port) { Description = "Diluvium (UDP)" };
            natDevice.BeginCreatePortMap(udpMapping, state =>
            {
                if (state.IsCompleted)
                {
                    Debug.Log("UDP Mapping complete! Testing...");
                    try
                    {
                        var m = natDevice.GetSpecificMapping(Protocol.Udp, Port);
                        if (m == null)
                            throw new InvalidOperationException("Mapping not found");
                        if (m.PrivatePort != Port || m.PublicPort != Port)
                            throw new InvalidOperationException("Mapping invalid");

                        Debug.Log("Success!");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Failed to validate UDP mapping :\n" + ex.ToString());
                    }
                }
            }, null);

            tcpMapping = new Mapping(Protocol.Tcp, Port, Port) { Description = "Diluvium (TCP)" };
            natDevice.BeginCreatePortMap(tcpMapping, state =>
            {
                if (state.IsCompleted)
                {
                    Debug.Log("TCP Mapping complete! Testing...");
                    try
                    {
                        var m = natDevice.GetSpecificMapping(Protocol.Tcp, Port);
                        if (m == null)
                            throw new InvalidOperationException("Mapping not found");
                        if (m.PrivatePort != Port || m.PublicPort != Port)
                            throw new InvalidOperationException("Mapping invalid");

                        Debug.Log("Success!");
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("Failed to validate TCP mapping :\n" + ex.ToString());
                    }
                }
            }, null);
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to map port :\n" + ex.ToString());
        }
    }

    void CreateServer()
    {
        // Try UPnP port mapping nonetheless
        if (!Network.HavePublicAddress() && !LocalMode)
            MapPort();

        var result = Network.InitializeServer(2, Port, !Network.HavePublicAddress() && !LocalMode);
        if (result == NetworkConnectionError.NoError)
        {
            PeerType = NetworkPeerType.Server;
            if (!LocalMode)
                MasterServer.RegisterHost("Diluvium", GameName, "1.1");
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
    }

    #endregion

    #region Client

    void ConnectToServer()
    {
        Debug.Log("connection attempt will start");

        var isIp = (new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b")).IsMatch(GameName);

        if (isIp)
        {
            var result = Network.Connect(GameName, Port);
            if (result == NetworkConnectionError.NoError)
                PeerType = NetworkPeerType.Connecting;
            else
            {
                PeerType = NetworkPeerType.Disconnected;
                errorMessage = "Couldn't connect to server (reason : " + result + ") -- will retry in 2 seconds...";
                TaskManager.Instance.WaitFor(2).Then(() => GameFlow.State = GameState.ReadyToConnect);
                Debug.Log(errorMessage);
            }
        }
        else
        {
            MasterServer.RequestHostList("Diluvium");
            HostData chosenHost = null;

            foreach (var host in MasterServer.PollHostList().OrderBy(a => Guid.NewGuid()))
            {
                Debug.Log("Server found : " + " | game name : " + host.gameName + ", version = " + host.comment);
                if ((host.gameName == GameName || GameName == string.Empty) && host.comment == Version)
                {
                    Debug.Log("Found host");
                    chosenHost = host;
                    break;
                }
            }
            if (chosenHost == null)
            {
                errorMessage = "Couldn't connect to server (reason : not present in master server) -- will retry in 2 seconds...";
                TaskManager.Instance.WaitFor(2).Then(() => GameFlow.State = GameState.ReadyToConnect);
                Debug.Log(errorMessage);
            }
            else
            {
                chosenHost.useNat = true;
                var result = Network.Connect(chosenHost.guid);
                if (result == NetworkConnectionError.NoError)
                    PeerType = NetworkPeerType.Connecting;
                else
                {
                    PeerType = NetworkPeerType.Disconnected;
                    errorMessage = "Couldn't connect to server (reason : " + result + ") -- will retry in 2 seconds...";
                    TaskManager.Instance.WaitFor(2).Then(() => GameFlow.State = GameState.ReadyToConnect);
                    Debug.Log(errorMessage);
                }
            }
        }

        GameFlow.State = GameState.WaitingOrConnecting;
    }

    void OnFailedToConnect(NetworkConnectionError error)
    {
        if (IsServer) return; // Failed to connect to master server

        PeerType = NetworkPeerType.Disconnected;
        errorMessage = "Couldn't connect to server (reason : " + error + ") -- will retry in 2 seconds...";
        Debug.Log(errorMessage);

        TaskManager.Instance.WaitFor(2).Then(() => GameFlow.State = GameState.ReadyToConnect);
    }

    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Debug.Log("Could not connect to master server: " + info);
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
    }

    #endregion
}
