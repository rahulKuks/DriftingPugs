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
        forest.mute = true;
        splash.mute = false;
        sea.mute = false;
    }

    public void EnterSpace()
    {
        sea.mute = true;
        space.mute = false;
    }
}
