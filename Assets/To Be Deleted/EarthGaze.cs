using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthGaze : MonoBehaviour {

    [SerializeField] private GameObject earth;
    [SerializeField] private GameObject sun;

    [Tooltip("The duration the user wanders around before the light appears.")]
    [SerializeField] private float wanderingDuration = 40f;
    [Tooltip("The duration before the light starts orbiting.")]
    [SerializeField] private float pauseDuration = 10f;

    void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine("Trigger");
        }
        
    }

    private IEnumerator Trigger()
    {
        float progress = 0f;
        while (progress < wanderingDuration)
        {
            progress += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        earth.SetActive(true);
        sun.SetActive(true);

        earth.transform.SetParent(this.transform.parent);
        sun.transform.SetParent(this.transform.parent);
    }
}
