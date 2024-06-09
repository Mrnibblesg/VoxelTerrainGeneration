using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleReplaceTask : WorldTask
{
    Vector3 VoxelToReplace;
    VoxelType type;
    bool surfaceSet;
    bool surface;

    public SingleReplaceTask(Vector3 position, VoxelType type, bool surface=false)
    {
        VoxelToReplace = position;
        surfaceSet = false;
        this.surface = surface;
        this.type = type;
    }
    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        if (surface && !surfaceSet)
        {
            float x = this.VoxelToReplace.x;
            float z = this.VoxelToReplace.z;
            this.VoxelToReplace.y = agent.CurrentWorld.HeightAtLocation(x, z);
            surfaceSet = true;
        }
        agent.TryPlace(VoxelToReplace, type);
        IsComplete = true;
    }

    public override void Interrupt()
    {

    }
}
