using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Collections;

public class TerrainGrid : MonoBehaviour
{
    public int sizeX = 8;
    public int sizeZ = 16;

    public bool LocalMode = true;

    public static TerrainGrid Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        Totems.Add(ServerPlayerId, new List<Totem>());
        Totems.Add(ClientPlayerId, new List<Totem>());
    }

    public GridCell[,] Cells { get; private set; }

    readonly public Dictionary<int, Summoner> Summoners = new Dictionary<int, Summoner>();
    readonly public Dictionary<int, List<Totem>> Totems = new Dictionary<int, List<Totem>>();

    public const int ServerPlayerId = 0;
    public const int ClientPlayerId = 1;

    public static bool IsWalkable(int x, int z)
    {
        return x >= 0 && x < Instance.sizeX &&
               z >= 0 && z < Instance.sizeZ &&
               Instance.Cells[x, z].Occupant == null;
    }

    public static GridCell RegisterTotem(int playerId, Totem totem)
    {
        var x = (int) Math.Floor(totem.transform.position.x);
        var z = (int) Math.Floor(totem.transform.position.z);

        var gridCell = Instance.Cells[x, z];
        if (gridCell.Occupant != null)
            throw new InvalidOperationException("Cell already occupied");

        gridCell.Occupant = totem.gameObject;
        Instance.Totems[playerId].Add(totem);
        return gridCell;
    }
    public static GridCell MoveTotem(Vector3 oldPosition, Vector3 newPosition)
    {
        var oldX = (int)Math.Floor(oldPosition.x);
        var oldZ = (int)Math.Floor(oldPosition.z);
        var oldCell = Instance.Cells[oldX, oldZ];

        var newX = (int)Math.Floor(newPosition.x);
        var newZ = (int)Math.Floor(newPosition.z);
        var newCell = Instance.Cells[newX, newZ];

        newCell.Occupant = oldCell.Occupant;
        oldCell.Occupant = null;

        return newCell;
    }
    public static void UnregisterTotem(int playerId, Totem totem)
    {
        var x = (int)Math.Floor(totem.transform.position.x);
        var z = (int)Math.Floor(totem.transform.position.z);

        Instance.Cells[x, z].Occupant = null;
        Instance.Totems[playerId].Remove(totem);
    }

    public static float GetHeightAt(Vector3 position)
    {
        var x = (int)Math.Floor(position.x);
        var z = (int)Math.Floor(position.z);

        return Instance.Cells[x, z].Height;
    }
    public static float GetHeightAt(int x, int z)
    {
        return Instance.Cells[x, z].Height;
    }

    void Start()
    {
        Cells = new GridCell[sizeX, sizeZ];

        for(int i = 0; i < sizeX; i++) for(int j = 0; j < sizeZ; j++)
        {
            RaycastHit hit;
            if(Physics.Raycast(transform.TransformPoint(new Vector3(i+0.5f, 100, j+0.5f)),
                               -Vector3.up, out hit, Mathf.Infinity))
            {
                Cells[i, j] = new GridCell
                                  {
                                      X = i,
                                      Z = j,
                                      Height = transform.InverseTransformPoint(hit.point).y
                                  };
            }
            else
                Cells[i, j] = new GridCell { X = i, Z = j, Height = 100 };
                    
        }
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        Totems[ClientPlayerId].Clear();
    }

    void OnDrawGizmos()
    {
        for(int i = 0; i < sizeX; i++) for(int j = 0; j < sizeZ; j++)
        {
            float h = Cells == null ? 0 : Cells[i, j].Height + 0.01f;
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i,   h, j  )),
                            transform.TransformPoint(new Vector3(i+1, h, j  )));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i,   h, j+1)),
                            transform.TransformPoint(new Vector3(i+1, h, j+1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i,   h, j  )),
                            transform.TransformPoint(new Vector3(i,   h, j+1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i+1, h, j  )),
                            transform.TransformPoint(new Vector3(i+1, h, j+1)));
        }
    }
}

public class GridCell
{
    public GameObject Occupant { get; set; }
    public float Height { get; set; }
    public int X { get; set; }
    public int Z { get; set; }
}
