using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassSingleReplaceScenario : WorldTask
{
    private Queue<WorldTask> tasks;
    WorldParameters world;

    private int progressPlaced;
    private int progressBroken;
    private int maxAmount;

    public MassSingleReplaceScenario(WorldParameters world, int amount)
    {
        this.world = world;
        tasks = new();
        progressPlaced = 0;
        progressBroken = 0;

        maxAmount = amount;
        tasks.Enqueue(new SwitchProfilerAgentWorld(world));
        tasks.Enqueue(new WaitTask(1f));
        tasks.Enqueue(new WaitConditionTask(WaitConditionTask.ChunkWaitCondition));

    }

    public override void Perform(Agent agent)
    {
        base.Perform(agent);

        if (tasks.Count > 0)
        {
            agent.Taskable.AddTask(tasks.Dequeue());
        }
        else if (progressPlaced < maxAmount)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(2, 50),
                Random.Range(-10, 40),
                Random.Range(-50, 50));
            agent.Taskable.AddTask(
                new SingleReplaceTask(
                    agent.transform.position + randomOffset,
                    VoxelType.STONE));
            progressPlaced++;
        }
        else if (progressBroken < maxAmount)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(2, 50),
                Random.Range(-10, 40),
                Random.Range(-50, 50));
            agent.Taskable.AddTask(
                new SingleReplaceTask(
                    agent.transform.position + randomOffset,
                    VoxelType.AIR));
            progressBroken++;
        }
        else
        {
            ProfilerManager.Manager.CompleteScenario("Mass Single Replace Scenario", world);
            IsComplete = true;
        }
    }
}
