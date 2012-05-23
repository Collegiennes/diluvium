using System;
using System.Linq;
using UnityEngine;
using System.Collections;

public class Incantation : MonoBehaviour
{
    public Color errorTint = Color.red;
    public Color successTint = Color.white;
    public GUIStyle containerStyle;
    public GUIStyle textBoxStyle;
    public GUIStyle textStyle;
    public GUIStyle outlineStyle;
    public GUIStyle boxStyle;
    public Color hpGoodColor;
    public Color hpBadColor;
    public Color hpEnemyGoodColor;
    public Color hpEnemyBadColor;

    public Texture2D portraits;

    public AudioClip mistakeSound;
    public AudioClip enterSound;

    string text = "";

    public static Incantation Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void Restart()
    {
        text = string.Empty;
    }

    void ShowHealthBar(float amount, Color full, Color empty)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("HP", textStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label(Mathf.CeilToInt(amount) + "/" + Summoner.MaxHealth, textStyle);
        GUILayout.EndHorizontal();
        GUI.color = new Color(0, 0, 0, 0.75f);
        GUILayout.BeginHorizontal(outlineStyle);
        GUI.color = full;
        GUILayout.Box("", boxStyle, GUILayout.Width(360.0f * amount / Summoner.MaxHealth), GUILayout.Height(10));
        GUI.color = empty;
        GUILayout.Box("", boxStyle, GUILayout.Width(360 * (1 - (amount / Summoner.MaxHealth))), GUILayout.Height(10));
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
    }

    void OnGUI ()
    {
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 400 * CameraMotion.PanFactor, 0), Quaternion.identity, Vector3.one);

        var textureHeight = containerStyle.normal.background.height;

        GUILayout.BeginArea(new Rect(0, Screen.height - textureHeight, containerStyle.normal.background.width, textureHeight), containerStyle);
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(291, Screen.height - 140, 414-48, 162));
        GUILayout.BeginHorizontal(textBoxStyle);

        string[] words = text.Split(' ');
        for(int i = 0; i < words.Length; i++)
        {
            string word = words[i];

            if(AnimalDatabase.Get(word) == null)
                GUI.color = errorTint;
            else
                GUI.color = successTint;

            GUILayout.Label(word, textStyle);
            GUI.color = Color.white;

            if(i < words.Length-1)
                GUILayout.Space(11);
            else
                GUILayout.Label("|", textStyle);
        }

        var thisSummoner = TerrainGrid.Instance.Summoners[Network.isServer ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId];
        var enemySummoner = TerrainGrid.Instance.Summoners[Network.isServer ? TerrainGrid.ClientPlayerId : TerrainGrid.ServerPlayerId];

        GUILayout.EndHorizontal();
        {
            float health = thisSummoner.Health;
            ShowHealthBar(health, hpGoodColor, hpBadColor);
        }
        GUILayout.EndArea();

        GUI.matrix = Matrix4x4.TRS(new Vector3(0, -100 * CameraMotion.PanFactor, 0), Quaternion.identity, Vector3.one);

        GUILayout.BeginArea(new Rect(Screen.width-414+48-20, 20, 414-48, 162));
        {
            float health = enemySummoner.Health;
            ShowHealthBar(health, hpEnemyGoodColor, hpEnemyBadColor);
        }
        GUILayout.EndArea();

        GUI.matrix = Matrix4x4.TRS(new Vector3(0, 400 * CameraMotion.PanFactor, 0), Quaternion.identity, Vector3.one);

        //GUILayout.BeginArea(new Rect(300 - 48, Screen.height - 45, 410, 25));
        //GUILayout.BeginHorizontal(hpTextStyle);

        //GUILayout.EndHorizontal();
        //GUILayout.EndArea();

        // idle : 2/3f
        // hurt : 0/3f
        // fail : 1/3f

        var offset = thisSummoner.HasTakenDamage ? 0 : thisSummoner.HasFailed ? 1 / 3f : 2 / 3f;

        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - portraits.height, 512, portraits.height),
                                     portraits, new Rect(offset, 0, 1 / 3f, 1));

        GUI.matrix = Matrix4x4.identity;

        if (GameFlow.State == GameState.Gameplay)
        {
            // handle text entry
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (char.IsLetter(e.character) || (e.character == ' ' && words.Length < 3 && words.Length > 0 && words[0].Trim().Length > 0))
                {
                    text += char.ToUpper(e.character);
                }
                else if (e.character == '\n')
                {
                    var validWords = words.Where(x => AnimalDatabase.Get(x) != null).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();

                    foreach (var w in validWords.ToArray())
                    {
                        foreach (var t in TerrainGrid.Instance.Totems.Values.SelectMany(x => x))
                            if (t.AnimalData.Any(x => x.name.Equals(w, StringComparison.InvariantCultureIgnoreCase)))
                                validWords.Remove(w);
                    }

                    if (words.Length != validWords.Count)
                    {
                        thisSummoner.HasFailed = true;
                        TaskManager.Instance.WaitFor(0.5f).Then(() => { thisSummoner.HasFailed = false; });
                        audio.PlayOneShot(mistakeSound);
                    }
                    else if (validWords.Count > 0)
                    {
                        audio.PlayOneShot(enterSound);
                    }

                    if (validWords.Count > 0)
                        thisSummoner.TrySpawnOnServer(validWords.ToArray());

                    //foreach(string word in words)
                    //{
                    //    if(AnimalDatabase.Get(word) != null)
                    //    {
                    //        print("awesoem word: " + word);
                    //    }
                    //}
                    text = "";
                }
                else if (e.keyCode == KeyCode.Backspace && text.Length > 0)
                {
                    text = text.Remove(text.Length - 1);
                }
            }
        }
    }
}
