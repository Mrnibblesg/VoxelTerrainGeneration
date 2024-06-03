using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JourneyScenario : WorldTask
{
    private Queue<WorldTask> tasks;
    WorldParameters world;
    public JourneyScenario(WorldParameters world)
    {
        this.world = world;
        tasks = new();
        tasks.Enqueue(new SwitchProfilerAgentWorld(world));
        tasks.Enqueue(new WaitConditionTask(WaitConditionTask.ChunkWaitCondition));

        //Long journey
        tasks.Enqueue(new LongMoveTask(5, new Vector3(0, 0, 1000)));
    }

    public override void Perform(Agent agent)
    {
        base.Perform(agent);

        if (tasks.Count > 0)
        {
            agent.Taskable.AddTask(tasks.Dequeue());
        }
        else
        {
            ProfilerManager.Manager.CompleteScenario("Journey Scenario", world);
            IsComplete = true;
        }
    }
}
