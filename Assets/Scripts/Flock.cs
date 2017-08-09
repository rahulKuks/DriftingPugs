using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script to simulate the flocking behaviour of fishes.
/// </summary>
public class Flock : MonoBehaviour
{
    [Tooltip("Speed of the fish before flocking.")]
    [SerializeField] private float moveSpeed = 0.1f;
    [Tooltip("Speed the fish will rotate at.")]
    [SerializeField] private float rotationSpeed = 4.0f;
    [Tooltip("The threshold distance to flock with another fish.")]
    [SerializeField] private float neighbourDistance = 3.0f;
    [Tooltip("The maximum speed when fishes flock together.")]
    [SerializeField] private float maxSpeed = 7.0f;

    private bool turning = false;
    private Vector3 goalPos = Vector3.zero;
    private Vector3 vCentre = Vector3.zero;
    private Vector3 vAvoid = Vector3.zero;
    private Vector3 direction = Vector3.zero;
    private float gSpeed;
    private float dist;
    private int groupSize;

    private string id;

    // Use this for initialization
    void Start()
    {
        moveSpeed = Random.Range(0.5f, 1);
        goalPos = FishManager.Instance.FishGoalPosition(id);
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.Range(0, 5) < 1)
            ApplyRules();

        transform.Translate(0, 0, Time.deltaTime * moveSpeed);
    }

    /// <summary>
    /// Determines which fishes to flock with.
    /// </summary>
    private void ApplyRules()
    {
        // Reset vars
        goalPos = FishManager.Instance.FishGoalPosition(id);
        vCentre = Vector3.zero;
        vAvoid = Vector3.zero;
        direction = Vector3.zero;
        gSpeed = 0.1f;
        dist = 0;
        groupSize = 0;
        
        foreach (Flock otherFish in FishManager.Instance.Fishes)
        {
            if (otherFish.GetInstanceID() != this.gameObject.GetInstanceID())
            {
                dist = Vector3.Distance(otherFish.transform.position, this.transform.position);
                // Add to flock group if within threshold
                if (dist <= neighbourDistance)
                {
                    vCentre += otherFish.transform.position;
                    groupSize++;

                    // Avoid colliding with other fish
                    if (dist < 1.0f)
                    {
                        vAvoid = vAvoid + (this.transform.position - otherFish.transform.position);
                    }
                    gSpeed = gSpeed + otherFish.moveSpeed;
                }
            }
        }
        if (groupSize > 0)
        {
            // Calculate variables in flock
            vCentre = vCentre / groupSize + (goalPos - this.transform.position);
            moveSpeed = Mathf.Min(gSpeed / groupSize, maxSpeed);

            direction = (vCentre + vAvoid) - transform.position;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction),
                    rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Changes the type of the fish.
    /// </summary>
    /// <param name="name">Type of fish.</param>
    public void SetId(string name) { this.id = name; }
}