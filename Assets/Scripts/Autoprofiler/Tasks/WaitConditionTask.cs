using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wait until a custom given condition evaluates to true before completion
/// This could be something like waiting until all the chunks in range have been loaded for instance.
/// </summary>
public class WaitConditionTask : WorldTask
{
    Func<Agent, bool> condition;
    public static bool ChunkWaitCondition(Agent agent)
    {
        return !agent.CurrentWorld.IsLoadingInProgress();
    }
    public WaitConditionTask(Func<Agent, bool> condition)
    {
        this.condition = condition;
    }

    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        if (this.condition(agent))
        {
            IsComplete = true;
        }
    }
}
