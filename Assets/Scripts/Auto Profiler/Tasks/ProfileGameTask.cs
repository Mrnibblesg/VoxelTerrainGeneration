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
        for (int res = 1; res <= 3; res++)
        {
            for (int chunkSizeFactor = 1; chunkSizeFactor < 5; chunkSizeFactor++)
            {
                int chunkSize = 16 * (int)Mathf.Pow(2, chunkSizeFactor-1);

                //Vertically-segmented chunks, then single vertical chunks.
                worldParams.Resolution = res;
                worldParams.ChunkSize = chunkSize;
                worldParams.ChunkHeight = 16;
                worldParams.WorldHeightInChunks = 4;

                QueueScenarios();

                worldParams.ChunkHeight = 64;
                worldParams.WorldHeightInChunks = 1;

                QueueScenarios();
                
            }
        }
    }
    //Queue all scenarios with the currently set world params
    private void QueueScenarios()
    {
        worldParams.Name = $"Simple Actions: " +
                    $"Resolution {worldParams.Resolution}, " +
                    $"Chunk size {worldParams.ChunkSize}, " +
                    $"Chunk Height {worldParams.ChunkHeight}, " +
                    $"World Height (Chunks) {worldParams.WorldHeightInChunks}";
        tasks.Enqueue(new SimpleActionsScenario(worldParams));

        //other scenarios down here...
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
