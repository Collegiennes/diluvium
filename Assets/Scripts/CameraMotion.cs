using UnityEngine;
using System.Collections;

public class CameraMotion : MonoBehaviour
{
	void Start ()
    {
	    transform.localRotation = Quaternion.Euler(0, 45, 0);
	}
	
    bool firstUpdate = true;
	void Update ()
    {
        Vector3 minPos = new Vector3(100, 100, 100);
        Vector3 maxPos = new Vector3(0, 0, 0);

        foreach(GridCell c in TerrainGrid.Instance.Cells)
        {
            if(c.Occupant != null)
            {
                Vector3 pos = new Vector3(c.X+0.5f, c.Height, c.Z+0.5f);
                minPos = Vector3.Min(minPos, pos);
                maxPos = Vector3.Max(maxPos, pos);
            }
        }

        Vector3 center = (minPos + maxPos)/2;

        float t = Mathf.Pow(0.5f, Time.deltaTime);
        if(firstUpdate)
        {
            t = 0;
            firstUpdate = false;
        }
        transform.position = t * transform.position + (1-t) * center;
	}
}
