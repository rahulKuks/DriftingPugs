using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test3 : MonoBehaviour {

    public bool flag = false;
    public GameObject obj;
    public GameObject earth;
    public float speed = 0.5f;

    private void Start()
    {
        StartCoroutine("Test");
    }

    IEnumerator Test() {
        while (Vector3.Distance(transform.position, obj.transform.position) > 1e-6)
        {
            transform.position = Vector3.MoveTowards(transform.position, obj.transform.position, speed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        Debug.Log(transform.position - earth.transform.position);
	}
}
