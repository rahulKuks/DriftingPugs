using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTriggerer : MonoBehaviour
{

    public bool trigger = false;
    public CheckpointController checkpointController;
    public GameObject player;
    public float speed = 25;

    private Checkpoint[] checkpoints;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (trigger)
        {
            checkpoints = checkpointController.checkpoints.ToArray();
            StartCoroutine(Test());
            trigger = false;
        }
    }

    IEnumerator Test()
    {
        foreach (Checkpoint cp in checkpoints)
        {
            while (Vector3.Distance(cp.transform.position, player.transform.position) > 1e-6)
            {
                cp.transform.position = Vector3.MoveTowards(cp.transform.position, player.transform.position, speed * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            cp.gameObject.SetActive(false);
            yield return new WaitForSeconds(1.0f);
        }
    }
}