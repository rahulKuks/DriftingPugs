using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointController : MonoBehaviour
{
    // Singleton pattern
    private static CheckpointController _instance;
    public static CheckpointController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<CheckpointController>();
            }
            return _instance;
        }
    }

    public List<Checkpoint> Checkpoints;
    // Event for other objects to add listener to
    public UnityEvent CheckpointEvent;
    public int CurrentCheckpointIndex
    {
        get;
        private set;
    }

    private void Awake()
    {
        CheckpointEvent = new UnityEvent();
    }

    public void CheckpointReached(Checkpoint point)
    {
        // Check if the collided checkpoint exists
        int index = Checkpoints.IndexOf(point);
        if (index < 0)
        {
            Debug.LogError("Invalid checkpoint " + point.gameObject.name);
            return;
        }
        // Don't trigger event if not first checkpoint and already hit the checkpoint
        else if (CurrentCheckpointIndex != 0 && index == CurrentCheckpointIndex)
            return;

        Debug.Log(string.Format("Checkpoint {0} reached.", index));
        CurrentCheckpointIndex = index;

        CheckpointEvent.Invoke();
    }
}
