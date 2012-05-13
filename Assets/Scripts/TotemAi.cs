using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TotemAi : MonoBehaviour
{
    void Start()
    {
        GetComponent<Totem>().MyTurn += OnMyTurn;
    }

    Vector2 CoordOf(Transform t)
    {
        Vector3 pos = t.localPosition;
        return new Vector2(Mathf.Floor(pos.x), Mathf.Floor(pos.z));
    }

    float Distance(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x-b.x) + Mathf.Abs(a.y-b.y);
    }

    float Closeness(Vector2 a, Vector2 b)
    {
        return 1-Mathf.Min(10, Distance(a, b))/10.0f;
    }

	Vector3? OnMyTurn(Totem totem)
    {
        // find a target
        var desire = new Dictionary<Vector2, float>();

        int otherPlayer = totem.Owner == TerrainGrid.ServerPlayerId ?
            TerrainGrid.ClientPlayerId : TerrainGrid.ServerPlayerId;

        Vector2 mySummoner = CoordOf(TerrainGrid.Instance.Summoners[totem.Owner].transform);
        Vector2 otherSummoner = CoordOf(TerrainGrid.Instance.Summoners[otherPlayer].transform);

        Vector2 myCoord = CoordOf(transform);

        float wantToKillSummoner = 1;
        float wantToDefendSummoner = 1;
        foreach(Totem t in TerrainGrid.Instance.Totems[otherPlayer])
        {
            desire[CoordOf(t.transform)] =
                wantToDefendSummoner*Closeness(mySummoner, CoordOf(t.transform)) +
                Closeness(myCoord, CoordOf(t.transform));
        }
        desire[otherSummoner] =
            wantToKillSummoner * Closeness(myCoord, otherSummoner);

        Vector2 target = desire.OrderBy(x => x.Value).First().Key;

        // now we have a target! go there!
        float edgeNoise = Mathf.Pow(10, 5-totem.TotemIntelligence);
        Vector2 source = CoordOf(transform);

        TerrainGrid grid = TerrainGrid.Instance;
        IShortestPathGraph<Vector2> graph = new DijkstraShortestPathGraph<Vector2>();

        for(int i = 0; i < grid.sizeX; i++)
        for(int j = 0; j < grid.sizeZ; j++)
            graph.AddNode(new Vector2(i, j));

        for(int i = 0; i < grid.sizeX; i++)
        for(int j = 0; j < grid.sizeZ; j++)
        {
            if(i < grid.sizeX-1)
            {
                if(Mathf.Abs(TerrainGrid.GetHeightAt(i, j) -
                   TerrainGrid.GetHeightAt(i+1, j)) < 1.01f
                   && (TerrainGrid.IsWalkable(i, j) ||
                       source == new Vector2(i, j) ||
                       target == new Vector2(i, j))
                   && (TerrainGrid.IsWalkable(i+1, j) ||
                       source == new Vector2(i+1, j) ||
                       target == new Vector2(i+1, j)))
                {
                    Debug.DrawLine(
                        new Vector3(i+0.5f, TerrainGrid.GetHeightAt(i, j), j+0.5f),
                        new Vector3(i+1.5f, TerrainGrid.GetHeightAt(i+1, j), j+0.5f),
                        Color.red, 1, false);
                    float weight = 1+edgeNoise*Random.value;
                    graph.AddEdge(new Vector2(i, j), new Vector2(i+1, j), weight);
                    graph.AddEdge(new Vector2(i+1, j), new Vector2(i, j), weight);
                }
            }
            if(j < grid.sizeZ-1)
            {
                if(Mathf.Abs(TerrainGrid.GetHeightAt(i, j) -
                   TerrainGrid.GetHeightAt(i, j+1)) < 1.01f
                   && (TerrainGrid.IsWalkable(i, j) ||
                       source == new Vector2(i, j) ||
                       target == new Vector2(i, j))
                   && (TerrainGrid.IsWalkable(i, j+1) ||
                       source == new Vector2(i, j+1) ||
                       target == new Vector2(i, j+1)))
                {
                    Debug.DrawLine(
                        new Vector3(i+0.5f, TerrainGrid.GetHeightAt(i, j), j+0.5f),
                        new Vector3(i+0.5f, TerrainGrid.GetHeightAt(i, j+1), j+1.5f),
                        Color.red, 1, false);
                    float weight = 1+edgeNoise*Random.value;
                    graph.AddEdge(new Vector2(i, j), new Vector2(i, j+1), weight);
                    graph.AddEdge(new Vector2(i, j+1), new Vector2(i, j), weight);
                }
            }
        }

        IList<Vector2> path = graph.GetPath(source, target);
        if(path == null)
            return null;

        Vector2 direction = path[1] - path[0];
        if (path.Count > 2)
            networkView.RPC("MoveTo", RPCMode.All, new Vector3(direction.x, 0, direction.y));
        else
            return new Vector3(direction.x, 0, direction.y);

        return null;
	}
}
