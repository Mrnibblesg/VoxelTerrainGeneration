using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleActionsScenario : WorldTask
{
    private Queue<WorldTask> tasks;


    public SimpleActionsScenario(WorldParameters world)
    {
        tasks = new();
        tasks.Enqueue(new SwitchProfilerAgentWorld(world));
        tasks.Enqueue(new WaitTask(1.5f));
        tasks.Enqueue(new SimpleMoveTask(new Vector3(30, -1, 30), true));
        tasks.Enqueue(new WaitTask(1f));
        tasks.Enqueue(new SingleBreakTask(new Vector3(27, 29, 31)));
        tasks.Enqueue(new WaitTask(0.2f));
        tasks.Enqueue(new SingleBreakTask(new Vector3(27, 29, 30)));
        tasks.Enqueue(new WaitTask(0.2f));
        tasks.Enqueue(new SingleBreakTask(new Vector3(28, 29, 31)));

        tasks.Enqueue(new ChunkSingleBreakTask(new Vector3Int(0, 1, 2)));
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
            ProfilerManager.Manager.CompleteScenario("Simple actions");
            IsComplete = true;
        }
    }
}
