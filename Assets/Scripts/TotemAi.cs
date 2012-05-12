using System;
using UnityEngine;
using System.Collections;
using System.Linq;

public class TotemAi : MonoBehaviour
{
    void Start()
    {
        GetComponent<Totem>().MyTurn += OnMyTurn;
    }

	void OnMyTurn(Totem totem)
    {
        Vector3 direction = Vector3.zero;
        var valid = false;
        Vector3[] directions = new [] {
            Vector3.right,
            Vector3.left,
            Vector3.forward,
            Vector3.back
        };

        foreach (var d in directions.OrderBy(elem => Guid.NewGuid()))
        {
            var x = (int) Math.Floor(transform.position.x + d.x);
            var z = (int) Math.Floor(transform.position.z + d.z);

            valid |= x >= 0 && x < TerrainGrid.Instance.sizeX &&
                     z >= 0 && z < TerrainGrid.Instance.sizeZ;

            if (valid)
            {
                direction = d;
                break;
            }
        }

        if (valid)
            networkView.RPC("MoveTo", RPCMode.All, direction);
	}
}
