using System.Linq;
using UnityEngine;
using System.Collections;

public class Incantation : MonoBehaviour
{
    public Color errorTint = Color.red;
    public Color successTint = Color.white;
    public GUIStyle containerStyle;
    public GUIStyle textStyle;

    public SpawnPoint ServerSpawnPoint;
    public SpawnPoint ClientSpawnPoint;

    string text = "";

    void OnGUI ()
    {
        GUILayout.BeginArea(new Rect(
            0,
            Screen.height-containerStyle.padding.horizontal-textStyle.fontSize,
            Screen.width-containerStyle.padding.vertical,
            containerStyle.padding.vertical+textStyle.fontSize));
        GUILayout.BeginHorizontal(containerStyle);

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
                GUILayout.Space(12);
            else
                GUILayout.Label("_", textStyle);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // handle text entry
        Event e = Event.current;
        if(e.type == EventType.KeyDown)
        {
            if(char.IsLetter(e.character) || (e.character == ' ' && words.Length < 3))
                text += e.character;
            else if(e.character == '\n')
            {
                var validWords = words.Where(x => AnimalDatabase.Get(x) != null).ToArray();

                if (Network.isServer)   ServerSpawnPoint.SpawnTotem(validWords);
                else                    ClientSpawnPoint.SpawnTotem(validWords);

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
