using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenStressTestScenario : WorldTask
{
    private Queue<WorldTask> tasks;
    WorldParameters world;

    public WorldGenStressTestScenario(WorldParameters world, int renderDist)
    {
        this.world = world;
        tasks = new();
        tasks.Enqueue(new SwitchProfilerAgentWorld(world));
        tasks.Enqueue(new SwitchProfilerAgentRenderDist(renderDist));
        tasks.Enqueue(new WaitTask(1f));
        tasks.Enqueue(new WaitConditionTask(WaitConditionTask.ChunkWaitCondition));

        for (int i = 0; i < 19; i++)
        {
            tasks.Enqueue(new TeleportTask(new Vector3(i * 10000, 64, 0)));
            tasks.Enqueue(new WaitTask(2f));
            tasks.Enqueue(new WaitConditionTask(WaitConditionTask.ChunkWaitCondition));
        }
        //reset to some default value. magic number 7 right now :)
        tasks.Enqueue(new SwitchProfilerAgentRenderDist(7));
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
            ProfilerManager.Manager.CompleteScenario("World Gen Stress Test Scenario", world);
            IsComplete = true;
        }
    }
}
