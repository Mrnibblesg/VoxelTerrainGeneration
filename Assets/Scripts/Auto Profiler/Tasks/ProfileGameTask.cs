using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileGameTask : WorldTask
{
    private Queue<WorldTask> tasks;

    WorldParameters worldParams = new WorldParameters
    {
        WaterHeight = 10,
        WorldHeightInChunks = 4
    };

    public ProfileGameTask(ProfilerAgent profilerAgent)
    {
        tasks = new();
        
        //Queue up the scenarios with different combinations of world parameters.
        for (int res = 1; res < 4; res++)
        {
            for (int chunkSizeFactor = 1; chunkSizeFactor < 5; chunkSizeFactor++)
            {
                int chunkSize = 16 * (int)Mathf.Pow(2, chunkSizeFactor-1);

                //set up vertical-chunk worlds
                int maxHeight = worldParams.WorldHeightInChunks * 16;
                int originalWorldHeightInChunks = 4;
                for (int chunkHeight = 16; chunkHeight <= maxHeight; chunkHeight *= originalWorldHeightInChunks)
                {
                    //height = 16, then * world height in chunks, then finish loop
                    //only 2 settings.
                    worldParams.Resolution = res;
                    worldParams.ChunkSize = chunkSize;
                    worldParams.ChunkHeight = chunkHeight;
                    worldParams.WorldHeightInChunks = maxHeight / chunkHeight;

                    worldParams.Name = $"Simple Actions: " +
                        $"Resolution {res}, " +
                        $"Chunk size {chunkSize}, " +
                        $"Chunk Height {chunkHeight}";
                    tasks.Enqueue(new SimpleActionsScenario(worldParams));
                    //other scenarios down here...
                }
            }
        }
        
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
            ProfilerManager.Manager.FinishProfiling();
            IsComplete = true;
        }
    }

    public override void Interrupt()
    {

    }
}
