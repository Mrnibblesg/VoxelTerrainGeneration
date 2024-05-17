using System;
using System.Collections.Generic;
using UnityEngine;


// TODO In the future, this should *probably* only contain world info, chunks, and get chunks.
//Player related logic might be better suited for the player class or similar,
//as we get ready for multiplayer setups.
public class World
{

    public readonly Material vertexColorMaterial = (Material) Resources.Load("Textures/Vertex Colors");

    //Height of world in chunks
    public int worldHeight;

    //Dimensions of chunk in the amount of voxels
    public readonly int chunkSize;
    public readonly int chunkHeight;

    public readonly float resolution;

    //Use queues to dictate which order chunks are loaded and unloaded.
    //This will be threaded. TODO.

    private Dictionary<Vector3Int, Chunk> chunks;
    private HashSet<Vector3Int> chunksInProg; //Chunks that have been queued to generate
    //TODO: Switch to priority queue. Prioritize nearest chunks.
    private Queue<Vector3Int> loadQueue; //contains chunkCoord of unloaded chunks
    //TODO: make a priority queue, furthest chunks unloaded first
    private Queue<Vector3Int> unloadQueue; //contains loaded chunks

    //TODO: Make a priority queue. Nearest first.
    private HashSet<Vector3Int> unloadedNeighbors;

    private Vector3Int playerChunkPos;
    private int playerLoadDist;
    private int playerUnloadDist;

    ChunkFactory chunkFactory;

    public World(int worldHeight, int chunkSize, int chunkHeight, float resolution)
    {
        this.worldHeight = worldHeight;
        this.chunkSize = chunkSize;
        this.chunkHeight = chunkHeight;
        this.resolution = resolution;

        chunks = new();
        unloadedNeighbors = new();
        chunksInProg = new();

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
    public void UpdatePlayerChunkPos(Vector3Int chunkCoord, int renderDist, int unloadDist)
    {
        //For chained chunk updates if the player doesn't leave their chunk for a while
        playerChunkPos = chunkCoord;
        playerLoadDist = renderDist;
        playerUnloadDist = unloadDist;

        //If player chunk is unloaded, then start a new neighbor chunk thing here,
        //starting a new loaded chunk structure centered around this position.
        if (!chunks.ContainsKey(chunkCoord))
        {
            unloadedNeighbors.Add(chunkCoord);
        }
        
        //Queue furthest chunks to unload. Only done when player coord changes.
        foreach (KeyValuePair<Vector3Int, Chunk> p in chunks)
        {
            if (Vector3Int.Distance(p.Key, chunkCoord) > playerUnloadDist)
            {
                unloadQueue.Enqueue(p.Key);
            }
        }

        UpdateNeighborQueues();
    }
    private void UpdateNeighborQueues()
    {
        while (unloadQueue.Count != 0)
        {
            UnloadChunk(unloadQueue.Dequeue());
        }

        //Queue nearby chunks to load. Only if in range and not currently scheduled.
        foreach (Vector3Int unloaded in unloadedNeighbors)
        {
            if (Vector3Int.Distance(unloaded, playerChunkPos) < playerLoadDist &&
                !chunksInProg.Contains(unloaded))
            {
                loadQueue.Enqueue(unloaded);
            }
        }

        while (loadQueue.Count != 0)
        {
            LoadChunk(loadQueue.Dequeue());
        }
    }

    /// <summary>
    /// Gets a chunk either from memory, or from chunk generator
    /// </summary>
    /// <param name="chunkCoords"></param>
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
        chunkObj.transform.position = new(
            position.x * WorldController.Controller.chunkSize / resolution,
            position.y * WorldController.Controller.chunkHeight / resolution,
            position.z * WorldController.Controller.chunkSize / resolution
        );

        Chunk newChunk = chunkObj.AddComponent<Chunk>();
        chunkFactory.GenerateChunk(newChunk, position);

        //chunks.Add(position, newChunk);
        ChunkFinished(chunkCoords, newChunk);

        newChunk.RegenerateMesh();
        RefreshNeighbors(position);

    }
    /// <summary>
    /// Intended to be used as a callback function.
    /// Called when a chunk is finished generating.
    /// </summary>
    /// <param name="chunkCoords"></param>
    //While a chunk is loading, it remains as a neighbor chunk but is added to loading in progress set.
    //When a chunk finishes loading, add its neighbors to the unloaded neighbors set if not loaded.
    //  They get queued as well if they're in range.
    private void ChunkFinished(Vector3Int chunkCoords, Chunk c)
    {
        //add to chunks
        chunks.Add(chunkCoords, c);
        
        chunksInProg.Remove(chunkCoords);
        unloadedNeighbors.Remove(chunkCoords);

        AddUnloadedNeighbors(chunkCoords);

        //Queue up more unloaded neighbors to generate if they're in range
        UpdateNeighborQueues();
    }
    /// <summary>
    /// Dispose of a chunk plus extra necessary bookkeeping.
    /// </summary>
    /// <param name="chunkCoords"></param>
    public void UnloadChunk(Vector3Int chunkCoords)
    {
#if UNITY_EDITOR //Apparently use this if we're in the editor otherwise the destroy is ignored?
        GameObject.DestroyImmediate(chunks[chunkCoords].gameObject);
#else
        GameObject.Destroy(chunks[chunkCoords].gameObject);
#endif
        chunks.Remove(chunkCoords);
        unloadedNeighbors.Add(chunkCoords);

        TryRemoveUnloadedNeighbors(chunkCoords);
        TryRemoveUnloaded(chunkCoords);

        RefreshNeighbors(chunkCoords);
    }

