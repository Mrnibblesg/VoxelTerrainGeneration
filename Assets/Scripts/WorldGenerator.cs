using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator World { get; private set; }
    public static ProfilerMarker s_ChunkGen = new(ProfilerCategory.Render, "Chunk.RegenerateMesh"); //Profiling
    public static ProfilerMarker s_GenerateWorld = new(ProfilerCategory.Scripts, "WorldGenerator.GenerateWorld"); //Profiling

    //Dimensions of world in the amount of chunks
    public int worldSize;
    public int worldHeight;

    public int chunkSize;
    public int chunkHeight;

    private Dictionary<Vector3, Chunk> chunks;
    
    void Awake()
    { 
        if (World != null)
        {
            throw new Exception("Only one instance of the WorldGenerator is allowed");
        }
        World = this;

        Chunk.size = chunkSize;
        Chunk.height = chunkHeight;
        chunks = new Dictionary<Vector3, Chunk>();

        s_GenerateWorld.Begin();
        GenerateWorld();
        s_GenerateWorld.End();
    }

    //Generates a world with dimensions worldSize x worldSize chunks.
    void GenerateWorld()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int z = 0; z < worldSize; z++)
                {
                    GameObject chunkObj = new GameObject($"Chunk{x},{y},{z}");
                    Chunk newChunk = chunkObj.AddComponent<Chunk>();

                    //we'll store the chunk's chunk coordinates in the dictionary
                    //but use it's real location for the gameObject. This stuff will need to
                    //be done differently if we use vertical chunks
                    Vector3 position = new Vector3Int(x, y, z);
                    chunks.Add(position, newChunk);

                    position.x *= chunkSize;
                    position.y *= chunkHeight;
                    position.z *= chunkSize;
                    chunkObj.transform.position = position;
                    newChunk.Initialize(new Vector3Int(x, y, z));
                }
            }
        }
        //Hacky, instead want to render the chunk and re-render its
        //neighbors to get ready for procedural gen
        foreach (KeyValuePair<Vector3, Chunk> pair in chunks)
        {
            s_ChunkGen.Begin();
            pair.Value.RegenerateMesh();
            s_ChunkGen.End();
        }
    }

    //vec is in global coordinates
    public Chunk GetChunk(Vector3 vec)
    {
        Chunk c;
        Vector3 chunkCoordinates = new Vector3Int(
            Mathf.FloorToInt(vec.x / chunkSize),
            Mathf.FloorToInt(vec.y / chunkHeight),
            Mathf.FloorToInt(vec.z / chunkSize));
        
        if (chunks.TryGetValue(chunkCoordinates, out c))
        {
            return c;
        }
        return null;
    }

    /// <summary>
    /// Get a voxel from the world
    /// </summary>
    /// <param name="vec"> The world-space position from which the voxel is intended to be acquired from (will be converted to local space.) </param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Voxel GetVoxel(Vector3 vec)
    {
        Chunk c = GetChunk(vec);
        if (c != null)
        {
            return (Voxel) c.GetVoxel(c.transform.InverseTransformPoint(vec));
        }

        else
        {
            throw new Exception("Chunk not found");
        }
    }
    
    /// <summary>
    /// Set a voxel from the world
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="voxel"></param>
    /// <exception cref="Exception"></exception>
    public void SetVoxel(Vector3 vec, Voxel voxel)
    {
        Chunk c = GetChunk(vec);
        if (c != null)
        {
            c.SetVoxel(c.transform.InverseTransformPoint(vec), voxel);
        }
        else
        {
            throw new Exception("Chunk not found");
        }
    }
}
