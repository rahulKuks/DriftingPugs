using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointController : MonoBehaviour
{
	// Custom event to allow the index of the checkpoint as an input parameter
	[System.Serializable]
	public class CheckpointEvent : UnityEvent<int> {}

    // List of Checkpoints should be a child of this game object
    public List<Checkpoint> checkpoints;
    // Event for other objects to add listener to
	public CheckpointEvent checkpointEvent;
    public int currentCheckpointIndex
    {
        get;
        private set;
    }

    /// <summary>
    /// Update the currentCheckpointIndex if a succeeding checkpoint is hit and invokes the
    /// checkpointEvent.
    /// </summary>
    /// <param name="point">A checkpoint.</param>
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

        checkpointEvent.Invoke(index);
    }

    /// <summary>
    /// Finds the index of a given checkpoint
    /// </summary>
    /// <param name="cp">A checkpoint to check.</param>
    /// <returns>Index of cp if exists otherwise -1.</returns>
	public int IndexOfCheckpoint(Checkpoint cp)
	{
		return checkpoints.IndexOf(cp);
	}
}
