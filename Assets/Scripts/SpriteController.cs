using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    // States in which the sprite can be in
    public enum SpriteState
    {
        Idle, Moving
    };

    private SpriteState _currentState = SpriteState.Idle;
    public SpriteState CurrentState
    {
        get
        {
            return _currentState;
        }
    }

    // The SplineWalker the sprite will move along
    private SplineWalker walker;

    private void Awake()
    {
        walker = GetComponent<SplineWalker>();
    }

    void FixedUpdate () {
		switch(CurrentState)
        {
            case (SpriteState.Idle):
                break;
            case (SpriteState.Moving):
                /*transform.position += dir * MoveSpeed * Time.deltaTime;

                if (Vector3.Distance(transform.position, dest) < 1.0f)
                {
                    _currentState = SpriteState.Idle;
                }*/
                break;
        }
	}

    public void Move(BezierSpline spline)
    {
        _currentState = SpriteState.Moving;
        walker.StartWalker(spline);
    }
}