    /// <summary>
    /// Try removing all unloaded neighbors. Maybe a source of inefficiency, 36 accesses per use.
    /// </summary>
    /// <param name="chunkCoord"></param>
    private void TryRemoveUnloadedNeighbors(Vector3Int chunkCoord)
    {
        TryRemoveUnloaded(chunkCoord + Vector3Int.up);
        TryRemoveUnloaded(chunkCoord + Vector3Int.down);
        TryRemoveUnloaded(chunkCoord + Vector3Int.left);
        TryRemoveUnloaded(chunkCoord + Vector3Int.right);
        TryRemoveUnloaded(chunkCoord + Vector3Int.forward);
        TryRemoveUnloaded(chunkCoord + Vector3Int.back);
    }
    /// <summary>
    /// Remove an unloaded neighbor chunk if it is not a neighbor to a loaded chunk
    /// </summary>
    /// <param name="chunkCoord"></param>
    private void TryRemoveUnloaded(Vector3Int chunkCoord)
    {
        if (chunks.ContainsKey(chunkCoord + Vector3Int.up) ||
            chunks.ContainsKey(chunkCoord + Vector3Int.down) ||
            chunks.ContainsKey(chunkCoord + Vector3Int.left) ||
            chunks.ContainsKey(chunkCoord + Vector3Int.right) ||
            chunks.ContainsKey(chunkCoord + Vector3Int.forward) ||
            chunks.ContainsKey(chunkCoord + Vector3Int.back))
        {
            return;
        }
        unloadedNeighbors.Remove(chunkCoord);
    }

    /// <summary>
    /// Attempt to add neighbors of this chunk to the unloaded neighbor set
    /// </summary>
    /// <param name="chunkCoord"></param>
    private void AddUnloadedNeighbors(Vector3Int chunkCoord)
    {
        void addIfNotLoaded(Vector3Int neighborCoord)
        {
            if (!chunks.ContainsKey(neighborCoord))
            {
                unloadedNeighbors.Add(neighborCoord);
            }
        }
        addIfNotLoaded(chunkCoord + Vector3Int.up);
        addIfNotLoaded(chunkCoord + Vector3Int.down);
        addIfNotLoaded(chunkCoord + Vector3Int.left);
        addIfNotLoaded(chunkCoord + Vector3Int.right);
        addIfNotLoaded(chunkCoord + Vector3Int.forward);
        addIfNotLoaded(chunkCoord + Vector3Int.back);
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
            Mathf.FloorToInt(global.x / (WorldController.Controller.chunkSize / resolution)),
            Mathf.FloorToInt(global.y / (WorldController.Controller.chunkHeight / resolution)),
            Mathf.FloorToInt(global.z / (WorldController.Controller.chunkSize / resolution)));

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
