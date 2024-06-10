using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SimpleMoveTask : WorldTask
{
    float speed;
    Vector3 destination;
    bool surface;
    bool surfaceSet;

    public SimpleMoveTask(Vector3 destination, bool surface = false, float speed = 2)
    {
        this.speed = speed;
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
            this.destination.y = agent.CurrentWorld.HeightAtLocation(x, z)+1.5f;
            if (this.destination.y == 1.5f)
            {
                agent.Taskable.AddTask(new WaitTask(0.5f));
                Debug.Log("Waiting on terrain!");
                return;
            }
            else
            {
                surfaceSet = true;
            }
        }
        Vector3 difference = destination - agent.transform.position;
        float distance = speed;
        if (difference.magnitude < speed)
        {
            distance = difference.magnitude;
            this.IsComplete = true;
            
        }

        agent.Move(distance * Vector3.Normalize(difference));
    }
    public override void Interrupt()
    {

    }
}
