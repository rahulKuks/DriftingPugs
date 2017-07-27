using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float rotationSpeed = 4.0f;
    [SerializeField] private float neighbourDistance = 3.0f;
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
        /*if (Vector3.Distance(transform.position, Vector3.zero) >= 100)
        {
            turning = true;
        }
        else
            turning = false;
        if (turning)
        {
            Vector3 diretion = Vector3.zero - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(diretion),
                rotationSpeed * Time.deltaTime);
            speed = Random.Range(0.5f, 1);
        }
        else
        {
            if (Random.Range(0, 5) < 1)
                ApplyRules();
        }*/
        if (Random.Range(0, 5) < 1)
            ApplyRules();

        transform.Translate(0, 0, Time.deltaTime * moveSpeed);

    }

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
                if (dist <= neighbourDistance)
                {
                    vCentre += otherFish.transform.position;
                    groupSize++;

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
            vCentre = vCentre / groupSize + (goalPos - this.transform.position);
            moveSpeed = Mathf.Min(gSpeed / groupSize, maxSpeed);

            direction = (vCentre + vAvoid) - transform.position;
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(direction),
                    rotationSpeed * Time.deltaTime);
        }
    }

    public void SetId(string name) { this.id = name; }
}