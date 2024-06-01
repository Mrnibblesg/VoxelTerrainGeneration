using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Acts as a stand-in for a player to perform actions autonomously, directed by
//some controller (in this case, the Profiler
//Everything from movement, block breaking/placing, mass block breaks/places
//teleporting, world reloading is done automatically.

//Task behavior: Does a task one at a time in a stack structure.

public class ProfilerAgent : AuthoritativeAgent, ITaskable
{
    private Stack<WorldTask> tasks;
    private Action callback;
    private bool complete = false;
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
    public void Initialize(Action callback)
    {
        tasks = new();
        this.callback = callback;
    }

    public override void Update()
    {
        base.Update();
        
        if (tasks.Count > 0)
        {
            WorldTask task = tasks.Peek();
            if (task.IsComplete)
            {
                tasks.Pop();
            }
            else
            {
                task.Perform(this);
            }
            
        }
        else if (!complete)
        {
            callback();
            complete = true;
        }
    }
    public void PerformTask(WorldTask t)
    {
        t.Perform(this);
    }

    public void AddTask(WorldTask task)
    {
        tasks.Push(task);
    }
}
