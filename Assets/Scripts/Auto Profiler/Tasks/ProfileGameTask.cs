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

    public ProfileGameTask()
    {
        tasks = new();
        
        //Queue up the scenarios with different combinations of world parameters.
        for (int res = 1; res <= 1; res++)
        {
            //chunkSizeFactor is for chunk size. We try 16, 32, 64, 128.
            for (int chunkSizeFactor = 1; chunkSizeFactor < 5; chunkSizeFactor++)
            {
                int chunkSize = 16 * (int)Mathf.Pow(2, chunkSizeFactor-1);

                //Vertically-segmented chunks, then single vertical chunks.
                worldParams.Resolution = res;
                worldParams.ChunkSize = chunkSize;
                worldParams.ChunkHeight = 16;
                worldParams.WorldHeightInChunks = 4 * (int)Mathf.Pow(2, res - 1);

                QueueScenarios();

                worldParams.ChunkHeight = 16 * worldParams.WorldHeightInChunks;
                worldParams.WorldHeightInChunks = 1;

                QueueScenarios();
                
            }
        }
    }
    //Queue all scenarios with the currently set world params
    private void QueueScenarios()
    {
        string worldNameEnding = $"Resolution {worldParams.Resolution}, " +
                    $"Chunk size {worldParams.ChunkSize}, " +
                    $"Chunk Height {worldParams.ChunkHeight}, " +
                    $"World Height (Chunks) {worldParams.WorldHeightInChunks}";

        worldParams.Name = $"Simple Actions: " + worldNameEnding;
        worldParams.Seed = 1;
        tasks.Enqueue(new SimpleActionsScenario(worldParams));

/*        worldParams.Name = $"Journey: " + worldNameEnding;
        worldParams.Seed = 2;
        tasks.Enqueue(new JourneyScenario(worldParams));

        worldParams.Name = $"World Gen Stress Test: " + worldNameEnding;
        worldParams.Seed = 3;
        tasks.Enqueue(new WorldGenStressTestScenario(worldParams, 10));

        worldParams.Name = $"Mass Single Replace: " + worldNameEnding;
        worldParams.Seed = 4;
        tasks.Enqueue(new MassSingleReplaceScenario(worldParams, 2000));

        worldParams.Name = $"Mass Mass Replace: " + worldNameEnding;
        worldParams.Seed = 5;
        tasks.Enqueue(new MassMassReplaceScenario(worldParams, 1000));

        //This scenario consumes a TON of resources... It's more useful if we keep the params low.
        if (worldParams.ChunkSize <= 32 && worldParams.Resolution == 1)
        {
            worldParams.Name = $"Worst Chunks: " + worldNameEnding;
            worldParams.Seed = 6;
            tasks.Enqueue(new WorstChunksPossibleScenario(worldParams, 10));
        }*/
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
