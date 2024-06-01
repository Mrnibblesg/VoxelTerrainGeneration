using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMoveTask : ITask
{
    int speed;
    Vector3 destination;
    bool complete = false;
    public SimpleMoveTask(Vector3 destination)
    {
        speed = 10;
        this.destination = destination;
    }
    public void Perform(ITaskable<AbstractAgent> agent) {
        Vector3 direction = Vector3.Normalize(destination - agent.transform.position);
        float distance = speed;
        if (direction.magnitude < speed)
        {
            distance = direction.magnitude;
            complete = true;
        }
        agent.Move(Vector3.Normalize(direction) * distance);
    }
    public void Interrupt()
    {

    }
    public bool IsComplete()
    {
        return complete;
    }
}
