using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Summoner : MonoBehaviour
{
    public SpawnPoint[] SpawnPoints;

    public bool IsServerSummoner;

    System.Random random = new System.Random();

    void Start()
    {
        TerrainGrid.Instance.Summoners.Add(IsServerSummoner ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId,
                                           this);

        var x = (int) Math.Floor(transform.position.x);
        var z = (int) Math.Floor(transform.position.z);

        var gridCell = TerrainGrid.Instance.Cells[x, z];
        gridCell.Occupant = gameObject;
    }

    readonly List<int> TestList = new List<int>();

    public void TrySpawn(string[] validWords)
    {
        TestList.Clear();
        TestList.Add(0); TestList.Add(1); TestList.Add(2);

        while (TestList.Count > 0)
        {
            var i = random.Next(0, TestList.Count);
            var sp = SpawnPoints[TestList[i]];
            TestList.RemoveAt(i);

            var x = (int) Math.Floor(sp.transform.position.x);
            var z = (int) Math.Floor(sp.transform.position.z);

            if (TerrainGrid.IsWalkable(x, z))
            {
                sp.SpawnTotemOnServer(Network.isServer ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId, validWords);
                return;
            }
        }

        TestList.Add(3); TestList.Add(4); 
        while (TestList.Count > 0)
        {
            var i = random.Next(0, TestList.Count);
            var sp = SpawnPoints[TestList[i]];
            TestList.RemoveAt(i);

            var x = (int)Math.Floor(sp.transform.position.x);
            var z = (int)Math.Floor(sp.transform.position.z);

            if (TerrainGrid.IsWalkable(x, z))
            {
                sp.SpawnTotemOnServer(Network.isServer ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId, validWords);
                return;
            }
        }

        Debug.Log("No more space to spawn!");
    }
}
