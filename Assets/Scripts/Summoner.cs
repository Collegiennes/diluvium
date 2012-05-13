using UnityEngine;

public class Summoner : MonoBehaviour
{
    public SpawnPoint[] SpawnPoints;

    public bool IsServerSummoner;

    void Start()
    {
        TerrainGrid.Instance.Summoners.Add(IsServerSummoner ? TerrainGrid.ServerPlayerId : TerrainGrid.ClientPlayerId,
                                           this);
    }
}
