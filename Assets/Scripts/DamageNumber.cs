using System;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    const int ShadowOffset = 2;

    public float Amount;
    public GUIStyle Style;

    GUIStyle shadowStyle;
    float sinceStarted;
    Vector3 origin;

    void Start()
    {
        shadowStyle = new GUIStyle(Style) { normal = { textColor = Color.black } };
        origin = transform.position;
    }

    void Update()
    {
        sinceStarted += Time.deltaTime;
        var step = Easing.EaseIn(Mathf.Clamp01((sinceStarted - 0.3f) * 2), EasingType.Quadratic);

        Style.normal.textColor = new Color(Style.normal.textColor.r, Style.normal.textColor.g, Style.normal.textColor.b, 1 - step);
        shadowStyle.normal.textColor = new Color(0, 0, 0, 1 - step);

        transform.position = Vector3.Lerp(origin, origin + Vector3.up * 0.3f, step);

        if (step >= 1)
            Destroy(gameObject);
    }

    void OnGUI()
    {
        var point = Camera.main.WorldToScreenPoint(transform.position);

        var text = Amount.ToString();
        var size = Style.CalcSize(new GUIContent(text));

        // shadow
        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2 - ShadowOffset), (int)Math.Round(Screen.height - point.y - ShadowOffset), 100, 100), text, shadowStyle);
        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2 - ShadowOffset), (int)Math.Round(Screen.height - point.y + ShadowOffset), 100, 100), text, shadowStyle);
        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2 + ShadowOffset), (int)Math.Round(Screen.height - point.y - ShadowOffset), 100, 100), text, shadowStyle);
        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2 + ShadowOffset), (int)Math.Round(Screen.height - point.y + ShadowOffset), 100, 100), text, shadowStyle);

        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2 - ShadowOffset), (int)Math.Round(Screen.height - point.y), 100, 100), text, shadowStyle);
        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2 + ShadowOffset), (int)Math.Round(Screen.height - point.y), 100, 100), text, shadowStyle);
        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2), (int)Math.Round(Screen.height - point.y - ShadowOffset), 100, 100), text, shadowStyle);
        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2), (int)Math.Round(Screen.height - point.y + ShadowOffset), 100, 100), text, shadowStyle);

        // text
        GUI.Label(new Rect((int)Math.Round(point.x - size.x / 2), (int)Math.Round(Screen.height - point.y), 100, 100), text, Style);
    }
}
