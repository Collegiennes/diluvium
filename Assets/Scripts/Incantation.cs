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

    public Texture2D portraits;

    public SpawnPoint ServerSpawnPoint;
    public SpawnPoint ClientSpawnPoint;

    string text = "";

    void OnGUI ()
    {
        var textureHeight = containerStyle.normal.background.height;

        GUILayout.BeginArea(new Rect(0, Screen.height - textureHeight, containerStyle.normal.background.width, textureHeight), containerStyle);
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(291 - 48, Screen.height - 140, 414, 62));
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
        GUILayout.EndArea();

        // idle : 2/3f
        // hurt : 0/3f
        // summoning : 1/3f

        var offset = 2 / 3f;
        if (!string.IsNullOrEmpty(text))
            offset = 1 / 3f;

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
                        ServerSpawnPoint.SpawnTotemOnServer(validWords);
                    else
                        ClientSpawnPoint.SpawnTotemOnServer(validWords);
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
