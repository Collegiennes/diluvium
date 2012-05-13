using UnityEngine;
using System.Collections;

public class HealthDisplay : MonoBehaviour
{
    public Totem totem;
    public Transform pivot;

    void Update()
    {
        float scale = pivot.localScale.x;
        float t = Mathf.Pow(0.01f, Time.deltaTime);
        scale = scale * t +
            (float)totem.TotemCurrentHealth/totem.TotemMaxHealth*(1-t);
        pivot.localScale = new Vector3(scale, 1, 1);

        transform.localPosition =
            new Vector3(0, 0.28f+0.865f*totem.AnimalObjects.Count, 0);
    }
}
