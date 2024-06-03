using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassReplaceTask : WorldTask
{
    Vector3 p1;
    Vector3 p2;
    VoxelType type;
    public MassReplaceTask(Vector3 p1, Vector3 p2, VoxelType type)
    {
        this.p1 = p1;
        this.p2 = p2;
        this.type = type;
    }

    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        //agent.TryTwoPointReplace(VoxelType.AIR, type);
    }
}
