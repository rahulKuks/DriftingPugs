using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource forest;
    public AudioSource sea;
    public AudioSource space;
    public AudioSource splash;

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
		sea.Stop();
		space.Play();
    }
}
