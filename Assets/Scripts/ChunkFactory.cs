using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

//The chunk factory is in charge of creating chunks. Chunk creation may be complex
//so it's good to split the logic away from the World class.
//We not only need to generate the structure of terrain, but we then need to 
//determine what voxels are what.
public class ChunkFactory
{
#if PROFILER_ENABLED
    public bool worstChunks = false;
#endif

    uint seed;
    World world;

    //Special data that is used when generating the world.
    private struct JobData
    {
        public Vector3Int chunkCoord;
        public NativeArray<Voxel> voxels;
    }

    public ChunkFactory(World world)
    {
        seed = (uint)world.parameters.Seed;
        if (seed == 0)
        {
            seed = 1;
        }
        this.world = world;
    }

    /// <summary>
    /// Creates a job to build a new chunk.
    /// </summary>
    /// <param name="c"></param>
    /// <param name="chunkCoords"></param>
    public void RequestNewChunk(Vector3Int chunkCoords)
    {
            JobData data = new()
            {
                chunkCoord = chunkCoords,
                //If this job takes more than 4 frames, switch to Allocator.Persistent
                voxels = new NativeArray<Voxel>(world.parameters.ChunkSize * world.parameters.ChunkHeight * world.parameters.ChunkSize, Allocator.TempJob)
            };

            ChunkGenJob chunkGenJob = new()
            {
                size = world.parameters.ChunkSize,
                height = world.parameters.ChunkHeight,
                waterHeight = world.parameters.WaterHeight,
                seed = seed,
                resolution = world.parameters.Resolution,
                coords = new int3(chunkCoords.x, chunkCoords.y, chunkCoords.z),
#if PROFILER_ENABLED
                worstCase = worstChunks,
#endif

                voxels = data.voxels
            };

            JobManager.Manager.AddJob(chunkGenJob.Schedule(), FinishChunkData, data);
    }
    /// <summary>
    /// The callback used to return to when a chunk finishes generating.
    /// Calls world.chunkFinished()
    /// </summary>
    /// <param name="results"></param>
    public void FinishChunkData(object raw)
    {
        //Only need to convert the flat array into a 3d array
        JobData results = (JobData)raw;

        world.ChunkFinished(results.chunkCoord, VoxelRun.toVoxelRun(results.voxels));
        results.voxels.Dispose();
    }
}