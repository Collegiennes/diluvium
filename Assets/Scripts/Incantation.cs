using UnityEngine;
using System.Collections;

public class Incantation : MonoBehaviour
{
    public GUIStyle containerStyle;
    public GUIStyle textStyle;

    string text = "kitty hedgehog  pony";

	void Start ()
    {
	
	}
	
	void Update ()
    {
        
	}

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
            GUILayout.Label(word, textStyle);
            if(i < words.Length-1)
                GUILayout.Space(10);
            else
                GUILayout.Label("_", textStyle);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // handle text entry
        Event e = Event.current;
        if(e.type == EventType.KeyDown)
        {
            if(char.IsLetter(e.character) || e.character == ' ')
                text += e.character;
            else if(e.keyCode == KeyCode.Backspace && text.Length > 0)
            {
                text = text.Remove(text.Length-1);
            }
        }
    }
}
