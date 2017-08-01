using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
	[SerializeField] private AudioSource forest;
	[SerializeField] private AudioSource sea;
	[SerializeField] private AudioSource space;
	[SerializeField] private AudioSource splash;
	[SerializeField] private AudioSource earthGaze;
	[SerializeField] private float crossfadeDuration = 1.0f;

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
		splash.Play();
		sea.PlayDelayed(1.0f);
    }

    public void EnterSpace()
    {
		StartCoroutine(Crossfade(sea, space));
    }

    public void PlayEarthGaze()
    {
        earthGaze.Play();
    }

	private IEnumerator Crossfade(AudioSource source1, AudioSource source2)
	{
		// Play 2nd source with volume starting at 0 to fade in
		source2.volume = 0;
		source2.Play();

		float progress = 0.0f;

		// Fade out 1st source & fade in 2nd source
		while (progress <= crossfadeDuration)
		{
			progress += Time.deltaTime;
			source1.volume = Mathf.Min(1 - (progress / crossfadeDuration), 1.0f);
			source2.volume = Mathf.Min(progress / crossfadeDuration, 1.0f);
			yield return new WaitForEndOfFrame();
		}

		source1.Stop();
	}
}
