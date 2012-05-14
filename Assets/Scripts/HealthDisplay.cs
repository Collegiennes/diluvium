using UnityEngine;
using System.Collections;

public class HealthDisplay : MonoBehaviour
{
    public Totem totem;
    public Transform pivot;
    public Renderer healthFullRenderer;
    public Renderer healthEmptyRenderer;

    void Start()
    {
        Incantation i = Incantation.Instance;

        int myId = Network.isServer ?
            TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId;
        if(myId == totem.Owner)
        {
            healthFullRenderer.material.color = i.hpGoodColor;
            healthEmptyRenderer.material.color = i.hpBadColor;
        }
        else
        {
            healthFullRenderer.material.color = i.hpEnemyGoodColor;
            healthEmptyRenderer.material.color = i.hpEnemyBadColor;
        }
    }

    void Update()
    {
        if(totem.TotemMaxHealth == 0)
            return;

        float scale = pivot.localScale.x;
        float t = Mathf.Pow(0.01f, Time.deltaTime);
        scale = scale * t +
            (float)totem.TotemCurrentHealth/totem.TotemMaxHealth*(1-t);
        pivot.localScale = new Vector3(scale, 1, 1);

        transform.localPosition =
            new Vector3(0, 0.28f+0.865f*totem.AnimalObjects.Count, 0);
    }
}
