using UnityEngine;
using System.Collections;

public class TerrainGrid : MonoBehaviour
{
    public int sizeX = 8;
    public int sizeZ = 16;


    float [,] height;

    void Start()
    {
        height = new float[sizeX, sizeZ];

        for(int i = 0; i < sizeX; i++) for(int j = 0; j < sizeZ; j++)
        {

            RaycastHit hit;
            if(Physics.Raycast(
                transform.TransformPoint(new Vector3(i+0.5f, 100, j+0.5f)),
                -Vector3.up, out hit, Mathf.Infinity))
            {
                height[i, j] = transform.InverseTransformPoint(hit.point).y;
            }
            else
            {
                height[i, j] = 100;
            }
                    
        }
    }

    void OnDrawGizmos()
    {
        for(int i = 0; i < sizeX; i++) for(int j = 0; j < sizeZ; j++)
        {
            float h = height == null ? 0 : height[i, j] + 0.01f;
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
