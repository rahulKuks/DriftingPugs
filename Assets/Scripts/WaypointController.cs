using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointController : MonoBehaviour
{

    private static WaypointController _instance;

    public static WaypointController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<WaypointController>();
            }
            return _instance;
        }
    }

    public List<Waypoint> Waypoints;
    public SpriteController sprite;

    private int currentWayPointIndex = 0;

    public void WaypointReached(Waypoint point)
    {
        int index = Waypoints.IndexOf(point);
        if (index < 0)
        {
            Debug.LogError("Invalid way point " + point.gameObject.name);
            return;
        }
        Debug.Log(string.Format("Waypoint {0} reached.", index));
        currentWayPointIndex = index;
        int nextWayPointIndex = Mathf.Min(index+1, Waypoints.Count-1);

        /*Vector3 nextPoint = Waypoints[nextWayPointIndex].transform.position;
        sprite.Move(nextPoint);*/

        BezierSpline spline = Waypoints[nextWayPointIndex].gameObject.  GetComponent<BezierSpline>();
        if (spline == null)
            Debug.LogError("null spline");
        //sprite.Move(spline);
    }
}
