using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The chunk factory is in charge of creating chunks. Chunk creation may be complex
//so it's good to split the logic away from the World class.
//We not only need to generate the structure of terrain, but we then need to 
//determine what voxels are what.
public class ChunkFactory
{
    int seed;
    World world;

    public ChunkFactory(World world)
    {
        seed = 0;
        this.world = world;
    }
    public Chunk GenerateChunk(Chunk c, Vector3Int chunkCoords)
    {
        c.Initialize(world);
        RandomVoxels(c, chunkCoords);
        //SineFunction(c, chunkCoords);
        Perlin(c, chunkCoords);
        return c;
    }

    private void RandomVoxels(Chunk c, Vector3Int coords)
    {
        //Delegate this to some kind of chunk generator factory
        Array types = Enum.GetValues(typeof(VoxelType));

        //Seeds don't really work the same way for a System random object.
        //This generates random numbers one after another, whereas other noise
        //functions are seeded then generate based off of space.
        //so we gotta do it the weird way here
        System.Random r = new System.Random(int.Parse($"{coords.x}{Math.Abs(coords.y)}{Math.Abs(coords.z)}"));

        for (int x = 0; x < world.chunkSize; x++)
        {
            for (int y = 0; y < world.chunkHeight; y++)
            {
                for (int z = 0; z < world.chunkSize; z++)
                {
                    VoxelType type = (VoxelType)types.GetValue(r.Next(1, types.Length - 1));
                    c.voxels[x, y, z] = new Voxel(type, false);
                }
            }
        }

        for (int x = 0; x < world.chunkSize; x++)
        {
            for (int y = 0; y < world.chunkHeight; y++)
            {
                for (int z = 0; z < world.chunkSize; z++)
                {
                    c.MarkExposed(x, y, z);
                }
            }
        }
    }
    private void SineFunction(Chunk c, Vector3Int chunkCoords)
    {
        Vector3 chunkPos = new Vector3(chunkCoords.x * world.chunkSize / world.resolution,
            chunkCoords.y * world.chunkHeight / world.resolution,
            chunkCoords.z * world.chunkSize / world.resolution);
        for (int x = 0; x < world.chunkSize; x++)
        {
            for (int y = 0; y < world.chunkHeight; y++)
            {
                for (int z = 0; z < world.chunkSize; z++)
                {
                    Vector3 voxelPos = chunkPos + (new Vector3(x, y, z) / world.resolution);
                    if (voxelPos.y > 10*Math.Sin((voxelPos.x)/16)*Math.Cos((voxelPos.z) / -16) + 10)
                    {
                        c.voxels[x, y, z].SetType(VoxelType.AIR);
                    }
                }
            }
        }
    }
    private void Perlin(Chunk c, Vector3Int chunkCoords)
    {

    }
}