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
        seed = 1;
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
            voxels = new NativeArray<Voxel>(world.chunkSize * world.chunkHeight * world.chunkSize, Allocator.Persistent) //If this job takes more than 4 frames, switch to Allocator.Persistent
        };

        ChunkGenJob chunkGenJob = new()
        {
            size = world.chunkSize,
            height = world.chunkHeight,
            seed = seed,
            resolution = world.resolution,
            coords = new int3(chunkCoords.x, chunkCoords.y, chunkCoords.z),

            voxels = data.voxels
        };

        JobManager.Manager.addJob(chunkGenJob.Schedule(), FinishChunkData, data);
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
        Voxel[,,] voxels = new Voxel[world.chunkSize, world.chunkHeight, world.chunkSize];

        //Convert flat data array to 3d array
        for (int i = 0; i < results.voxels.Length; i++)
        {
            voxels[i / (world.chunkHeight * world.chunkSize),
                (i / world.chunkSize) % world.chunkHeight,
                i % world.chunkSize] = results.voxels[i];
        }

        results.voxels.Dispose();
        world.ChunkFinished(results.chunkCoord, voxels);
        
    }
}