using UnityEngine;
using System.Collections;

public class SplineWalker : MonoBehaviour {

    [Tooltip("Duration of the entire spline movement in seconds.")]
    public float Duration;
    [Tooltip("Determines if the object be moving forwards on the spline.")]
	public bool MoveFoward = true;
    [Tooltip("Determines if the object will be facing forwards when moving along the spline.")]
	public bool LookFoward;
    public bool IsMoving
    {
        get { return this.isMoving;  }
        private set { this.isMoving = value; }
    }
    public SplineWalkerMode Mode;

	private BezierSpline spline;
    private float progress = 0f;

    [SerializeField] private float deltaThreshold = 0.05f;

    private bool isMoving = false;

    public void StartWalker(BezierSpline spline)
    {
        this.spline = spline;
        IsMoving = true;
        StartCoroutine("Walk");
    }

    private IEnumerator Walk()
    {
        if (spline == null)
        {
            Debug.LogError("Spline path not defined.", this.gameObject);
            yield break;
        }

        // Iterate while sprite is still moving
        while (IsMoving)
        {
            // Used to get delta of position
            Vector3 prevPosition = spline.GetPoint(progress);

            if (progress < 1f)
            {
                // Move forward on spline
                if (MoveFoward)
                {
                    progress += Time.fixedDeltaTime / Duration;
                    if (progress > 1f)
                    {
                        if (Mode == SplineWalkerMode.Once)
                        {
                            progress = 1f;
                        }
                        else if (Mode == SplineWalkerMode.Loop)
                        {
                            progress -= 1f;
                        }
                        else
                        {
                            progress = 2f - progress;
                            MoveFoward = false;
                        }
                    }
                }
                else    // Move backwards on spline
                {
                    progress -= Time.fixedDeltaTime / Duration;
                    if (progress < 0f)
                    {
                        progress = -progress;
                        MoveFoward = true;
                    }
                }

                // Get the point on spline
                Vector3 position = spline.GetPoint(progress);
                transform.localPosition = position;
                if (LookFoward)
                {
                    transform.LookAt(position + spline.GetDirection(progress));
                }

                // Sprite is not moving anymore if the delta is less than the set threshold
                IsMoving = !(Vector3.Distance(prevPosition, position) < deltaThreshold);
            }
            yield return new WaitForFixedUpdate();
        }
    }
}