using UnityEngine;

public class SplineWalker : MonoBehaviour {


	public float duration;
	public bool lookForward;
	public SplineWalkerMode mode;

	private BezierSpline spline;
    [SerializeField] private float progress = 0f;
	[SerializeField] private bool goingFoward = true;
    [SerializeField] private float threshold = 0.05f;

    private bool isMoving = false;

    public void StartWalker(BezierSpline spline)
    {
        Debug.Log("Starting spline walker");
        this.spline = spline;
        isMoving = true;
    }

    private void Update () {
        Vector3 prevPosition = spline.GetPoint(progress);

        if (progress < 1f)
        {
            // Move forward on spline
            if (goingFoward)
            {
                progress += Time.deltaTime / duration;
                if (progress > 1f)
                {
                    if (mode == SplineWalkerMode.Once)
                    {
                        progress = 1f;
                    }
                    else if (mode == SplineWalkerMode.Loop)
                    {
                        progress -= 1f;
                    }
                    else
                    {
                        progress = 2f - progress;
                        goingFoward = false;
                    }
                }
            }
            else    // Move backwards on spline
            {
                progress -= Time.deltaTime / duration;
                if (progress < 0f)
                {
                    progress = -progress;
                    goingFoward = true;
                }
            }

            Vector3 position = spline.GetPoint(progress);
            transform.localPosition = position;
            if (lookForward)
            {
                transform.LookAt(position + spline.GetDirection(progress));
            }

            isMoving = !(Vector3.Distance(prevPosition, position) < threshold);
        }
	}
}