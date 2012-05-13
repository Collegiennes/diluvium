using System;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Totem TotemPrefab;

    public void SpawnTotemOnServer(int playerId, string[] animalNames)
    {
        if (Network.isServer)
            SpawnTotem(playerId, 
                       animalNames[0], 
                       animalNames.Length > 1 ? animalNames[1] : null,
                       animalNames.Length > 2 ? animalNames[2] : null);
        else
            networkView.RPC("SpawnTotem", RPCMode.Server, playerId, animalNames);
    }

    [RPC]
    public void SpawnTotem(int owner, string animalName1, string animalName2, string animalName3)
    {
        if (!Network.isServer)
            throw new InvalidOperationException("Spawning only allowed on the server!");

        var totemGo = Network.Instantiate(TotemPrefab, transform.position, Quaternion.identity, 0) as Totem;

        totemGo.networkView.RPC("AddAnimal", RPCMode.All, animalName1);
        if (!string.IsNullOrEmpty(animalName2)) totemGo.networkView.RPC("AddAnimal", RPCMode.All, animalName2);
        if (!string.IsNullOrEmpty(animalName3)) totemGo.networkView.RPC("AddAnimal", RPCMode.All, animalName3);

        if (totemGo.SetOwner(owner))
            totemGo.networkView.RPC("SetOwner", RPCMode.Others, owner);
    }
}
