using UnityEngine;
using System.Collections;

public class CameraMotion : MonoBehaviour
{
    const float TransitionTime = 0.5f;
    const float ShowTime = 1.5f;

    public GameObject Credits, Logo, Tutorial;

    public static float PanFactor = 1;

    int showing;
    GameObject[] order;
    float time = -1;
    bool stop = true;

	void Start ()
    {
        transform.localRotation = Quaternion.Euler(0, 45, 0);

        order = new[] { Credits, Logo, Tutorial };
	}

    void OnConnectedToServer()
    {
        transform.localRotation = Quaternion.Euler(0, -45, 0);
    }

	void Update ()
	{
        if (showing <= 2)
        {
            if (showing == 2)
            {
                if (time > TransitionTime && stop)
                {
                    stop &= !Input.GetKeyDown(KeyCode.Space);
                    if (!stop)
                        time += ShowTime;
                }
                else
                    time += Time.deltaTime;
            }
            else
                time += Time.deltaTime;

            var step = time < (TransitionTime + ShowTime)
                           ? Mathf.Clamp01(time / TransitionTime)
                           : Mathf.Clamp01(1 - ((time - (ShowTime + TransitionTime)) / TransitionTime));
            order[showing].renderer.material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, step));

            if (time >= TransitionTime * 2 + ShowTime)
            {
                showing++;
                time = 0;
            }
        }

        if (showing >= 3)
        {
            time += Time.deltaTime;

            float t = Mathf.Pow(0.5f, Time.deltaTime);
            transform.position = t * transform.position + (1 - t) * FindSceneCenter();

            PanFactor = 1 - Easing.EaseOut(Mathf.Clamp01(time / 3), EasingType.Quadratic);
        }
	}

    Vector3 FindSceneCenter()
    {
        Vector3 minPos = new Vector3(100, 100, 100);
        Vector3 maxPos = new Vector3(0, 0, 0);

        foreach (GridCell c in TerrainGrid.Instance.Cells)
        {
            if (c.Occupant != null)
            {
                Vector3 pos = new Vector3(c.X + 0.5f, c.Height, c.Z + 0.5f);
                minPos = Vector3.Min(minPos, pos);
                maxPos = Vector3.Max(maxPos, pos);
            }
        }

        return (minPos + maxPos) / 2;
    }
}
