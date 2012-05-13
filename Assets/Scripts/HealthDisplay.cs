using UnityEngine;
using System.Collections;

public class HealthDisplay : MonoBehaviour
{
    public Totem totem;
    public Transform pivot;

    void Update()
    {
        pivot.localScale = new Vector3(
            (float)totem.TotemCurrentHealth/totem.TotemMaxHealth, 1, 1);

        transform.localPosition =
            new Vector3(0, 0.28f+0.865f*totem.AnimalObjects.Count, 0);
    }
}
