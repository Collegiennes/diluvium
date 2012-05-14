using UnityEngine;
using System.Collections;

public class Ghost : MonoBehaviour
{
    float sinceStarted = 0;
    Vector3 origin;

    void Start()
    {
        origin = transform.position;
    }

	void Update ()
    {
        //timeLeft -= Time.deltaTime;
        //renderer.material.color = new Color(1, 1, 1, Mathf.Sqrt(timeLeft/totalTime));

        sinceStarted += Time.deltaTime;
        var step = Easing.EaseIn(Mathf.Clamp01((sinceStarted - 0.7f) / 2), EasingType.Quadratic);

        renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, 1 - step);
        //shadowStyle.normal.textColor = new Color(0, 0, 0, 1 - step);

        transform.position = Vector3.Lerp(origin, origin + Vector3.up * 3, step);

        if (step >= 1)
            Destroy(gameObject);
	}
}
