using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchProfilerAgentRenderDist : WorldTask
{
    int dist;
    public SwitchProfilerAgentRenderDist(int dist)
    {
        this.dist = dist;
    }
    public override void Perform(Agent agent)
    {
        ProfilerManager.Manager.SetProfilerAgentRenderDist(dist);
        IsComplete = true;
    }
}
