using System;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance;

    public static GameState State { get; set; }

    public void Restart()
    {
        var tgi = TerrainGrid.Instance;

        foreach (var totems in tgi.Totems.Values)
        {
            foreach (var t in totems)
                Destroy(t.gameObject);
            totems.Clear();
        }

        for (int i = 0; i < tgi.sizeX; i++) for (int j = 0; j < tgi.sizeZ; j++)
            tgi.Cells[i, j].Occupant = null;

        foreach (var s in tgi.Summoners.Values)
        {
            s.Restart();

            var x = (int)Math.Floor(s.transform.position.x);
            var z = (int)Math.Floor(s.transform.position.z);

            tgi.Cells[x, z].Occupant = s.gameObject;
        }

        Incantation.Instance.Restart();

        State = GameState.Splash;
    }

    void Awake()
    {
        Instance = this;
    }
}

public enum GameState
{
    Splash, Gameplay, Won, Lost
}