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

    //We use spline points to control how the terrain height value slopes 
    //on different noise intervals via linear interpolation.
    //We use two native arrays because these are the only arrays that can be given
    //to jobs.
    NativeArray<float> continentalnessSplinePoints;
    NativeArray<float> continentalnessTerrainHeight;
    
    NativeArray<float> erosionSplinePoints;
    NativeArray<float> erosionTerrainHeight;



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
        // (0, 1)
        // (0.3, 50)
        // (0.7, 70)
        // (0.8, 150)
        // (1, 150)
        continentalnessSplinePoints = new(5,Allocator.Persistent);
        continentalnessTerrainHeight = new(5,Allocator.Persistent);

        erosionSplinePoints = new(5,Allocator.Persistent);
        erosionTerrainHeight = new(5,Allocator.Persistent);

        continentalnessSplinePoints[0] = 0;
        continentalnessSplinePoints[1] = 0.2f;
        continentalnessSplinePoints[2] = 0.4f;
        continentalnessSplinePoints[3] = 0.8f;
        continentalnessSplinePoints[4] = 1f;

        continentalnessTerrainHeight[0] = 30;
        continentalnessTerrainHeight[1] = 50f;
        continentalnessTerrainHeight[2] = 70f;
        continentalnessTerrainHeight[3] = 75f;
        continentalnessTerrainHeight[4] = 101f;



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
                voxels = new NativeArray<Voxel>(world.chunkSize * world.chunkHeight * world.chunkSize, Allocator.TempJob), //If this job takes more than 4 frames, switch to Allocator.Persistent
                
            };

            ChunkGenJob chunkGenJob = new()
            {
                size = world.chunkSize,
                height = world.chunkHeight,
                waterHeight = world.waterHeight,
                seed = seed,
                resolution = world.resolution,
                coords = new int3(chunkCoords.x, chunkCoords.y, chunkCoords.z),

                continentalnessPoints = this.continentalnessSplinePoints,
                continentalnessTerrainHeight = this.continentalnessTerrainHeight,



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