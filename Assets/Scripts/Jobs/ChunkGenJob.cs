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

    [WriteOnly]
    public NativeArray<Voxel> voxels;


    public void Execute()
    {
        RandomVoxels();
        Perlin();
    }
    private void RandomVoxels()
    {
        int voxelAmt = 5;


        Unity.Mathematics.Random r = new(seed);

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    VoxelType type = (VoxelType)r.NextInt(1, voxelAmt - 1);
                    voxels[height*size * x + size*y + z] = new Voxel(type);
                }
            }
        }
    }
    private void Perlin()
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
    }
}
