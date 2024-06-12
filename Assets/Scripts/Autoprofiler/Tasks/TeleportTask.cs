using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTask : WorldTask
{
    Vector3 destination;
    public TeleportTask(Vector3 destination)
    {
        this.destination = destination;
    }
    public override void Perform(Agent agent)
    {
        agent.transform.position = destination;
        this.IsComplete = true;
    }
}
