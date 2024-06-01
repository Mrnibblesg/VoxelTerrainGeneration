using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMoveTask : WorldTask
{
    int speed;
    Vector3 destination;
    public SimpleMoveTask(Vector3 destination)
    {
        speed = 10;
        this.destination = destination;
    }
    public override void Perform(Agent agent) {
        Vector3 direction = Vector3.Normalize(destination - agent.transform.position);
        float distance = speed;
        if (direction.magnitude < speed)
        {
            distance = direction.magnitude;
            this.IsComplete = true;
        }
        agent.Move(Vector3.Normalize(direction) * distance);
    }
    public override void Interrupt()
    {

    }
}
