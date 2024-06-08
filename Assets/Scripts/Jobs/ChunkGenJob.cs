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

    //These are all for passing in world gen parameters.
    //Pairs of points & factors define spline graphs.
    [ReadOnly]
    public NativeArray<float> continentalnessPoints;
    [ReadOnly]
    public NativeArray<float> continentalnessFactor;

    [ReadOnly]
    public NativeArray<float> erosionPoints;
    [ReadOnly]
    public NativeArray<float> erosionFactor;

    [ReadOnly]
    public NativeArray<float> peaksAndValleysPoints;
    [ReadOnly]
    public NativeArray<float> peaksAndValleysFactor;

    [ReadOnly]
    public NativeArray<float> tempPoints;
    [ReadOnly]
    public NativeArray<float> tempFactor;

    [ReadOnly]
    public NativeArray<float> humidPoints;
    [ReadOnly]
    public NativeArray<float> humidFactor;

    //The finished array of voxels
    [WriteOnly]
    public NativeArray<Voxel> voxels;

    private enum Biome
    {
        OCEAN,
        MOUNTAINS,
        PLAINS,
        FOREST,
        DESERT,
    }


    public void Execute()
    {
#if PROFILER_ENABLED
        if (worstCase)
        {
            WorstCase();
            return;
        }
#endif
        //Store the noise values in these arrays
        NativeArray<float> continentalness = new(size * size, Allocator.Temp);
        NativeArray<float> erosion = new(size * size, Allocator.Temp);
        NativeArray<float> PV = new(size * size, Allocator.Temp);

        Stone();
        CarveTerrain(continentalness, erosion, PV);
        

    }
    private void Stone()
    {
        for (int i = 0; i < size * height * size; i++)
        {
            voxels[i] = new Voxel(VoxelType.STONE);
        }
    }

    private void CarveTerrain(NativeArray<float> continentalnessArr, NativeArray<float> erosionArr, NativeArray<float> PVArr)
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

                //Save the remapped terrain parameters for use in assigning biomes.
                //For each of these "get X" functions we create a new Unity Math random.
                //There's probably a better way to do this so that doesn't need to happen.
                float continentalness = GetContinentalness(chunkPos.x + xOff, chunkPos.z + zOff, out float rawContinentalness);

                float erosion = GetErosion(chunkPos.x + xOff, chunkPos.z + zOff, out float rawErosion);

                float PV = GetPV(chunkPos.x + xOff, chunkPos.z + zOff, out float rawPV);

                float targetHeight = continentalness * erosion + PV;

                float temp = GetTemp(xOff, zOff, out _);
                float humidity = GetHumidity(xOff, zOff, out _);

                int surfaceDepth = 3;
                int surfacePlaced = 0;
                VoxelType surfaceType;
                VoxelType subSurfaceType;

                switch(DecideBiome(continentalness, erosion, PV, temp, humidity))
                {
                    case Biome.OCEAN:
                        surfaceType = VoxelType.SAND;
                        subSurfaceType = VoxelType.STONE;
                        break;
                    case Biome.PLAINS:
                        surfaceType = VoxelType.GRASS;
                        subSurfaceType = VoxelType.DIRT;
                        break;
                    case Biome.DESERT:
                        surfaceType = VoxelType.SAND;
                        subSurfaceType = VoxelType.SAND;
                        break;
                    default:
                        surfaceType = VoxelType.STONE;
                        subSurfaceType = VoxelType.STONE;
                        break;

                }

                //Finally set the blocks according to if we've reached where the surface
                //should be, and what type of biome we're in.

                //Loop from the index of the target height - subsurface depth upwards
                int startYIndex = (int)Math.Floor((targetHeight - chunkPos.y) / resolution) - surfaceDepth;
                surfacePlaced = math.abs(math.min(0, startYIndex));
                for (int y = Math.Max(0, startYIndex); y < height; y++)
                {
                    VoxelType t;
                    if (surfacePlaced < surfaceDepth)
                    {
                        surfacePlaced++;
                        if (surfacePlaced == surfaceDepth)
                        {
                            t = surfaceType;
                        }
                        else
                        {
                            t = subSurfaceType;
                        }
                    }
                    else
                    {
                        if (chunkPos.y + (y / resolution) <= waterHeight)
                        {
                            t = VoxelType.WATER_SOURCE;
                        }
                        else
                        {
                            t = VoxelType.AIR;
                        }
                    }
                    
                    voxels[height * size * x + size * y + z] = new Voxel(t);
                }
            }
        }
    }
    /// <summary>
    /// Use previously generated values of continentalness, erosion, and peaks & valleys
    /// as well as new values of temperaure and humidity to decide what biome we're in
    /// </summary>
    /// <param name="continentalnessArr"></param>
    /// <param name="erosionArr"></param>
    /// <param name="PVArr"></param>
    private Biome DecideBiome(float cont, float erosion, float PV, float temp, float humidity)
    {
        /*if (temp > 0.5)
        {
            return Biome.DESERT;
        }*/
        return Biome.PLAINS;
    }

    /// <summary>
    /// Interpolate based on the supplied spline points
    /// </summary>
    /// <returns>The height based on continentalness for this x and z</returns>
    private float GetContinentalness(float x, float z, out float rawRemapped)
    {
        //seed and find the noise for the coords
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        int offset = r.NextInt(-100000, 100000);
        //how much the frequency changes per octave
        int lacunarity = 2;

        //how much the amplitude changes per octave
        float gain = 0.5f;

        //our initial loop values
        float frequency = 500;
        float amplitude = 4/7f;

        float noise = OctaveNoise(lacunarity, gain, frequency, amplitude, x, z, offset);
        return ReMapNoise(continentalnessPoints, continentalnessFactor, noise, out rawRemapped);
    }
    private float GetErosion(float x, float z, out float rawRemapped)
    {
        //seed and find the noise for the coords
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        int offset = r.NextInt(100000, 300000);
        //how much the frequency changes per octave
        int lacunarity = 2;

        //how much the amplitude changes per octave
        float gain = 0.5f;

        //our initial loop values
        float frequency = 300;
        float amplitude = 4 / 7f;

        float noise = OctaveNoise(lacunarity, gain, frequency, amplitude, x, z, offset);
        return ReMapNoise(erosionPoints, erosionFactor, noise, out rawRemapped);
    }

    // PV = peaks & valleys
    private float GetPV(float x, float z, out float rawRemapped)
    {
        //seed and find the noise for the coords
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        int offset = r.NextInt(-300000, -100000);
        //how much the frequency changes per octave
        int lacunarity = 2;

        //how much the amplitude changes per octave
        float gain = 0.5f;

        //our initial loop values
        float frequency = 100;
        float amplitude = 4 / 7f;

        //By adding noise in 3 octaves, we achieve fractal brownian motion (fBM)
        float noise = OctaveNoise(lacunarity, gain, frequency, amplitude, x, z, offset);
        return ReMapNoise(peaksAndValleysPoints, peaksAndValleysFactor, noise, out rawRemapped);

    }
    private float GetTemp(float x, float z, out float rawRemapped)
    {
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        int offset = r.NextInt(-500000, -300000);
        //how much the frequency changes per octave
        int lacunarity = 2;

        //how much the amplitude changes per octave
        float gain = 0.5f;

        //our initial loop values
        float frequency = 100;
        float amplitude = 4 / 7f;

        //By adding noise in 3 octaves, we achieve fractal brownian motion (fBM)
        float noise = OctaveNoise(lacunarity, gain, frequency, amplitude, x, z, offset);
        return ReMapNoise(tempPoints, tempFactor, noise, out rawRemapped);
    }
    private float GetHumidity(float x, float z, out float rawRemapped)
    {
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        int offset = r.NextInt(300000, 500000);
        //how much the frequency changes per octave
        int lacunarity = 2;

        //how much the amplitude changes per octave
        float gain = 0.5f;

        //our initial loop values
        float frequency = 300;
        float amplitude = 4 / 7f;

        //By adding noise in 3 octaves, we achieve fractal brownian motion (fBM)
        float noise = OctaveNoise(lacunarity, gain, frequency, amplitude, x, z, offset);
        return ReMapNoise(humidPoints, humidFactor, noise, out rawRemapped);
    }

    /// <summary>
    /// Add three octaves of perlin noise to produce a better noise than simply perlin noise.
    /// </summary>
    /// <param name="lacunarity"></param>
    /// <param name="gain"></param>
    /// <param name="frequency"></param>
    /// <param name="amplitude"></param>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    private float OctaveNoise(float lacunarity, float gain, float frequency, float amplitude, float x, float z, float offset)
    {
        float noise = 0;
        //By adding noise in 3 octaves, we achieve octave noise
        for (int i = 0; i < 3; i++)
        {
            noise += Mathf.PerlinNoise((x + offset) / frequency, (z + offset) / frequency) * amplitude;
            frequency *= lacunarity;
            amplitude *= gain;
        }
        return noise;
    }

    /// <summary>
    /// Locate the index of the spline point that serves as the upper bound of the given noise.
    /// </summary>
    /// <returns></returns>
    private int FindSplineUpperBound(NativeArray<float> splinePoints, float noiseValue)
    {
        int point = 1;
        while (point < splinePoints.Length-1 && noiseValue > splinePoints[point])
        {
            point++;
        }
        return point;
    }
    /// <summary>
    /// Given a noise, return the height value associated based on the given points and heights.
    /// rawRemapped is the smoothed value.
    /// </summary>
    private float ReMapNoise(NativeArray<float> points, NativeArray<float> heights, float noise, out float rawRemapped)
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
        //value is the same % of the way between p1 value and p2 value
        float percentage = (noise - first) / (second - first);
        //percentage is for math.lerp-ing only.
        //for smoothstep just use noise and then use the smoothed value

        float smoothed = math.smoothstep(
            points[splineUpperBound - 1],
            points[splineUpperBound],
            noise);
        rawRemapped = smoothed;

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
