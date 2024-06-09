using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

/// <summary>
/// When the agent wants to perform, assign them SimpleMoveTasks to serve as
/// checkpoints on the way there. If the agent is close enough, set the destination
/// </summary>
public class LongMoveTask : WorldTask
{
    //How long between each checkpoint
    float checkpointDistance = 100;
    float speed;
    Vector3 destination;
    public LongMoveTask(float speed, Vector3 destination)
    {
        this.speed = speed;
        this.destination = destination;
    }

    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        Vector3 difference = destination - agent.transform.position;
        if (difference.magnitude > checkpointDistance)
        {
            Vector3 checkpoint = agent.transform.position + Vector3.Normalize(difference) * checkpointDistance;
            agent.Taskable.AddTask(
                new SimpleMoveTask(
                    checkpoint, true, speed
                )
            );
        }
        else
        {
            agent.Taskable.AddTask(
                new SimpleMoveTask(
                    destination, true, speed
                )
            );
            IsComplete = true;
        }
    }
}
