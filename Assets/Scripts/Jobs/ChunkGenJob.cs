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

    [ReadOnly]
    public NativeArray<float> continentalnessPoints;

    [ReadOnly]
    public NativeArray<float> continentalnessTerrainHeight;


    [WriteOnly]
    public NativeArray<Voxel> voxels;


    public void Execute()
    {
        //Terrain shaping,
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
                float targetHeight = GetContinentalnessHeight(chunkPos.x + xOff, chunkPos.z + zOff);

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

/*    private void Perlin()
    {
        //Max amplitude in world-space
        const int amplitude = 30;
        Unity.Mathematics.Random r = new(seed);
        int o1Offset = r.NextInt(-100000, 100000);
        int o2Offset = r.NextInt(-100000, 100000);
        int o3Offset = r.NextInt(-100000, 100000);

        float3 chunkPos = new float3(
            coords.x * size / resolution,
            coords.y * height / resolution,
            coords.z * size / resolution);

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {

                float2 voxelPos = new float2(x, z) / resolution;
                voxelPos.x += chunkPos.x;
                voxelPos.y += chunkPos.z;

                //We might want to use a perlin noise function that is not from the Mathf library.
                //Mathf isn't particularly suited for burst code. Maybe Unity.Mathematics? or some other library.
                double octave1 = amplitude * Mathf.PerlinNoise((voxelPos.x + o1Offset) / 50, (voxelPos.y + o1Offset) / 50);

                double octave2 = amplitude * Mathf.PerlinNoise((voxelPos.x + o2Offset) / 15, (voxelPos.y + o2Offset) / 15) / 2;

                //I don't understand why the input values need to be small. Dividing by 2 is perfectly fine but not dividing breaks it.
                double octave3 = amplitude * Mathf.PerlinNoise((voxelPos.x + o3Offset) / 5, (voxelPos.y + o3Offset) / 5) / 15;
                double targetHeight = octave1 + octave2 + octave3;
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
    }*/

    /// <summary>
    /// Interpolate based on the supplied spline points
    /// </summary>
    /// <returns>The height based on continentalness for this x and z</returns>
    private float GetContinentalnessHeight(float x, float z)
    {
        //seed and find the noise for the coords
        Unity.Mathematics.Random r = new Unity.Mathematics.Random(seed);
        int offset = r.NextInt(-100000, 100000);
        float noise = Mathf.Clamp(Mathf.PerlinNoise((x+offset)/50, (z+offset)/50),0,1);

        //which spline point has the next value above our noise?
        //should be at least 1
        int point = 1;
        while (point < continentalnessPoints.Length && noise > continentalnessPoints[point])
        {
            point++;
        }
        float first = continentalnessPoints[point - 1];
        float second = continentalnessPoints[point];

        //find spline bounds
        //take the 2 bounds as min and max
        //the value is mapped to be within min and max
        //noise is a % of the way between p1 and p2, so
        //value is the same % of the way between p1 and p2
        float percentage = (noise - first) / (second - first);

        return math.lerp(continentalnessTerrainHeight[point-1], continentalnessTerrainHeight[point], percentage);
        
        //return 
    }
}
