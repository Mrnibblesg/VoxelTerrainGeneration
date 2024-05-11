using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


// TODO In the future, this should *probably* only contain world info, chunks, and get chunks.
//Player related logic might be better suited for the player class or similar,
//as we get ready for multiplayer setups.
public class World
{
    private Dictionary<Vector3Int, Chunk> chunks;
    public readonly Material vertexColorMaterial = (Material) Resources.Load("Textures/Vertex Colors");

    //Height of world in chunks
    public int worldHeight;

    //Dimensions of chunk in the amount of voxels
    public int chunkSize;
    public int chunkHeight;

    //Use queues to dictate which order chunks are loaded and unloaded.
    //This will be threaded. TODO.
    private Queue<Vector3Int> loadQueue;
    private Queue<Vector3Int> unloadQueue;

    ChunkFactory chunkFactory;

    public World(int worldHeight, int chunkSize, int chunkHeight)
    {
        this.worldHeight = worldHeight;
        this.chunkSize = chunkSize;
        this.chunkHeight = chunkHeight;
        chunks = new();
        loadQueue = new();
        unloadQueue = new();
        chunkFactory = new(this);
    }

    /// <summary>
    /// Updates the loaded and unloaded area surrounding the player.
    /// Chunks that come in range or go out of range are added to the
    /// load/unload queue.
    /// </summary>
    /// <param name="chunkCoord"></param>
    public void UpdateNearbyChunks(Vector3Int chunkCoord, int renderDist, int unloadDist)
    {
        //loop in growing taxicab distance
        //Keep list of unloaded neighbor chunks? Check neighbor chunk list only.
        //If a player were to teleport,

        //Currently just attempt to load in a cube. Probably inefficient. TODO
        for (int dx = -renderDist; dx < renderDist; dx++)
        {
            for (int dy = -renderDist; dy < renderDist; dy++)
            {
                for (int dz = -renderDist; dz < renderDist; dz++)
                {
                    Vector3Int currentCoord = new Vector3Int(
                        chunkCoord.x + dx,
                        chunkCoord.y + dy,
                        chunkCoord.z + dz);

                    if (currentCoord.y >= worldHeight || currentCoord.y < 0)
                    {
                        continue;
                    }
                    if (!chunks.ContainsKey(currentCoord))
                    {
                        LoadChunk(currentCoord);
                    }
                }
            }
        }

        //Can't edit values within a foreach loop. Add to a queue.
        foreach(KeyValuePair<Vector3Int, Chunk> pair in chunks)
        {
            if (Vector3Int.Distance(pair.Key, chunkCoord) > unloadDist)
            {
                unloadQueue.Enqueue(pair.Key);
            }
        }

        //Unload chunks. This, along with loading chunks should be threaded
        while (unloadQueue.Count > 0)
        {
            UnloadChunk(unloadQueue.Dequeue());

        }
    }
    private void LoadChunk(Vector3Int chunkCoords)
    {
        Chunk c = GetChunk(chunkCoords);
        if (c)
        {
            //don't generate duplicate chunks
            return;
        }
        //if no chunk, configure and generate new one
        int x = chunkCoords.x;
        int y = chunkCoords.y;
        int z = chunkCoords.z;
        GameObject chunkObj = new GameObject($"Chunk{x},{y},{z}");

        Vector3Int position = new Vector3Int(x, y, z);
        Chunk newChunk = chunkObj.AddComponent<Chunk>();
        chunkFactory.GenerateChunk(newChunk, position);
        //newChunk.Initialize(this);
        chunks.Add(position, newChunk);

        chunkObj.transform.position = new(
            position.x * WorldController.Controller.chunkSize,
            position.y * WorldController.Controller.chunkHeight,
            position.z * WorldController.Controller.chunkSize
        );

        newChunk.RegenerateMesh();
        RefreshNeighbors(position);

    }
    public void UnloadChunk(Vector3Int chunkCoords)
    {
#if UNITY_EDITOR //Apparently use this if we're in the editor otherwise the destroy is ignored?
        GameObject.DestroyImmediate(chunks[chunkCoords].gameObject);
#else
        GameObject.Destroy(chunks[chunkCoords].gameObject);
#endif
        chunks.Remove(chunkCoords);
        RefreshNeighbors(chunkCoords); 
    }

    //When a neighbor chunk is rendered, use this to refresh neighboring chunk meshes.
    //Honestly inefficient...
    //Render 6 chunks for the price of 1!!
    private void RefreshNeighbors(Vector3Int chunkCoord)
    {
        RefreshChunk(chunkCoord + Vector3Int.left);
        RefreshChunk(chunkCoord + Vector3Int.right);
        RefreshChunk(chunkCoord + Vector3Int.up);
        RefreshChunk(chunkCoord + Vector3Int.down);
        RefreshChunk(chunkCoord + Vector3Int.back);
        RefreshChunk(chunkCoord + Vector3Int.forward);
    }

    /// <summary>
    /// For when a chunk should be re-rendered or something, like when a neighbor 
    /// loads and the supplied one should reload.
    /// </summary>
    /// <param name="c"></param>
    private void RefreshChunk(Vector3Int chunkCoord)
    {
        Chunk c = GetChunk(chunkCoord);
        if (c != null)
        {
            c.RegenerateMesh();
        }
    }

    //Retrieve chunk from chunk coordinates
    public Chunk GetChunk(Vector3Int chunkCoords)
    {
        if (chunks.TryGetValue(chunkCoords, out Chunk c))
        {
            return c;
        }

        return null;
    }

    //Retrieve chunk from global coordinates
    public Chunk ChunkFromGlobal(Vector3 global)
    {
        Vector3Int chunkCoordinates = new Vector3Int(
            Mathf.FloorToInt(global.x / WorldController.Controller.chunkSize),
            Mathf.FloorToInt(global.y / WorldController.Controller.chunkHeight),
            Mathf.FloorToInt(global.z / WorldController.Controller.chunkSize));

        return GetChunk(chunkCoordinates);
    }

    /// <summary>
    /// Get a voxel from the world
    /// </summary>
    /// <param name="vec"> The world-space position from which the voxel is intended to be acquired from (will be converted to local space.) </param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Voxel? VoxelFromGlobal(Vector3 vec)
    {
        Chunk c = ChunkFromGlobal(vec);
        if (c != null)
        {
            return (Voxel)c.GetVoxel(c.transform.InverseTransformPoint(vec));
        }
        return null;
    }

    /// <summary>
    /// Set a voxel from the given world-space position
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="voxel"></param>
    /// <exception cref="Exception"></exception>
    public void SetVoxel(Vector3 vec, VoxelType type)
    {
        SetVoxel(vec, new Voxel(type));
    }
    /// <summary>
    /// Set a voxel from the given world-space position
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="voxel"></param>
    /// <exception cref="Exception"></exception>
    public void SetVoxel(Vector3 vec, Voxel voxel)
    {
        Chunk c = ChunkFromGlobal(vec);
        if (c != null)
        {
            c.SetVoxel(c.transform.InverseTransformPoint(vec), voxel);
        }
    }
}
