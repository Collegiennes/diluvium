using UnityEngine;
using System.Collections;

public class TerrainGrid : MonoBehaviour
{
    public int sizeX = 8;
    public int sizeZ = 16;

    void OnDrawGizmos()
    {
        for(int i = 0; i < sizeX; i++) for(int j = 0; j < sizeZ; j++)
        {
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i,   0.01f, j  )),
                            transform.TransformPoint(new Vector3(i+1, 0.01f, j  )));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i,   0.01f, j+1)),
                            transform.TransformPoint(new Vector3(i+1, 0.01f, j+1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i,   0.01f, j  )),
                            transform.TransformPoint(new Vector3(i,   0.01f, j+1)));
            Gizmos.DrawLine(transform.TransformPoint(new Vector3(i+1, 0.01f, j  )),
                            transform.TransformPoint(new Vector3(i+1, 0.01f, j+1)));
        }
    }
}
