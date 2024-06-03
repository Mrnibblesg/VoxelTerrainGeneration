using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextWorldWorstCaseChunks : WorldTask
{
    bool value;
    public NextWorldWorstCaseChunks(bool value)
    {
        this.value = value;
    }
    public override void Perform(Agent agent)
    {
        ProfilerManager.Manager.SetProfilerAgentWorldWorstChunks(value);
        IsComplete = true;
    }
}
