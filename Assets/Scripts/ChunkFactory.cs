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

    //We use spline points to control how the terrain height value slopes 
    //on different noise intervals via linear interpolation.
    //We use two native arrays because these are the only arrays that can be given
    //to jobs.
    NativeArray<float> continentalnessSplinePoints;
    NativeArray<float> continentalnessFactor;

    NativeArray<float> erosionSplinePoints;
    NativeArray<float> erosionFactor;

    NativeArray<float> peaksAndValleysPoints;
    NativeArray<float> peaksAndValleysFactor;

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
        // (0, 1)
        // (0.3, 50)
        // (0.7, 70)
        // (0.8, 150)
        // (1, 150)
        continentalnessSplinePoints = new(5,Allocator.Persistent);
        continentalnessFactor = new(5,Allocator.Persistent);

        erosionSplinePoints = new(5, Allocator.Persistent);
        erosionFactor = new(5, Allocator.Persistent);

        peaksAndValleysPoints = new(5, Allocator.Persistent);
        peaksAndValleysFactor = new(5, Allocator.Persistent);

        //Manually define the spline points, which map raw noise to a terrain height.
        //Continentalness defines how far inland we are. A value closer to 0 indicates we are
        //further away from land, and closer (or in) the ocean.
        continentalnessSplinePoints[0] = 0;
        continentalnessSplinePoints[1] = 0.5f;
        continentalnessSplinePoints[2] = 0.6f;
        continentalnessSplinePoints[3] = 0.7f;
        continentalnessSplinePoints[4] = 1f;

        continentalnessFactor[0] = 5f;
        continentalnessFactor[1] = 40f;
        continentalnessFactor[2] = 90f;
        continentalnessFactor[3] = 140f;
        continentalnessFactor[4] = 300f;


        //Erosion limits how high the terrain can go at this spot, due to erosion.
        //A higher value of erosion means the terrain is generally lower and flatter.
        erosionSplinePoints[0] = 0;
        erosionSplinePoints[1] = 0.3f;
        erosionSplinePoints[2] = 0.4f;
        erosionSplinePoints[3] = 0.5f;
        erosionSplinePoints[4] = 1f;

        erosionFactor[0] = 1.6f;
        erosionFactor[1] = 1.2f;
        erosionFactor[2] = 1f;
        erosionFactor[3] = 0.7f;
        erosionFactor[4] = 0.3f;

        //Peaks and valleys are small-scale variations in terrain.
        peaksAndValleysPoints[0] = 0;
        peaksAndValleysPoints[1] = 0.3f;
        peaksAndValleysPoints[2] = 0.4f;
        peaksAndValleysPoints[3] = 0.5f;
        peaksAndValleysPoints[4] = 1f;

        peaksAndValleysFactor[0] = -10f;
        peaksAndValleysFactor[1] = -5f;
        peaksAndValleysFactor[2] = 0f;
        peaksAndValleysFactor[3] = 5f;
        peaksAndValleysFactor[4] = 10f;


    }
    ~ChunkFactory()
    {
        continentalnessSplinePoints.Dispose();
        continentalnessFactor.Dispose();

        erosionSplinePoints.Dispose();
        erosionFactor.Dispose();
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

                continentalnessPoints = this.continentalnessSplinePoints,
                continentalnessFactor = this.continentalnessFactor,

                erosionPoints = this.erosionSplinePoints,
                erosionFactor = this.erosionFactor,

                peaksAndValleysPoints = this.peaksAndValleysPoints,
                peaksAndValleysFactor = this.peaksAndValleysFactor,

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