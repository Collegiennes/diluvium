using UnityEngine;
using System.Collections;

public class WeatherRain : MonoBehaviour {
    void Update() {
	
        float offset = Mathf.Floor(Time.time) * 0.33F;
		
		
        renderer.material.mainTextureOffset = new Vector2(0, offset);

    }
}