using UnityEngine;
using System.Collections;

public class WeatherRain : MonoBehaviour {
	
	public float randoom;
	
	void Start() {
		
		
		randoom = Random.value * 4;
		
	}
    void Update() {
	
        float offset = Mathf.Floor((Time.time * 8)) * (0.25F * randoom);

        renderer.material.mainTextureOffset = new Vector2(0, offset);

    }
}