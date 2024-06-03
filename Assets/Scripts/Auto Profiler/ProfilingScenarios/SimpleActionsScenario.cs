using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleActionsScenario : WorldTask
{
    private Queue<WorldTask> tasks;
    WorldParameters world;

    public SimpleActionsScenario(WorldParameters world)
    {
        this.world = world;
        tasks = new();
        tasks.Enqueue(new SwitchProfilerAgentWorld(world));
        tasks.Enqueue(new WaitTask(1f));
        tasks.Enqueue(new WaitConditionTask(WaitConditionTask.ChunkWaitCondition));
        tasks.Enqueue(new SimpleMoveTask(new Vector3(30, -1, 30), true, 5));
        tasks.Enqueue(new WaitTask(1f));
        tasks.Enqueue(new SingleReplaceTask(new Vector3(27, -1, 31), VoxelType.AIR, true));
        tasks.Enqueue(new WaitTask(0.2f));
        tasks.Enqueue(new SingleReplaceTask(new Vector3(27, -1, 30), VoxelType.AIR, true));
        tasks.Enqueue(new WaitTask(0.2f));
        tasks.Enqueue(new SingleReplaceTask(new Vector3(28, -1, 31), VoxelType.AIR, true));
        tasks.Enqueue(new WaitTask(1f));
        //tasks.Enqueue(new ChunkSingleReplaceTask(new Vector3Int(0, -1, 2), VoxelType.AIR, true));
        //tasks.Enqueue(new ChunkSingleReplaceTask(new Vector3Int(1, -1, 2), VoxelType.DIRT, true));
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
            ProfilerManager.Manager.CompleteScenario("Simple actions", world);
            IsComplete = true;
        }
    }
}
