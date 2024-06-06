using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using System;

//To fully take advantage of the Burst compiler we should use 
[BurstCompile(CompileSynchronously = true)]
public struct ChunkGenJob : IJob
{
    //Everything we need to generate a chunk
    [ReadOnly]
    public int size;
    public int height;
    public int waterHeight;
    public uint seed;
    public float resolution;
    public int3 coords;
#if PROFILER_ENABLED 
    public bool worstCase;
#endif

    [ReadOnly]
    public NativeArray<float> continentalnessPoints;

    [ReadOnly]
    public NativeArray<float> continentalnessFactor;

    [ReadOnly]
    public NativeArray<float> erosionPoints;

    [ReadOnly]
    public NativeArray<float> erosionFactor;

    [WriteOnly]
    public NativeArray<Voxel> voxels;


    public void Execute()
    {
#if PROFILER_ENABLED
        if (worstCase)
        {
            WorstCase();
            return;
        }
#endif

        Grass();
        CarveTerrain();


        //Decorating terrain based on biome

        


        //Perlin(); // very basic terrain shaping
    }
    private void Grass()
    {
        for (int i = 0; i < size * height * size; i++)
        {
            voxels[i] = new Voxel(VoxelType.GRASS);
        }
    }

    private void CarveTerrain()
    {
        float3 chunkPos = new float3(
            coords.x * size / resolution,
            coords.y * height / resolution,
            coords.z * size / resolution);

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                float xOff = x / resolution;
                float zOff = z / resolution;
                float continentalness = GetContinentalness(chunkPos.x + xOff, chunkPos.z + zOff);
                float erosion = GetErosion(chunkPos.x + xOff, chunkPos.z + zOff);
                float targetHeight = continentalness * erosion;

                for (int y = height - 1; y >= 0; y--)
                {
                    if (chunkPos.y + (y / resolution) <= targetHeight)
                    {
                        break;
                    }

                    if (chunkPos.y + (y / resolution) <= waterHeight)
                    {
                        voxels[height * size * x + size * y + z] = new Voxel(VoxelType.WATER_SOURCE);
                    }
                    else
                    {
                        voxels[height * size * x + size * y + z] = new Voxel(VoxelType.AIR);
                    }
                }
            }
        }
        
    }

    /// <summary>
    /// Interpolate based on the supplied spline points
    /// </summary>
    /// <returns>The height based on continentalness for this x and z</returns>
    private float GetContinentalness(float x, float z)
    {
        //seed and find the noise for the coords
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        int offset = r.NextInt(-100000, 100000);
        float noise = 0;
        //how much the frequency changes per octave
        int lacunarity = 2;

        //how much the amplitude changes per octave
        float gain = 0.5f;

        //our initial loop values
        float frequency = 500;
        float amplitude = 4/7f;

        //By adding noise in 3 octaves, we achieve fractal brownian motion (fBM)
        for (int i = 0; i < 3; i++)
        {
            noise += Mathf.PerlinNoise((x + offset) / frequency, (z + offset) / frequency) * amplitude;
            frequency *= lacunarity;
            amplitude *= gain;
        }
        
        
        return GetSplineHeight(continentalnessPoints, continentalnessFactor, noise);
    }
    private float GetErosion(float x, float z)
    {
        //seed and find the noise for the coords
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        int offset = r.NextInt(100000, 200000);
        float noise = 0;
        //how much the frequency changes per octave
        int lacunarity = 2;

        //how much the amplitude changes per octave
        float gain = 0.5f;

        //our initial loop values
        float frequency = 300;
        float amplitude = 4 / 7f;

        //By adding noise in 3 octaves, we achieve fractal brownian motion (fBM)
        for (int i = 0; i < 3; i++)
        {
            noise += Mathf.PerlinNoise((x + offset) / frequency, (z + offset) / frequency) * amplitude;
            frequency *= lacunarity;
            amplitude *= gain;
        }

        return GetSplineHeight(erosionPoints, erosionFactor, noise);
    }
    private int FindSplineUpperBound(NativeArray<float> splinePoints, float noiseValue)
    {
        int point = 1;
        while (point < splinePoints.Length-1 && noiseValue > splinePoints[point])
        {
            point++;
        }
        return point;
    }
    private float GetSplineHeight(NativeArray<float> points, NativeArray<float> heights, float noise)
    {
        //which spline point has the next value above our noise?
        //should be at least 1
        int splineUpperBound = FindSplineUpperBound(points, noise);
        float first = points[splineUpperBound - 1];
        float second = points[splineUpperBound];

        //find spline bounds
        //take the 2 bounds as min and max
        //the value is mapped to be within min and max
        //noise is a % of the way between p1 and p2, so
        //value is the same % of the way between p1 and p2
        float percentage = (noise - first) / (second - first);
        //percentage is for math.lerp-ing only.
        //for smoothstep just use noise and then use the smoothed value

        float smoothed = math.smoothstep(
            points[splineUpperBound - 1],
            points[splineUpperBound],
            noise);

        return math.lerp(
            heights[splineUpperBound - 1],
            heights[splineUpperBound],
            smoothed
        );
    }

    /// <summary>
    /// Alternate voxels between some block and air
    /// </summary>
    private void WorstCase()
    {
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < height; y++)
                {
                    voxels[height * size * x + size * y + z] =
                        (x + y + z) % 2 == 0 ? new Voxel(VoxelType.AIR) : new Voxel(VoxelType.DIRT);
                }
            }
        }
    }
}
