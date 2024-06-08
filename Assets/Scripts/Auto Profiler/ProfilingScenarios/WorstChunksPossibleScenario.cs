using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorstChunksPossibleScenario : WorldTask
{
    private Queue<WorldTask> tasks;
    WorldParameters world;
    public WorstChunksPossibleScenario(WorldParameters world, int renderDist)
    {
        this.world = world;
        tasks = new();
        tasks.Enqueue(new NextWorldWorstCaseChunks(true));
        tasks.Enqueue(new SwitchProfilerAgentWorld(world));
        
        tasks.Enqueue(new SwitchProfilerAgentRenderDist(10));
        tasks.Enqueue(new WaitTask(1f));
        tasks.Enqueue(new WaitConditionTask(WaitConditionTask.ChunkWaitCondition));
        tasks.Enqueue(new NextWorldWorstCaseChunks(false));

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
            ProfilerManager.Manager.CompleteScenario("Worst Chunks Possible Scenario", world);
            agent.CurrentWorld.SetWorstChunks(false);
            IsComplete = true;
        }
    }
}
