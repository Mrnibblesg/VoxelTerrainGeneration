using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using static UnityEditor.PlayerSettings;


// TODO In the future, this should *probably* only contain world info, chunks, and get chunks.
//Player related logic might be better suited for the player class or similar,
//as we get ready for multiplayer setups.
public class World
{
    public readonly Material vertexColorMaterial = (Material) Resources.Load("Textures/Vertex Colors");

    public WorldParameters parameters;

    private List<Agent> players = new List<Agent>();

    //Use queues to dictate which order chunks are loaded and unloaded.
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

    private static ProfilerMarker s_moveChunk = new ProfilerMarker(ProfilerCategory.Scripts, "Move Chunk");

    public World(WorldParameters worldParams)
    {
        this.parameters = worldParams;

        chunks = new();
        unloadedNeighbors = new();
        chunksInProg = new();

        loadQueue = new();
        unloadQueue = new();

        chunkFactory = new(this);
    }

    /// <summary>
    /// Updates the loaded and unloaded area surrounding the supplied agent.
    /// Chunks that come in range or go out of range are added to the
    /// load/unload queue. TODO this shouldn't really be in World. It should be in something that interacts with World, as well as the loaded chunks.
    /// </summary>
    /// <param name="chunkCoord"></param>
    public void UpdateAuthAgentChunkPos(AuthoritativeAgent agent)
    {
        int renderDist = agent.RenderDist;
        int unloadDist = agent.UnloadDist;
        Vector3Int chunkCoord = agent.chunkCoord;

        //Add 1 to compensate for the current chunk mesh method
        renderDist *= (int)parameters.Resolution;
        unloadDist *= (int)parameters.Resolution;

        renderDist += 1;
        unloadDist += 1;

        s_moveChunk.Begin();
        
        //For when a chunk finishes and new jobs must be queued
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
        s_moveChunk.End();
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
            return;
        }
        if (ChunkWithinWorld(chunkCoords)) 
        {
            chunksInProg.Add(chunkCoords);
            chunkFactory.RequestNewChunk(chunkCoords);
        }
    }

    /// <param name="chunkCoords"></param>
    /// <returns>True if chunk is within world bounds</returns>
    private bool ChunkWithinWorld(Vector3Int chunkCoords)
    {
        return
            chunkCoords.y < parameters.WorldHeightInChunks &&
            chunkCoords.y >= 0;
    }

    /// <summary>
    /// Called when a chunk finishes generating. Builds a new chunk.
    /// </summary>
    /// <param name="chunkCoords"></param>
    //While a chunk is loading, it remains as a neighbor chunk but is added to loading in progress set.
    //When a chunk finishes loading, add its neighbors to the unloaded neighbors set if not loaded.
    //  They get queued as well if they're in range.
    public void ChunkFinished(Vector3Int chunkCoords, VoxelRun voxels)
    {
        //Protect against loading chunks that aren't needed anymore
        if (!chunksInProg.Contains(chunkCoords))
        {
            return;
        }

        GameObject chunkObj = new GameObject($"Chunk{chunkCoords.x},{chunkCoords.y},{chunkCoords.z}");
        chunkObj.transform.position = new(
            chunkCoords.x * parameters.ChunkSize / (float)parameters.Resolution,
            chunkCoords.y * parameters.ChunkHeight / (float)parameters.Resolution,
            chunkCoords.z * parameters.ChunkSize / (float)parameters.Resolution
        );

        Chunk newChunk = chunkObj.AddComponent<Chunk>();
        newChunk.Initialize(this);
        newChunk.voxels = voxels;

        //add to chunks
        chunks.Add(chunkCoords, newChunk);

        //neighbor chunk bookkeeping
        chunksInProg.Remove(chunkCoords);
        unloadedNeighbors.Remove(chunkCoords);

        AddUnloadedNeighbors(chunkCoords);

        //Queue up new unloaded neighbors to generate if they're in range
        UpdateNeighborQueues();

        //Now that this chunk has had its terrain generated, we should see if the neighboring chunks should generate their meshes.
        TryMeshNeighbors(chunkCoords);
        TryMesh(chunkCoords);
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

        //Extra bookkeeping
        TryRemoveUnloadedNeighbors(chunkCoords);
        TryRemoveUnloaded(chunkCoords);
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
    /// <summary>
    /// When a chunk has its terrain generated, use this to alert its neighboring chunks
    /// so they can attempt to create a mesh.
    /// </summary>
    /// <param name="chunkCoord"></param>
    private void TryMeshNeighbors(Vector3Int chunkCoord)
    {
        TryMesh(chunkCoord + Vector3Int.left);
        TryMesh(chunkCoord + Vector3Int.right);
        TryMesh(chunkCoord + Vector3Int.up);
        TryMesh(chunkCoord + Vector3Int.down);
        TryMesh(chunkCoord + Vector3Int.back);
        TryMesh(chunkCoord + Vector3Int.forward);
    }

    /// <summary>
    /// Tell a chunk to regenerate its mesh if all its valid chunk neighbors have
    /// their terrains generated.
    /// </summary>
    /// <param name="c"></param>
    private void TryMesh(Vector3Int chunkCoord)
    {
        //should fail until the last chunk neighbor has terrain generated
        
        bool valid(Vector3Int neighbor)
        {
            return chunks.ContainsKey(neighbor) || !ChunkWithinWorld(neighbor);
        }
        Chunk c = GetChunk(chunkCoord);
        if (c != null &&
            valid(chunkCoord + Vector3Int.left) &&
            valid(chunkCoord + Vector3Int.right) &&
            valid(chunkCoord + Vector3Int.up) &&
            valid(chunkCoord + Vector3Int.down) &&
            valid(chunkCoord + Vector3Int.back) &&
            valid(chunkCoord + Vector3Int.forward))
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
            Mathf.FloorToInt(global.x / (parameters.ChunkSize / parameters.Resolution)),
            Mathf.FloorToInt(global.y / (parameters.ChunkHeight / parameters.Resolution)),
            Mathf.FloorToInt(global.z / (parameters.ChunkSize / parameters.Resolution)));

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
            return (Voxel)c.VoxelFromLocal(c.transform.InverseTransformPoint(vec));
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
    public void SetVoxels(List<Vector3> vec, List<VoxelType> type)
    {
        List<Voxel> voxel = new List<Voxel>();
        for (int i = 0; i < type.Count; i++)
        {
            voxel.Add(new Voxel(type[i]));
        }
        SetVoxels(vec, voxel);
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
            c.SetVoxelFromLocal(c.transform.InverseTransformPoint(vec), voxel);
        }
    }
    /// <summary>
    /// Set a list of voxels from the given world-space positions
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="voxel"></param>
    /// <exception cref="Exception"></exception>
    public void SetVoxels(List<Vector3> vec, List<Voxel> voxel)
    {
        Dictionary<Chunk, Tuple<List<Vector3>, List<Voxel>>> chunks = new Dictionary<Chunk, Tuple<List<Vector3>, List<Voxel>>>();
        for (int i = 0; i < vec.Count; i++)
        {
            Chunk c = ChunkFromGlobal(vec[i]);
            if (c == null)
            {
                continue;
            }
            
            if (!chunks.ContainsKey(c))
            {
                List<Vector3> vecList = new List<Vector3>();
                List<Voxel> voxelList = new List<Voxel>();
                Tuple<List<Vector3>, List<Voxel>> container = new Tuple<List<Vector3>, List<Voxel>>(vecList, voxelList);
                chunks.Add(c, container);
            }
            
            chunks[c].Item1.Add(c.transform.InverseTransformPoint(vec[i]));
            chunks[c].Item2.Add(voxel[i]);
        }

        foreach (var c in chunks)
        {
            c.Key.SetVoxels(c.Value.Item1, c.Value.Item2);
        }
    }
    /// <returns>The y value of the highest solid terrain at the global x and z.
    /// Min world height if there's no ground. </returns>
    public float HeightAtLocation(float globalX, float globalZ)
    {
        float globalChunkWidth = parameters.ChunkSize / parameters.Resolution;
        int xIndex = (int)Math.Abs(Mathf.FloorToInt(globalX % globalChunkWidth) * parameters.Resolution);
        int zIndex = (int)Math.Abs(Mathf.FloorToInt(globalZ % globalChunkWidth) * parameters.Resolution);

        Vector3Int chunkCoords = new Vector3Int(
            Mathf.FloorToInt(globalX / globalChunkWidth), 0,
            Mathf.FloorToInt(globalZ / globalChunkWidth));

        //Start from world height and loop downards to min world height
        for (int chunkY = parameters.WorldHeightInChunks; chunkY >= 0; chunkY--){
            chunkCoords.y = chunkY;
            Chunk c = GetChunk(chunkCoords);
            if (c == null) continue;

            for (int y = parameters.ChunkHeight - 1; y >= 0; y--)
            {
                Voxel voxel = c.GetVoxel(new Vector3Int(xIndex, y, zIndex));
                if (voxel.type != VoxelType.AIR)
                {
                    return c.transform.position.y + (y / parameters.Resolution);
                }
            }
        }
        return 0;
    }
    public void UnloadAll()
    {
        loadQueue.Clear();
        unloadQueue.Clear();
        chunksInProg.Clear();
        foreach (Vector3Int coord in chunks.Keys)
        {
            unloadQueue.Enqueue(coord);
        }

        UpdateNeighborQueues();
    }
    public bool IsLoadingInProgress()
    {
        return chunksInProg.Count > 0 || loadQueue.Count > 0;
    }

    public bool Contains(Agent player)
    {
        return players.Contains(player);
    }

    public void AddPlayer(Agent player)
    {
        players.Add(player);
    }
}
