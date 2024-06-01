using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleBreakTask : WorldTask
{
    Vector3 VoxelToBreak;
    bool surfaceSet;
    bool surface;

    public SingleBreakTask(Vector3 position, bool surface=false)
    {
        VoxelToBreak = position;
        surfaceSet = false;
        this.surface = surface;
    }
    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        if (surface && !surfaceSet)
        {
            float x = this.VoxelToBreak.x;
            float z = this.VoxelToBreak.z;
            this.VoxelToBreak.y = agent.CurrentWorld.HeightAtLocation(x, z);
            surfaceSet = true;
        }
        agent.TryBreak(VoxelToBreak);
        IsComplete = true;
    }

    public override void Interrupt()
    {

    }
}
