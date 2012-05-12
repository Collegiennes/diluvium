using System;
using UnityEngine;
using System.Collections.Generic;

public class AnimalDatabase : MonoBehaviour
{
    //// static ///////////////////////////////////////////////////////////////

    public static AnimalDatabase Instance { get; private set; }

    public static AnimalData Get(string name)
    {
        name = name.ToLower();
        if(Instance.Animals.ContainsKey(name))
            return Instance.Animals[name];
        else
            return null;
    }

    //// instance /////////////////////////////////////////////////////////////
    public TextAsset animalCsv;

    Dictionary<string, AnimalData> Animals;

    void Awake()
    {
        Instance = this;
        Animals = new Dictionary<string, AnimalData>();

	    string[] data = animalCsv.text.Split('\n');
        for(int i = 1; i < data.Length; i++)
        {
            string[] rowData = data[i].Split(',');

            AnimalData animal = new AnimalData();
            animal.name = rowData[0].Trim().ToLower();
            if(animal.name.Length != 0)
            {
                try
                {
                    animal.parentName = rowData[1].Trim();
                    if(animal.parentName.Length == 0)
                        animal.parentName = null;
                    animal.spriteIndex = ParseStat(rowData[2]);
                    animal.effectIndex = ParseStat(rowData[3]);
                    // attack graphic
                    animal.attack = ParseStat(rowData[4]);
                    animal.health = ParseStat(rowData[5]);
                    animal.speed = ParseStat(rowData[6]);
                    animal.intelligence = ParseStat(rowData[7]);

                    Animals[animal.name] = animal;
                }
                catch(System.Exception e)
                {
                    Debug.LogError(
                        "error reading animal \"" + animal.name + "\"\n" + e);
                }
            }
        }

        foreach(AnimalData animal in Animals.Values)
        {
            try
            {
                if(animal.parentName != null)
                {
                    AnimalData parent = Assert.NonNull(
                        Get(animal.parentName),
                        "missing parent");

                    if(animal.spriteIndex < 0)
                        animal.spriteIndex = parent.spriteIndex;
                    if(animal.effectIndex < 0)
                        animal.effectIndex = parent.effectIndex;
                    if(animal.attack < 0) animal.attack = parent.attack;
                    if(animal.health < 0) animal.health = parent.health;
                    if(animal.speed < 0) animal.speed = parent.speed;
                    if(animal.intelligence < 0)
                        animal.intelligence = parent.intelligence;
                }
                Assert.Condition(animal.spriteIndex >= 0,
                    "spriteIndex out of range");
                //Assert.Condition(animal.effectIndex >= 0,
                //    "effectIndex out of range");
                Assert.Condition(animal.attack >= 0, "attack out of range");
                Assert.Condition(animal.health >= 0, "health out of range");
                Assert.Condition(animal.speed >= 0, "speed out of range");
                Assert.Condition(animal.intelligence >= 0,
                    "intelligence out of range");
            }
            catch(System.Exception e)
            {
                Debug.LogError(
                    "error processing animal \"" + animal.name + "\"\n" + e);
            }
        }

        string msg = "";
        foreach(AnimalData animal in Animals.Values)
        {
            msg += animal.name + " " + animal.attack + " " + animal.health + " " +
                   animal.speed + " " + animal.intelligence + "\n";
        }
        print(msg);
	}

    int ParseStat(string s)
    {
        int stat = s.Trim().Length == 0 ? -1 : int.Parse(s);
        Assert.Condition(stat >= -1, "stat out of range");
        return stat;
    }
}

[Serializable]
public class AnimalData
{
    public string name;
    public string parentName;
    public int spriteIndex;
    public int effectIndex;
    public int attack;
    public int health;
    public int speed;
    public int intelligence;
}
