using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    [SerializeField] private CheckpointController checkpointController;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (checkpointController == null)
        {
            Debug.LogError("Missing checkpoint controller.", this.gameObject);
        }
    }

    public void MoveAnimation()
    {
        anim.SetInteger("Checkpoint", checkpointController.currentCheckpointIndex);
    }
}
