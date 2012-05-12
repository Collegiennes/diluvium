using UnityEngine;

class Billboard : MonoBehaviour
{
    void Update()
    {
        transform.rotation = Quaternion.AngleAxis(180, Vector3.up) * Camera.main.transform.rotation;
    }
}
