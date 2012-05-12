using UnityEngine;
using System.Collections;

public class Incantation : MonoBehaviour
{
    public Color errorTint = Color.red;
    public Color successTint = Color.white;
    public GUIStyle containerStyle;
    public GUIStyle textStyle;

    string text = "";

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
            if(char.IsLetter(e.character) || e.character == ' ')
                text += e.character;
            else if(e.character == '\n')
            {
                foreach(string word in words)
                {
                    if(AnimalDatabase.Get(word) != null)
                    {
                        print("awesoem word: " + word);
                    }
                }
                text = "";
            }
            else if(e.keyCode == KeyCode.Backspace && text.Length > 0)
            {
                text = text.Remove(text.Length-1);
            }
        }
    }
}
