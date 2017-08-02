using UnityEngine;

public class FireLightScript : MonoBehaviour
{
	[SerializeField]
    private float minIntensity = 0.25f;
    [SerializeField]
    private float maxIntensity = 0.5f;
    [SerializeField]
	private Light fireLight;
    [SerializeField]
    private float randomMax = 150;

    private float random;

    void Update()
	{
		random = Random.Range(0.0f, randomMax);
		float noise = Mathf.PerlinNoise(random, Time.time);
		fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
	}
}