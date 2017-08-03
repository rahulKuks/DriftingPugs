using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
	[SerializeField] private AudioSource forest;
	[SerializeField] private AudioSource sea;
	[SerializeField] private AudioSource space;
	[SerializeField] private AudioSource earthGaze;
    [SerializeField] private AudioSource resolution;
    [SerializeField] private float seaSpaceCrossfadeDuration = 1.0f;

    private static SoundController _instance;
    public static SoundController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<SoundController>();
            }
            return _instance;
        }
    }

    public void EnterLake()
    {
		forest.Stop();
		sea.Play();
    }

    public void EnterSpace()
    {
		StartCoroutine(Crossfade(sea, space, seaSpaceCrossfadeDuration));
    }

    public void PlayEarthGaze()
    {
        earthGaze.Play();
    }

    public void PlayResolution(float fadeDuration)
    {
        StartCoroutine(Crossfade(space, resolution, fadeDuration));
    }

	private IEnumerator Crossfade(AudioSource source1, AudioSource source2, float duration)
	{
		// Play 2nd source with volume starting at 0 to fade in
		source2.volume = 0;
		source2.Play();

		float progress = 0.0f;

		// Fade out 1st source & fade in 2nd source
		while (progress <= seaSpaceCrossfadeDuration)
		{
			progress += Time.deltaTime;
			source1.volume = Mathf.Min(1 - (progress / duration), 1.0f);
			source2.volume = Mathf.Min(progress / duration, 1.0f);
			yield return new WaitForEndOfFrame();
		}

		source1.Stop();
	}
}
