using System.Linq;
using UnityEngine;

class Summoner : MonoBehaviour
{
    public SpawnPoint SpawnPoint;

    // debug stuff
    string[] animalNames;
    bool isSpawning;

    void Awake()
    {
        animalNames = AnimalDatabase.Instance.Animals.Keys.ToArray();
    }

    void Update()
    {
        if (Network.isServer && !isSpawning)
        {
            SpawnEveryTwoSeconds();
            isSpawning = true;
        }
    }

    void SpawnEveryTwoSeconds()
    {
        var count = Random.Range(1, 4);
        //var count = 3;
        var names = new string[count];
        for (int i = 0; i < count; i++)
            names[i] = animalNames[Random.Range(0, animalNames.Length)];

        TaskManager.Instance.WaitFor(2).Then(() => SpawnPoint.SpawnTotem(names)).Then(SpawnEveryTwoSeconds);
    }
}
