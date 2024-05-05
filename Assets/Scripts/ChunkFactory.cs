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
                    c.voxels[x, y, z] = new Voxel(
                         type,//(x + z) % 2 == 0 ? VoxelType.Type.GRASS : VoxelType.Type.DIRT, 
                         false, false
                    //(type == VoxelType.Type.GLASS ? true : false)
                    //,(x + y + z) % 2 == 1
                    );
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
}