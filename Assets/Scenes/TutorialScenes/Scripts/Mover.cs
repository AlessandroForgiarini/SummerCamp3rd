using NUnit.Framework;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public Transform[] points;
    public float Speed = 1f;
    int target = 0;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, points[target].position, Time.deltaTime * Speed);

        if(transform.position == points[target].position)
        {
            target = 1-target;
        }
    }

    void OnDrawGizmos()
    {
        if (points == null) return;
        if (points.Length != 2) return;

        Gizmos.DrawWireSphere(points[0].position, 0.1f);
        Gizmos.DrawWireSphere(points[1].position, 0.1f);

        Gizmos.DrawLine(points[0].position, points[1].position);
    }
}
