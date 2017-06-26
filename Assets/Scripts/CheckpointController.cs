﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointController : MonoBehaviour
{
    // List of Checkpoints should be a child of this game object
    public List<Checkpoint> checkpoints;
    // Event for other objects to add listener to
    public UnityEvent checkpointEvent;
    public int currentCheckpointIndex
    {
        get;
        private set;
    }

    private void Awake()
    {
        checkpointEvent = new UnityEvent();
    }

    public void CheckpointReached(Checkpoint point)
    {
        // Check if the collided checkpoint exists
        int index = checkpoints.IndexOf(point);
        if (index < 0)
        {
            Debug.LogError("Invalid checkpoint " + point.gameObject.name);
            return;
        }
        // Don't trigger event if not first checkpoint and already hit the checkpoint
        else if (currentCheckpointIndex != 0 && index == currentCheckpointIndex)
            return;

        Debug.Log(string.Format("Checkpoint {0} reached.", index));
        currentCheckpointIndex = index;

        checkpointEvent.Invoke();
    }
}
