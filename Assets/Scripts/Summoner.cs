using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Summoner : MonoBehaviour
{
    public const int MaxHealth = 1000;

    public SpawnPoint[] SpawnPoints;
    public DamageNumber WordDisplay;
    public Totem TotemPrefab;

    public bool IsServerSummoner;
    public bool IsFakeAI;

    public bool HasFailed { get; set; }
    public bool HasTakenDamage { get; private set; }
    public float Health { get; private set; }
    public bool IsReady { get; set; }

    public AudioClip hurtSound;

    static readonly System.Random random = new System.Random();
    readonly List<int> TestList = new List<int>();

    public event Action Die;

    int PlayerId;

    // fake ai stuff
    float willSpawnIn = random.Next(4, 8);

    void Awake()
    {
        Restart();
    }

    public void Restart()
    {
        Health = MaxHealth;
        HasFailed = false;
        HasTakenDamage = false;

        Debug.Log("reset " + PlayerId);
    }

    void Start()
    {
        TerrainGrid.Instance.Summoners.Add(PlayerId = (IsServerSummoner ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId),
                                           this);

        var x = (int) Math.Floor(transform.position.x);
        var z = (int) Math.Floor(transform.position.z);

        var gridCell = TerrainGrid.Instance.Cells[x, z];
        gridCell.Occupant = gameObject;
    }

    void Update()
    {
        if (IsFakeAI && GameFlow.State == GameState.Gameplay)
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
                if (count == 1) TrySpawn(firstAnimal.ToUpper(), "", "");
                if (count == 2) TrySpawn(firstAnimal.ToUpper(), secondAnimal.ToUpper(), "");
                if (count == 3) TrySpawn(firstAnimal.ToUpper(), secondAnimal.ToUpper(), thirdAnimal.ToUpper());

                willSpawnIn = random.Next(4, 8);
            }
        }
    }

    public void TellReady()
    {
        IsReady = true;
        Debug.Log("telling ready! " + PlayerId);
        networkView.RPC("MarkReady", RPCMode.Others);
    }

    [RPC]
    public void MarkReady()
    {
        Debug.Log("marking ready! " + PlayerId);
        IsReady = true;
    }

    public void TrySpawnOnServer(string[] animals)
    {
        if (IsServerSummoner)
            TrySpawn(animals[0], animals.Length > 1 ? animals[1] : "", animals.Length > 2 ? animals[2] : "");
        else
            networkView.RPC("TrySpawn", RPCMode.Server, animals[0], animals.Length > 1 ? animals[1] : "", animals.Length > 2 ? animals[2] : "");
    }

    [RPC]
    public void TrySpawn(string animalName1, string animalName2, string animalName3)
    {
        bool hasSpawned = false;

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
                Spawn(sp, animalName1, animalName2, animalName3);
                hasSpawned = true;
                break;
            }
        }

        if (!hasSpawned)
        {
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
                    Spawn(sp, animalName1, animalName2, animalName3);
                    hasSpawned = true;
                    break;
                }
            }
        }

        if (hasSpawned)
            networkView.RPC("ShowWords", RPCMode.All, animalName1, animalName2, animalName3);
        else
            Debug.Log("No more space to spawn!");
    }

    void Spawn(SpawnPoint spawnPoint, string animalName1, string animalName2, string animalName3)
    {
        var totemGo = Network.Instantiate(TotemPrefab, spawnPoint.transform.position, Quaternion.identity, 0) as Totem;

        totemGo.networkView.RPC("AddAnimal", RPCMode.All, animalName1);
        if (!string.IsNullOrEmpty(animalName2)) totemGo.networkView.RPC("AddAnimal", RPCMode.All, animalName2);
        if (!string.IsNullOrEmpty(animalName3)) totemGo.networkView.RPC("AddAnimal", RPCMode.All, animalName3);

        totemGo.SetOwner(PlayerId);
        totemGo.networkView.RPC("SetOwner", RPCMode.Others, PlayerId);
    }

    [RPC]
    public void ShowWords(string animalName1, string animalName2, string animalName3)
    {
        var maxHeight = !string.IsNullOrEmpty(animalName3) ? 3.4f : !string.IsNullOrEmpty(animalName2) ? 2.85f : 2.3f;

        var go = Instantiate(WordDisplay, transform.position + Vector3.up * maxHeight, Quaternion.identity) as DamageNumber;
        go.Color = Color.white;
        go.Text = animalName1;

        if (!string.IsNullOrEmpty(animalName2))
            TaskManager.Instance.WaitFor(0.2f).Then(() =>
            {
                go = Instantiate(WordDisplay, transform.position + Vector3.up * (maxHeight - 0.55f), Quaternion.identity) as DamageNumber;
                go.Color = Color.white;
                go.Text = animalName2;
            });

        if (!string.IsNullOrEmpty(animalName3))
            TaskManager.Instance.WaitFor(0.4f).Then(() =>
            {
                go = Instantiate(WordDisplay, transform.position + Vector3.up * (maxHeight - 1.1f), Quaternion.identity) as DamageNumber;
                go.Color = Color.white;
                go.Text = animalName3;
            });
    }

    [RPC]
    public void Hurt(float amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            Health = 0;
            if (Die != null && GameFlow.State == GameState.Gameplay)
            {
                Die();
            }
        }

        audio.PlayOneShot(hurtSound);

        HasTakenDamage = true;
        TaskManager.Instance.WaitFor(0.5f).Then(() => { HasTakenDamage = false; });
    }
}
