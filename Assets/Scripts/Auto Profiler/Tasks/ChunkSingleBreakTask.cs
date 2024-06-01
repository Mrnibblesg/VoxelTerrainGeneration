using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Destroy the supplied chunk in a random order.
/// </summary>
public class ChunkSingleBreakTask : WorldTask
{
    Vector3Int chunkCoords;
    Chunk c;
    Vector3[] DestroyOrder;
    int progress = 0;

    public ChunkSingleBreakTask(Vector3Int chunkCoords)
    {
        this.chunkCoords = chunkCoords;
    }
    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        if (c == null)
        {
            c = agent.CurrentWorld.GetChunk(chunkCoords);
            if (c == null)
            {
                IsComplete = true;
                return;
            }
            SetUpDestroyOrder();
        }
        if (progress < DestroyOrder.Length)
        {
            agent.TryBreak(DestroyOrder[progress]);
            progress++;
        }
        else
        {
            IsComplete = true;
        }

    }
    private void SetUpDestroyOrder()
    {
        int size = c.parent.chunkSize;
        int height = c.parent.chunkHeight;
        int voxels = size * height * size;
        DestroyOrder = new Vector3[voxels];
        for (int i = 0; i < voxels; i++)
        {
            DestroyOrder[i] = c.transform.position + (new Vector3(
                i / (size * height),
                (i / size) % height,
                i % size) / c.parent.resolution);
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
