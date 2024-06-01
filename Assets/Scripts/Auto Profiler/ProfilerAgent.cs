using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Acts as a stand-in for a player to perform actions autonomously, directed by
//some controller (in this case, the Profiler
//Everything from movement, block breaking/placing, mass block breaks/places
//teleporting, world reloading is done automatically.

//Task behavior: Does a task one at a time in a stack structure.

public class ProfilerAgent : AuthoritativeAgent, ITaskable<ProfilerAgent>
{
    private Stack<ITask> tasks;
    private ITask currentTask;
    public override World CurrentWorld
    {
        set
        {
            currentWorld = value;
            Vector3 startPosition = new(
                0.5f,
                value.worldHeight * value.chunkHeight / value.resolution,
                0.5f
            );
            transform.position = startPosition;

            UpdateChunkCoord();
        }
    }
    private void Start()
    {
        tasks = new();
    }

    public override void Update()
    {
        base.Update();
        if (currentTask == null || currentTask.IsComplete())
        {
            if (tasks.Count > 0)
            {
                currentTask = tasks.Pop();
            }
            else
            {
                currentTask = null;
            }
        }

        if (currentTask != null)
        {
            currentTask.Perform(this);

        }
        
    }
    public void PerformTask(ITask t)
    {
        t.Perform(this);
    }

    public void AddTask(ITask task)
    {
        tasks.Push(task);
    }
}
