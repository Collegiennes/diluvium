using UnityEngine;
using System.Collections;

public class RenderCamera : MonoBehaviour
{
    public Renderer renderSurface;
    RenderTexture texture;

	void Start ()
    {
	    texture = new RenderTexture((int)(Screen.width*0.9f), (int)(Screen.height*1.0f), 24);
        texture.filterMode = FilterMode.Point;
        Camera.main.targetTexture = texture;
        renderSurface.material.mainTexture = texture;
	}
}
