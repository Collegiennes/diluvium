using UnityEngine;
using System.Collections;

public class RenderCamera : MonoBehaviour
{
    public Renderer renderSurface;
    RenderTexture texture;

	void Start ()
    {
	    texture = new RenderTexture((int)(Screen.width*0.5f), (int)(Screen.height*0.5f), 24);
        texture.filterMode = FilterMode.Point;
        Camera.main.targetTexture = texture;
        renderSurface.material.mainTexture = texture;
	}
}
