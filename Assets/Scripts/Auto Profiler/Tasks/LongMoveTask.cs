using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When the agent wants to progress, assign them SimpleMoveTasks to serve as
/// checkpoints on the way there.
/// </summary>
public class LongMoveTask : WorldTask
{

    public LongMoveTask(float speed, Vector3 destination)
    {

    }

    public override void Perform(Agent agent)
    {
        base.Perform(agent);


    }
}
