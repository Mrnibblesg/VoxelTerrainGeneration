using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator World { get; private set; }

    public int worldSize;
    public int chunkSize = 32;
    public int worldHeight = 128;
    private Dictionary<Vector3, Chunk> chunks;
    
    void Start()
    { 
        if (World != null)
        {
            throw new Exception("Only one instance of the WorldGenerator is allowed");
        }
        World = this;

        Chunk.size = chunkSize;
        Chunk.height = worldHeight;
        chunks = new Dictionary<Vector3, Chunk>();

        GenerateWorld();
    }

    //Generates a world with dimensions worldSize x worldSize chunks.
    void GenerateWorld()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                GameObject chunkObj = new GameObject($"Chunk{x},{z}");

                //we'll store the chunk's chunk coordinates in the dictionary
                //but use it's real location for the gameObject. This stuff will need to
                //be done differently if we use vertical chunks
                Vector3 position = new Vector3Int(x, 0, z);
                chunkObj.transform.position = position * chunkSize;

                Chunk newChunk = chunkObj.AddComponent<Chunk>();
                newChunk.Initialize(new Vector3Int(x, 0, z));
                chunks.Add(position, newChunk);

            }
        }
        //Hacky, instead want to render the chunk and re-render its
        //neighbors to get ready for procedural gen
        foreach (KeyValuePair<Vector3, Chunk> pair in chunks)
        {
            pair.Value.Render();
        }
    }

    //vec is in global coordinates
    public Chunk GetChunk(Vector3 vec)
    {
        Chunk c;
        Vector3 chunkCoordinates = new Vector3Int(
            Mathf.FloorToInt(vec.x / chunkSize),
            Mathf.FloorToInt(vec.y / worldHeight),
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
