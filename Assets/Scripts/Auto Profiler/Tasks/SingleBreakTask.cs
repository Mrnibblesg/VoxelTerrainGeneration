using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleBreakTask : WorldTask
{
    Vector3 VoxelToBreak;

    public SingleBreakTask(Vector3 position)
    {
        VoxelToBreak = position;
    }
    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        agent.TryBreak(VoxelToBreak);
        IsComplete = true;
    }

    public override void Interrupt()
    {

    }
}
