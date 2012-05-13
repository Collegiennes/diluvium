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

	void OnMyTurn(Totem totem)
    {
        float edgeNoise = Mathf.Pow(10, 5-totem.TotemIntelligence);
        Vector2 source = CoordOf(transform);
        Vector2 target = new Vector2(8, 2);

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
                float weight = 1+edgeNoise*Random.value;
                graph.AddEdge(new Vector2(i, j), new Vector2(i+1, j), weight);
                graph.AddEdge(new Vector2(i+1, j), new Vector2(i, j), weight);
            }
            if(j < grid.sizeZ-1)
            {
                float weight = 1+edgeNoise*Random.value;
                graph.AddEdge(new Vector2(i, j), new Vector2(i, j+1), weight);
                graph.AddEdge(new Vector2(i, j+1), new Vector2(i, j), weight);
            }
        }

        IList<Vector2> path = graph.GetPath(source, target);
        if(path == null)
            return;

        Vector2 direction = path[1] - path[0];
        networkView.RPC("MoveTo", RPCMode.All,
            new Vector3(direction.x, 0, direction.y));

        //Vector3 direction = Vector3.zero;
        //var valid = false;
        //Vector3[] directions = new [] {
        //    Vector3.right,
        //    Vector3.left,
        //    Vector3.forward,
        //    Vector3.back
        //};

        //foreach (var d in directions.OrderBy(elem => Guid.NewGuid()))
        //{
        //    var x = (int) Math.Floor(transform.position.x + d.x);
        //    var z = (int) Math.Floor(transform.position.z + d.z);

        //    valid |= x >= 0 && x < TerrainGrid.Instance.sizeX &&
        //             z >= 0 && z < TerrainGrid.Instance.sizeZ;

        //    if (valid)
        //    {
        //        direction = d;
        //        break;
        //    }
        //}

        //if (valid)
        //    networkView.RPC("MoveTo", RPCMode.All, direction);
	}
}
