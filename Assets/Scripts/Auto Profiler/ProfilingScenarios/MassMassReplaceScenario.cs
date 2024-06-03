using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassMassReplaceScenario : WorldTask
{
    private Queue<WorldTask> tasks;
    WorldParameters world;

    private int progress;
    private int maxAmount;

    public MassMassReplaceScenario(WorldParameters world, int amount)
    {
        this.world = world;
        tasks = new();
        progress = 0;

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
        else if (progress < maxAmount)
        {
            Vector3 random1 = getRandomVec();
            Vector3 random2 = getRandomVec();
            Vector3 random3 = getRandomVec();
            Vector3 random4 = getRandomVec();


            agent.Taskable.AddTask(
                new MassReplaceTask(
                    agent.transform.position + random1,
                    agent.transform.position + random2,
                    VoxelType.STONE));

            agent.Taskable.AddTask(
                new MassReplaceTask(
                    agent.transform.position + random3,
                    agent.transform.position + random4,
                    VoxelType.AIR));
            progress++;
        }
        else
        {
            ProfilerManager.Manager.CompleteScenario("Mass Mass Replace Scenario", world);
            IsComplete = true;
        }
    }
    private Vector3 getRandomVec()
    {
        return new Vector3(
                Random.Range(2, 50),
                Random.Range(-10, 40),
                Random.Range(-50, 50));
    }
}
