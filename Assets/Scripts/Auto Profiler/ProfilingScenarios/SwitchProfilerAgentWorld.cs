using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Honestly, not really a world task. This is a code smell. We needed some way
/// to automatically switch the world for some agent, as a completable task.
/// We delegate this task to the ProfilerManager for the agent.
/// Really, what should be possible is for this task to only be doable by
/// AuthoritativeAgents (we should provide methods to switch worlds in that abstract class),
/// so they can switch worlds of their own accord.
/// </summary>
public class SwitchProfilerAgentWorld : WorldTask
{
    WorldParameters parameters;
    public SwitchProfilerAgentWorld(WorldParameters parameters)
    {
        this.parameters = parameters;
    }
    public override void Perform(Agent agent)
    {
        ProfilerManager.Manager.SetProfilerAgentWorld(parameters);
        IsComplete = true;
    }
}
