using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;

    [SerializeField]
    Transform cop;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        Flee(cop.position);
    }

    void Seek(Vector3 target)
    {
        agent.SetDestination(target);
    }

    void Flee(Vector3 runFrom)
    {
        Vector3 fleeVector = transform.position - runFrom;
        agent.SetDestination(transform.position + fleeVector);
    }
}
