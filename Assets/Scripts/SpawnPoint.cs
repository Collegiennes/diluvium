﻿using System;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Totem TotemPrefab;

    public void SpawnTotem(params string[] names)
    {
        if (!Network.isServer) 
            throw new InvalidOperationException("Only server can network-instantiate units");

        foreach (var n in names)
            Debug.Log(n);

        var totemGo = Network.Instantiate(TotemPrefab, transform.position, Quaternion.identity, 0) as Totem;
        foreach (var animalName in names)
            totemGo.networkView.RPC("AddAnimal", RPCMode.All, animalName);
    }
}
