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
    public GUIStyle boxStyle;
    public Color hpGoodColor;
    public Color hpBadColor;
    public Color hpEnemyGoodColor;
    public Color hpEnemyBadColor;

    public Texture2D portraits;

    string text = "";

    public static Incantation Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void ShowHealthBar(float amount, Color full, Color empty)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("HP", textStyle);
        GUILayout.FlexibleSpace();
        GUILayout.Label(Mathf.CeilToInt(amount) + "/20", textStyle);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.Height(10));
        GUI.color = full;
        GUILayout.Box("", boxStyle, GUILayout.Width(366*amount/20.0f));
        GUI.color = empty;
        GUILayout.Box("", boxStyle, GUILayout.Width(366*(1-(amount/20.0f))));
        GUI.color = Color.white;
        GUILayout.EndHorizontal();
    }

    void OnGUI ()
    {
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

        GUILayout.EndHorizontal();

        {
            int playerId = Network.isServer ?
                TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId;
            float health = TerrainGrid.Instance.Summoners[playerId].Health;
            ShowHealthBar(health, hpGoodColor, hpBadColor);
        }
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(Screen.width-414+48-20, 20, 414-48, 162));
        {
            int playerId = !Network.isServer ?
                TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId;
            float health = TerrainGrid.Instance.Summoners[playerId].Health;
            ShowHealthBar(health, hpEnemyGoodColor, hpEnemyBadColor);
        }
        GUILayout.EndArea();

        //GUILayout.BeginArea(new Rect(300 - 48, Screen.height - 45, 410, 25));
        //GUILayout.BeginHorizontal(hpTextStyle);

        //GUILayout.EndHorizontal();
        //GUILayout.EndArea();

        // idle : 2/3f
        // hurt : 0/3f
        // fail : 1/3f

        var offset = 2 / 3f;

        GUI.DrawTextureWithTexCoords(new Rect(0, Screen.height - portraits.height, 512, portraits.height),
                                     portraits, new Rect(offset, 0, 1 / 3f, 1));

        // handle text entry
        Event e = Event.current;
        if(e.type == EventType.KeyDown)
        {
            if(char.IsLetter(e.character) || (e.character == ' ' && words.Length < 3))
                text += char.ToUpper(e.character);
            else if(e.character == '\n')
            {
                var validWords = words.Where(x => AnimalDatabase.Get(x) != null).ToArray();

                if (validWords.Length > 0)
                {
                    if (Network.isServer)
                        TerrainGrid.Instance.Summoners[TerrainGrid.ServerPlayerId].TrySpawn(validWords);
                    else
                        TerrainGrid.Instance.Summoners[TerrainGrid.ClientPlayerId].TrySpawn(validWords);
                }

                //foreach(string word in words)
                //{
                //    if(AnimalDatabase.Get(word) != null)
                //    {
                //        print("awesoem word: " + word);
                //    }
                //}
                text = "";
            }
            else if(e.keyCode == KeyCode.Backspace && text.Length > 0)
            {
                text = text.Remove(text.Length-1);
            }
        }
    }
}
