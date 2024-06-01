using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfileGameTask : WorldTask
{
    private Queue<WorldTask> tasks;
    private WorldTask currentTask;
    public ProfileGameTask(ProfilerAgent profilerAgent)
    {
        tasks = new();

        tasks.Enqueue(new WaitTask(1.5f));

        tasks.Enqueue(new SimpleMoveTask(new Vector3(30,-1,30), true));
        tasks.Enqueue(new WaitTask(1f));

        tasks.Enqueue(new SingleBreakTask(new Vector3(27,29,31)));
        tasks.Enqueue(new WaitTask(0.2f));
        tasks.Enqueue(new SingleBreakTask(new Vector3(27,29,30)));
        tasks.Enqueue(new WaitTask(0.2f));
        tasks.Enqueue(new SingleBreakTask(new Vector3(28,29,31)));

        tasks.Enqueue(new ChunkSingleBreakTask(new Vector3Int(0,1,2)));
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
            IsComplete = true;
        }
    }
    private void GiveNextTask(Agent agent)
    {
        
    }

    public override void Interrupt()
    {

    }
}
