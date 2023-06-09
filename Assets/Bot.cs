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

    Vector3 wanderTarget = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //Pursue(cop);
        //Evade(cop);
        //Wander();
        //Hide();
        if (CanSeeTarget())
            HideV2();
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

    void Wander()
    {
        // creating the circle that the destination would be on
        float wanderRadius = 10.0f;
        // distance from the Bot
        float wanderDistance = 20.0f;
        // jitter along the circle circumforance
        float wanderJitter = 1.5f;

        // adjusting the wander target as the game plays
        wanderTarget += new Vector3(
            Random.Range(-1.0f, 1.0f) * wanderJitter,
            0.0f,
            Random.Range(-1.0f, 1.0f) * wanderJitter);

        // normarilze the vector so we can multiply with the radiuse
        // to keep it in the circle circumforance
        wanderTarget.Normalize();
        wanderTarget *= wanderRadius;

        // apply the distance offset to the wander target in the radiuse
        Vector3 targetLocal = wanderTarget + new Vector3(0.0f, 0.0f, wanderDistance);
        // convert the position to a world position the the bot can seek it
        Vector3 targetWorld = gameObject.transform.InverseTransformVector(targetLocal);

        // finally seek the newly created position
        Seek(targetWorld);
    }


    void Hide()
    {
        float distance = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        float hidingDistance = 3.0f;

        foreach (GameObject hidingSpot in World.Instance.HidingSpots)
        {
            // a vector from the cop to the tree
            Vector3 hidingDirection = hidingSpot.transform.position - cop.position;
            // set offset to the hiding vector so the robber can hide behind the tree
            Vector3 hidingPosition = hidingSpot.transform.position + hidingDirection.normalized * hidingDistance;

            // choose the closest hiding spot
            if (Vector3.Distance(transform.position, hidingPosition) < distance)
            {
                chosenSpot = hidingPosition;
                distance = Vector3.Distance(transform.position, hidingPosition);
            }
        }

        Seek(chosenSpot);
    }


    void HideV2()
    {
        float distance = Mathf.Infinity;
        Vector3 chosenSpot = Vector3.zero;
        Vector3 ChosenDirection = Vector3.zero;
        GameObject chosenHidingSpot = World.Instance.HidingSpots[0];
        float rayProjectionStart = 10.0f;

        foreach (GameObject hidingSpot in World.Instance.HidingSpots)
        {
            // a vector from the cop to the tree
            Vector3 hidingDirection = hidingSpot.transform.position - cop.position;
            // set offset to the hiding vector so the robber can hide behind the tree
            Vector3 hidingPosition = hidingSpot.transform.position + hidingDirection.normalized * rayProjectionStart;

            // choose the closest hiding spot
            if (Vector3.Distance(transform.position, hidingPosition) < distance)
            {
                chosenSpot = hidingPosition;
                // get the direction of which the raycast would start
                ChosenDirection = hidingDirection;
                // get the gameobject that we want to calculate its collider
                chosenHidingSpot = hidingSpot;
                distance = Vector3.Distance(transform.position, hidingPosition);
            }
        }

        // get reference to collider
        Collider hidingSpotCollider = chosenHidingSpot.GetComponent<Collider>();
        // launch a ray from the back of the cover
        // to hid the back of the cover collider and get the hiding spot from that point
        Ray backRay = new Ray(chosenSpot, -ChosenDirection.normalized);
        RaycastHit hit;
        float rayDistance = 100.0f;
        hidingSpotCollider.Raycast(backRay, out hit, rayDistance);

        // seek the position of the cover + a relive space
        float reliveDistance = 3.0f;
        Seek(hit.point + ChosenDirection.normalized * reliveDistance);
    }


    bool CanSeeTarget()
    {
        Vector3 rayToTarget = cop.position - transform.position;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, rayToTarget, out hit))
        {
            if (hit.collider.CompareTag("Cop"))
            {
                Debug.Log("The cop found me!!!!!!");
                return true;
            }
        }

        return false;
    }
}
