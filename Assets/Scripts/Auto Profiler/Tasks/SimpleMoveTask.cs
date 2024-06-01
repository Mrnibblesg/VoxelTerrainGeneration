using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMoveTask : WorldTask
{
    int speed;
    Vector3 destination;
    bool surface;
    bool surfaceSet;

    public SimpleMoveTask(Vector3 destination, bool surface = false)
    {
        speed = 4;
        this.destination = destination;
        if (surface)
        {
            surfaceSet = false;
            this.surface = true;
        }
    }
    public override void Perform(Agent agent) {
        if (surface && !surfaceSet)
        {
            float x = this.destination.x;
            float z = this.destination.z;
            this.destination.y = agent.CurrentWorld.HeightAtLocation(x, z)+1;
            surfaceSet = true;
        }
        Vector3 difference = destination - agent.transform.position;
        float distance = speed;
        if (difference.magnitude < speed)
        {
            distance = difference.magnitude;
            this.IsComplete = true;
        }
        agent.Move(Vector3.Normalize(difference) * distance * Time.fixedDeltaTime);
    }
    public override void Interrupt()
    {

    }
}
