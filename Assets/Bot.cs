using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class Bot : MonoBehaviour
{
    NavMeshAgent agent;

    [SerializeField]
    Transform cop;

    [SerializeField]
    DataManagerBlueprint dataTracker;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //Pursue(cop);
        Evade(cop);
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

    void Pursue(Transform target)
    {
        Vector3 targetDirection = target.position - transform.position;
        float lookAhead = targetDirection.magnitude / (agent.speed + dataTracker.copSpeed);

        // calculate the angle between the two forward vectors of the robber and the cop
        // if the angle is so small it should seek the position of the cop
        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));

        // angle between the forward direction of the agent and the angle to the taget
        // if the angle greater than 90 the agent is in front of the cop and should seek his position and turn around
        // if the angle is less than 90 the agent is behind the cop
        float angleToTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDirection));

        // seek the cop if he stops
        if ((angleToTarget > 90.0f && relativeHeading < 20.0f) || dataTracker.copSpeed < 0.01f)
        {
            Seek(target.position);
            return;
        }
        Seek(target.position + target.transform.forward * lookAhead);
    }


    void Evade(Transform target)
    {
        Vector3 targetDirection = target.position - transform.position;
        float lookAhead = targetDirection.magnitude / (agent.speed + dataTracker.copSpeed);

        // calculate the angle between the two forward vectors of the robber and the cop
        // if the angle is so small it should flee the position of the cop
        float relativeHeading = Vector3.Angle(transform.forward, transform.TransformVector(target.transform.forward));

        // angle between the forward direction of the agent and the angle to the taget
        // if the angle greater than 90 the agent is in front of the cop and should flee his position
        // if the angle is less than 90 the agent is behind the cop
        float angleToTarget = Vector3.Angle(transform.forward, transform.TransformVector(targetDirection));


        // flee the cop of he stops
        if ((angleToTarget > 90.0f && relativeHeading < 20.0f) || dataTracker.copSpeed < 0.01f)
        {
            Flee(target.position);
            return;
        }
        Flee(target.position + target.transform.forward * lookAhead);
    }
}
