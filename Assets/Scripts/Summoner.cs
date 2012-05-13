using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Summoner : MonoBehaviour
{
    public SpawnPoint[] SpawnPoints;

    public bool IsServerSummoner;
    public bool IsFakeAI;

    System.Random random = new System.Random();
    readonly List<int> TestList = new List<int>();

    // fake ai stuff
    float willSpawnIn;

    void Start()
    {
        TerrainGrid.Instance.Summoners.Add(IsServerSummoner ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId,
                                           this);

        var x = (int) Math.Floor(transform.position.x);
        var z = (int) Math.Floor(transform.position.z);

        var gridCell = TerrainGrid.Instance.Cells[x, z];
        gridCell.Occupant = gameObject;

        if (IsFakeAI)
            willSpawnIn = random.Next(4, 8);
    }

    void Update()
    {
        if (IsFakeAI)
        {
            willSpawnIn -= Time.deltaTime;
            if (willSpawnIn <= 0)
            {
                var animals = AnimalDatabase.Instance.Animals.Keys.ToArray();
                var firstAnimal = animals[random.Next(0, animals.Length)];
                string secondAnimal, thirdAnimal;
                while ((secondAnimal = animals[random.Next(0, animals.Length)]) == firstAnimal) ;
                while ((thirdAnimal = animals[random.Next(0, animals.Length)]) == firstAnimal && thirdAnimal == secondAnimal) ;

                var count = random.Next(1, 4);
                if (count == 1) TrySpawn(new[] { firstAnimal });
                if (count == 2) TrySpawn(new[] { firstAnimal, secondAnimal });
                if (count == 3) TrySpawn(new[] { firstAnimal, secondAnimal, thirdAnimal });

                willSpawnIn = random.Next(4, 8);
            }
        }
    }

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
                sp.SpawnTotemOnServer((Network.isServer && !IsFakeAI) ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId, validWords);
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
                sp.SpawnTotemOnServer((Network.isServer && !IsFakeAI) ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId, validWords);
                return;
            }
        }

        Debug.Log("No more space to spawn!");
    }
}
