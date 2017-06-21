using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        CheckpointController.Instance.CheckpointEvent.AddListener(MoveAnimation);
    }

    private void MoveAnimation()
    {
        anim.SetInteger("Checkpoint", CheckpointController.Instance.CurrentCheckpointIndex);
    }

    public void EnterSpace() {
        anim.SetBool("EnterSpace", true);
    }
}
