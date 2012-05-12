using UnityEngine;
using System.Collections;

public class TerrainGrid : MonoBehaviour
{
    public int sizeX = 8;
    public int sizeZ = 16;

    public static TerrainGrid Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public float[,] Height { get; private set; }

    void Start()
    {
        Height = new float[sizeX, sizeZ];

        for(int i = 0; i < sizeX; i++) for(int j = 0; j < sizeZ; j++)
        {

            RaycastHit hit;
            if(Physics.Raycast(
                transform.TransformPoint(new Vector3(i+0.5f, 100, j+0.5f)),
                -Vector3.up, out hit, Mathf.Infinity))
            {
                Height[i, j] = transform.InverseTransformPoint(hit.point).y;
            }
            else
            {
                Height[i, j] = 100;
            }
                    
        }
    }

    void OnDrawGizmos()
    {
        for(int i = 0; i < sizeX; i++) for(int j = 0; j < sizeZ; j++)
        {
            float h = Height == null ? 0 : Height[i, j] + 0.01f;
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
