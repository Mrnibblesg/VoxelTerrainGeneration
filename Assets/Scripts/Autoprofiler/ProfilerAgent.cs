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
    public override World CurrentWorld
    {
        set
        {
            this.currentWorld?.UnloadAll();
            currentWorld = value;
            Vector3 startPosition = new(
                0.5f,
                value.parameters.WorldHeightInChunks * value.parameters.ChunkHeight / value.parameters.Resolution,
                0.5f
            );
            transform.position = startPosition;

            UpdateChunkCoord();
        }
    }
    public void Initialize()
    {
        tasks = new();
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

        //Keep the bot from getting stuck on the terrain while navigating
        if ((currentWorld?.VoxelFromGlobal(transform.position-(Vector3.up*0.9f))?.type ?? VoxelType.AIR) != VoxelType.AIR)
        {
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            transform.position += Vector3.up / currentWorld.parameters.Resolution;
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
