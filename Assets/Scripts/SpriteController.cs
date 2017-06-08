using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    public enum SpriteState
    {
        Idle, Moving
    };

    [SerializeField] private float MoveSpeed = 10.0f;

    private SpriteState _currentState = SpriteState.Idle;
    public SpriteState CurrentState
    {
        get
        {
            return _currentState;
        }
    }

    public Vector3 dir = Vector3.zero;
    public Vector3 dest = Vector3.zero;

	void FixedUpdate () {
		switch(_currentState)
        {
            case (SpriteState.Idle):
                break;
            case (SpriteState.Moving):
                transform.position += dir * MoveSpeed * Time.deltaTime;

                if (Vector3.Distance(transform.position, dest) < 1.0f)
                {
                    _currentState = SpriteState.Idle;
                }
                break;
        }
	}

    public void Move(Vector3 dest)
    {
        this.dest = dest;
        this.dir = (dest - transform.position).normalized;
        _currentState = SpriteState.Moving;
    }
}
