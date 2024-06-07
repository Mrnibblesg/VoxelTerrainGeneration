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

    NativeArray<float> tempPoints;
    NativeArray<float> tempFactor;

    NativeArray<float> humidityPoints;
    NativeArray<float> humidityFactor;

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

        //Manually define the spline points, which map raw noise to a terrain height.

        //Continentalness defines how far inland we are. A value closer to 0 indicates we are
        //further away from land, and closer (or in) the ocean.
        continentalnessSplinePoints = new(new float[]
        { 
            0,
            0.5f,
            0.6f,
            0.7f,
            1f
        }, Allocator.Persistent);
        continentalnessFactor = new(new float[]
        {
            5,
            40f,
            90f,
            140f,
            300f
        }, Allocator.Persistent);


        //Erosion limits how high the terrain can go at this spot, due to erosion.
        //A higher value of erosion means the terrain is generally lower and flatter.
        erosionSplinePoints = new(new float[]
        {
            0,
            0.3f,
            0.4f,
            0.5f,
            1f
        }, Allocator.Persistent);
        erosionFactor = new(new float[]
        {
            1.6f,
            1.2f,
            1f,
            0.7f,
            0.3f
        }, Allocator.Persistent);


        //Peaks and valleys are small-scale variations in terrain.
        peaksAndValleysPoints = new(new float[]
        {
            0,
            0.3f,
            0.4f,
            0.5f,
            1f
        }, Allocator.Persistent);
        peaksAndValleysFactor = new(new float[]
        {
            -10f,
            -5f,
            0f,
            5f,
            10f
        }, Allocator.Persistent);

        //Temperature is only useful for deciding which biomes go where.
        tempPoints = new(new float[]
        {
            0,
            0.2f,
            0.8f,
            1f,
        }, Allocator.Persistent);
        tempFactor = new(new float[]
        {
            0,
            0.5f,
            0.5f,
            1f,
        }, Allocator.Persistent);

        //Humidity is only useful for deciding which biomes go where.
        humidityPoints = new(new float[]
        {
            0,
            0.3f,
            0.7f,
            1f
        }, Allocator.Persistent);

        humidityFactor = new(new float[]
        {
            0,
            0.5f,
            0.5f,
            1f,
        }, Allocator.Persistent);

    }
    ~ChunkFactory()
    {
        continentalnessSplinePoints.Dispose();
        continentalnessFactor.Dispose();

        erosionSplinePoints.Dispose();
        erosionFactor.Dispose();

        peaksAndValleysPoints.Dispose();
        peaksAndValleysFactor.Dispose();

        tempPoints.Dispose();
        tempFactor.Dispose();

        humidityPoints.Dispose();
        humidityFactor.Dispose();
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

                tempPoints = this.tempPoints,
                tempFactor = this.tempFactor,

                humidPoints = this.humidityPoints,
                humidFactor = this.humidityFactor,

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