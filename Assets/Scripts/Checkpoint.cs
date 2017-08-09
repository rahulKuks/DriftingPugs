using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    private CheckpointController checkpointController;
    private float sphereRadius = 4.0f;

    void Awake()
    {
        checkpointController = transform.parent.GetComponent<CheckpointController>();
        if (checkpointController == null)
        {
            Debug.LogError("Unable to get checkpoint controller.", this.gameObject);
        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            checkpointController.CheckpointReached(this);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}
