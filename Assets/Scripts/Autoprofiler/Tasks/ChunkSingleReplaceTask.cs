using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Destroy the supplied chunk in a random order.
/// Seems to have a really weird effect on large chunks.
/// </summary>
public class ChunkSingleReplaceTask : WorldTask
{
    Vector3Int chunkCoords;
    VoxelType type;
    Chunk c;
    Vector3[] DestroyOrder;
    int progress = 0;

    bool surfaceSet;
    bool surface;

    public ChunkSingleReplaceTask(Vector3Int chunkCoords, VoxelType type, bool surface=false)
    {
        this.chunkCoords = chunkCoords;
        surfaceSet = false;
        this.surface = surface;
        this.type = type;
    }
    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        if (c == null)
        {
            if (surface && !surfaceSet)
            {
                surfaceSet = true;
                //Get the chunk y-coordinate of the highest block at the chunk x & z
                float globalX = chunkCoords.x * agent.CurrentWorld.parameters.ChunkSize;
                float globalZ = chunkCoords.z * agent.CurrentWorld.parameters.ChunkSize;
                float highestY = agent.CurrentWorld.HeightAtLocation(globalX, globalZ);
                c = agent.CurrentWorld.ChunkFromGlobal(new Vector3(globalX, highestY, globalZ));
            }
            else
            {
                c = agent.CurrentWorld.GetChunk(chunkCoords);
            }
            
            if (c == null)
            {
                IsComplete = true;
                return;
            }
            SetUpDestroyOrder();
        }
        if (progress < DestroyOrder.Length)
        {
            agent.TryPlace(DestroyOrder[progress], type);
            progress++;
        }
        else
        {
            IsComplete = true;
        }

    }
    private void SetUpDestroyOrder()
    {
        int size = c.world.parameters.ChunkSize;
        int height = c.world.parameters.ChunkHeight;
        int voxels = size * height * size;
        DestroyOrder = new Vector3[voxels];
        for (int i = 0; i < voxels; i++)
        {
            DestroyOrder[i] = c.transform.position + (new Vector3(
                i / (size * height),
                (i / size) % height,
                i % size) / c.world.parameters.Resolution);
        }

        //Shuffle array using Fisher-Yates
        int n = DestroyOrder.Length;
        System.Random rnd = new System.Random();
        while (n > 1)
        {
            int r = rnd.Next(n--);
            Vector3 temp = DestroyOrder[n];
            DestroyOrder[n] = DestroyOrder[r];
            DestroyOrder[r] = temp;
        }
    }
}
