using System;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Totem TotemPrefab;

    public void SpawnTotem(params string[] names)
    {
        var totemGo = Network.Instantiate(TotemPrefab, transform.position, Quaternion.identity, 0) as Totem;

        foreach (var animalName in names)
            totemGo.networkView.RPC("AddAnimal", RPCMode.All, animalName);
    }
}
