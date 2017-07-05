using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthGaze : MonoBehaviour {

    [SerializeField] private GameObject earth;
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject space;

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
        Debug.Log("EarthGaze");
        float progress = 0f;
        while (progress < wanderingDuration)
        {
            progress += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("here");
        earth.SetActive(true);
        sun.SetActive(true);

        earth.transform.SetParent(this.transform.parent, true);
        sun.transform.SetParent(this.transform.parent, true);

        Debug.Log(Vector3.Distance(this.transform.position, sun.transform.position));
    }
}
