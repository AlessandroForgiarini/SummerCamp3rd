using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GoblinTutorial : MonoBehaviour
{
    public Transform[] points;
    public float Speed = 1.6f;
    int target = 0;
    NavMeshAgent agent;
    Animator anim;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = agent.GetComponentInChildren<Animator>();
        anim.SetBool("Moving", true);
        

        agent.SetDestination(points[target].position);
    }

    // Update is called once per frame
    void Update()
    {
        HandleAnimator();

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            target = ++target % points.Length;
            agent.SetDestination(points[target].position);
        }
    }

    private void HandleAnimator()
    {
        Vector3 vel = agent.transform.rotation * agent.velocity;
        vel.Normalize();
        anim.SetFloat("LocomotionX", vel.x);
        anim.SetFloat("LocomotionY", vel.z);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (points == null) return;
        if (points.Length <= 1) return;
        Gizmos.DrawLineStrip(points.Select(p => p.position).ToArray(), true);
    }
}
