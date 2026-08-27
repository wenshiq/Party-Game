using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 3f;
    public WaypointManager waypoints { get; protected set; }

    protected virtual void Awake()
    {
        tag = GameTags.Platform;
        waypoints = GetComponent<WaypointManager>();
    }

    protected void Update()
    {
        // 每帧的按照waypoint去移动它的位置，到了位置后就进行下一个waypoint
        var position = transform.position;
        var target = waypoints.current.position;
        position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
        transform.position = position;

        if (Vector3.Distance(transform.position, target) == 0)
        {
            waypoints.Next();
        }
    }
}
