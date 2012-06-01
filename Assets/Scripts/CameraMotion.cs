using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;

public class CameraMotion : MonoBehaviour
{
    const float TransitionTime = 0.4f;
    const float ShowTime = 1.5f;

    public GameObject Credits, Logo, Tutorial, Tutorial2;

    public static float PanFactor = 1;

    public GUIStyle textBoxStyle, labelStyle;

    int showing;
    Vector3 origin;
    GameObject[] order;
    float time = -1;
    bool stop = true;
    bool enterPressed;

    void Awake()
    {
        transform.localRotation = Quaternion.Euler(0, 45, 0);
        origin = transform.position;
        Restart();
        showing = 0;
        Tutorial.renderer.material.mainTextureOffset = new Vector2(0, 0.75f);
    }

	void Start()
    {
        order = new[] { Credits, Logo, Tutorial, Tutorial2 };
	}

    void Restart()
    {
        PanFactor = 1;
        time = -0.6f;
        stop = true;
        showing = 3; // On restart, start at the wait/sync phase
        transform.position = origin;

        Tutorial.renderer.material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 0));
        Tutorial.renderer.material.mainTextureOffset = new Vector2(0, 0.5f);
    }

    void OnServerInitialized()
    {
        //Debug.Log("server init");
        TaskManager.Instance.WaitUntil(_ => TerrainGrid.Instance.Summoners.Count == 2).Then(() =>
        {
            TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].Die += () => { GameFlow.State = GameState.Won; time = 0; foreach (var s in TerrainGrid.Instance.Summoners.Values) s.IsReady = false; };
            TerrainGrid.Instance.Summoners[TerrainGrid.ServerPlayerId].Die += () => { GameFlow.State = GameState.Lost; time = 0; foreach (var s in TerrainGrid.Instance.Summoners.Values) s.IsReady = false; };
        });
    }
    void OnConnectedToServer()
    {
        transform.localRotation = Quaternion.Euler(0, -45, 0);
        TaskManager.Instance.WaitUntil(_ => TerrainGrid.Instance.Summoners.Count == 2).Then(() =>
        {
            TerrainGrid.Instance.Summoners[TerrainGrid.ServerPlayerId].Die += () => { GameFlow.State = GameState.Won; time = 0; foreach (var s in TerrainGrid.Instance.Summoners.Values) s.IsReady = false; };
            TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].Die += () => { GameFlow.State = GameState.Lost; time = 0; foreach (var s in TerrainGrid.Instance.Summoners.Values) s.IsReady = false; };
        });
    }

	void Update()
	{
        if (GameFlow.State == GameState.Won)
        {
            Tutorial.renderer.material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 1));
            Tutorial.renderer.material.mainTextureOffset = new Vector2(0, 0.25f);

            if (Input.GetKeyDown(KeyCode.Return))
            {
                Restart();
                GameFlow.Instance.Restart();
                return;
            }
        }

        if (GameFlow.State == GameState.Lost)
        {
            Tutorial.renderer.material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, 1));
            Tutorial.renderer.material.mainTextureOffset = new Vector2(0, 0);

            if (Input.GetKeyDown(KeyCode.Return))
            {
                Restart();
                GameFlow.Instance.Restart();
                return;
            }
        }

        if (showing <= 3)
        {
            if (showing >= 2)
            {
                if (stop)
                {
                    if (time < TransitionTime)
                        time += Time.deltaTime;

                    if (showing == 2 || GameFlow.State == GameState.ReadyToPlay)
                    {
                        stop &= !Input.GetKeyDown(KeyCode.Return) && !enterPressed;
                        enterPressed = false;
                    }
                    if (!stop)
                    {
                        time = TransitionTime + ShowTime + TransitionTime / 2;
                        if (showing == 3)
                        {
                            TerrainGrid.Instance.Summoners[NetworkBootstrap.Instance.IsServer ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId].TellReady();
                            GameFlow.State = GameState.Syncing;
                        }
                    }
                }
                else
                {
                    if (showing == 2 || GameFlow.State == GameState.Gameplay)
                        time += Time.deltaTime;
                }
            }
            else 
            {
                if (showing != 1 || time < TransitionTime || GameFlow.State >= GameState.ReadyToConnect)
                    time += Time.deltaTime;

                if (showing != 1 && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                    time = TransitionTime * 2 + ShowTime;
            }

            var step = time < (TransitionTime + ShowTime)
                           ? Mathf.Clamp01(time / TransitionTime)
                           : Mathf.Clamp01(1 - ((time - (ShowTime + TransitionTime)) / TransitionTime));
            order[showing].renderer.material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, step));

            if (time >= TransitionTime * 2 + ShowTime)
            {
                showing++;
                time = 0;

                if (showing == 1)
                {
                    GameFlow.State = GameState.Login;
                }
                if (showing == 3)
                {
                    Tutorial2.renderer.material.mainTextureOffset = new Vector2(0, 0.5f);
                    stop = true;
                }
            }
        }

        if (GameFlow.State == GameState.Syncing &&
            TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].IsReady && TerrainGrid.Instance.Summoners[TerrainGrid.ServerPlayerId].IsReady)
        {
            Debug.Log("both are ready, gameplay time");
            TaskManager.Instance.WaitFor(1 / 60f).Then(() => GameFlow.State = GameState.Gameplay);
        }

        if (showing >= 4 && GameFlow.State >= GameState.Gameplay)
        {
            time += Time.deltaTime;

            float t = Mathf.Pow(0.5f, Time.deltaTime);
            transform.position = t * transform.position + (1 - t) * FindSceneCenter();

            PanFactor = 1 - Easing.EaseOut(Mathf.Clamp01(time / 3), EasingType.Quadratic);
            if (GameFlow.State == GameState.Won || GameFlow.State == GameState.Lost)
                PanFactor = 1 - PanFactor;
        }
	}

    void OnGUI()
    {
        if (showing == 1)
        {
            // Detect return key
            Event e = Event.current;
            if (e.type == EventType.KeyDown && time > TransitionTime / 4 && GameFlow.State == GameState.Login)
            {
                var isIP = Regex.IsMatch(NetworkBootstrap.Instance.ServerIP, @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}");

                if (e.character == '\n' && (isIP || NetworkBootstrap.Instance.ServerIP.Length == 0 || NetworkBootstrap.Instance.ServerIP == "LOCAL"))
                {
                    time = TransitionTime + ShowTime;

                    Debug.Log("ready to connect");
                    GameFlow.State = GameState.ReadyToConnect;

                    GUI.SetNextControlName("FOCUS_REMOVER");
                    GUI.Label(new Rect(-100, -100, 1, 1), "");
                    GUI.FocusControl("FOCUS_REMOVER");
                    return;
                }
            }

            var step = time < (TransitionTime + ShowTime)
               ? Mathf.Clamp01(time / TransitionTime)
               : Mathf.Clamp01(1 - ((time - (ShowTime + TransitionTime)) / TransitionTime));
            GUI.color = new Color(1f, 1, 1, step);

            var logoBottom = Screen.height * 0.7f;
            var boxLeft = Screen.width / 2f - 223 / 2 - 10 / 2;

            GUI.SetNextControlName("IP Box");
            NetworkBootstrap.Instance.ServerIP = GUI.TextField(new Rect(boxLeft, logoBottom + 50, 223 + 10, 42 + 10), NetworkBootstrap.Instance.ServerIP, 15, textBoxStyle);
            NetworkBootstrap.Instance.ServerIP = Regex.Replace(NetworkBootstrap.Instance.ServerIP, @"\s+", "").ToUpper();

            if (NetworkBootstrap.Instance.ServerIP.Length == 0)
            {
                var bigText = "HOSTING";
                var smallText = "LOCAL " + NetworkBootstrap.Instance.LanIP + "\nINTERNET " + NetworkBootstrap.Instance.WanIP;
                labelStyle.fontSize = 23;
                var size = labelStyle.CalcSize(new GUIContent(bigText));
                labelStyle.normal.textColor = new Color(0.5f, 1, 0, step);
                labelStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(boxLeft, logoBottom + 108, size.x, size.y), bigText, labelStyle);
                labelStyle.normal.textColor = new Color(1f, 1, 1, step);
                labelStyle.fontSize = 12;
                GUI.Label(new Rect(boxLeft + size.x + 8, logoBottom + 108, 200, 200), smallText, labelStyle);
            }
            else if (Regex.IsMatch(NetworkBootstrap.Instance.ServerIP, @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}"))
            {
                var bigText = "CONNECT";
                var smallText = "ENTER TO CONNECT TO\n" + NetworkBootstrap.Instance.ServerIP;
                labelStyle.fontSize = 23;
                var size = labelStyle.CalcSize(new GUIContent(bigText));
                labelStyle.normal.textColor = new Color(0.5f, 1, 0, step);
                labelStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(boxLeft, logoBottom + 108, size.x, size.y), bigText, labelStyle);
                labelStyle.normal.textColor = new Color(1f, 1, 1, step);
                labelStyle.fontSize = 12;
                GUI.Label(new Rect(boxLeft + size.x + 8, logoBottom + 108, 200, 200), smallText, labelStyle);
            }
            else
            {
                var bigText = "TYPE";
                var smallText = "'LOCAL' FOR SINGLE PLAYER\nAN IP ADDRESS FOR MULTIPLAYER";
                labelStyle.fontSize = 23;
                var size = labelStyle.CalcSize(new GUIContent(bigText));
                labelStyle.normal.textColor = new Color(1, 0, 0, step);
                labelStyle.alignment = TextAnchor.UpperLeft;
                GUI.Label(new Rect(boxLeft, logoBottom + 108, size.x, size.y), bigText, labelStyle);
                labelStyle.normal.textColor = new Color(1f, 1, 1, step);
                labelStyle.fontSize = 12;
                GUI.Label(new Rect(boxLeft + size.x + 8, logoBottom + 108, 200, 200), smallText, labelStyle);
            }

            GUI.color = Color.white;

            if (GameFlow.State == GameState.Login)
                GUI.FocusControl("IP Box");
        }

        if (showing == 2)
        {
            enterPressed = Event.current.character == '\n';
        }

        if (showing == 3)
        {
            var step = time < (TransitionTime + ShowTime)
               ? Mathf.Clamp01(time / TransitionTime)
               : Mathf.Clamp01(1 - ((time - (ShowTime + TransitionTime + TransitionTime / 2)) / (TransitionTime / 2)));
            GUI.color = new Color(1f, 1, 1, step);

            var aspectRatio = (float)Screen.width / Screen.height;

            var top = Screen.height * 0.86f;
            var left = Screen.width / 2f - (0.3335f / aspectRatio * (16 / 10f)) * Screen.width;

            string text = "";
            if (GameFlow.State == GameState.WaitingOrConnecting)
                text = NetworkBootstrap.Instance.IsServer ? "WAITING FOR PLAYER TO CONNECT..." : "CONNECTING TO OTHER PLAYER...";
            else if (GameFlow.State == GameState.Syncing)
                text = "WAITING FOR OTHER PLAYER TO START...";
            else if (GameFlow.State == GameState.ReadyToPlay)
                text = "PRESS ENTER TO BEGIN";

            labelStyle.fontSize = (int) Math.Round(24 / 800f * Screen.height);
            labelStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f, step);
            labelStyle.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(left, top, 500, 100), text, labelStyle);
        }
    }

    Vector3 FindSceneCenter()
    {
        if (GameFlow.State == GameState.Won || GameFlow.State == GameState.Lost) return origin;

        Vector3 minPos = new Vector3(100, 100, 100);
        Vector3 maxPos = new Vector3(0, 0, 0);

        foreach (GridCell c in TerrainGrid.Instance.Cells)
        {
            if (c.Occupant != null)
            {
                Vector3 pos = new Vector3(c.X + 0.5f, c.Height, c.Z + 0.5f);
                minPos = Vector3.Min(minPos, pos);
                maxPos = Vector3.Max(maxPos, pos);
            }
        }

        return (minPos + maxPos) / 2;
    }
}
