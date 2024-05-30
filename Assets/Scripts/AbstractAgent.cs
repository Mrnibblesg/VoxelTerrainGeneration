using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Defines methods that must be implemented which allow the player to interface with the world
public abstract class AbstractAgent : MonoBehaviour
{

    private Vector3Int currentChunkCoord;

    private int renderDist = 7;
    private int unloadDist = 8;

    //TODO more sophisticated get and set for potential world switching.

    //Until we can ensure that the player's world is set before the player
    //becomes active, we must always use the null-conditional operator ?. with it.
    private World currentWorld;
    public World CurrentWorld
    {
        get
        {
            return currentWorld;
        }
        set
        {
            this.currentWorld = value;
            Vector3 startPosition = new(
                0.5f,
                value.worldHeight * value.chunkHeight / value.resolution + 1.5f,
                0.5f
            );
            transform.position = startPosition;

            UpdateChunkCoord();
        }
    }


    /// <summary>
    /// Attempt to break a block in the current world, at world-space position.
    /// </summary>
    public void TryBreak(Vector3 pos)
    {
        currentWorld?.SetVoxel(pos, VoxelType.AIR);
    }
    public void TryBreakList(List<Vector3> pos)
    {
        List<VoxelType> types = new List<VoxelType>();
        for (int i = 0; i < pos.Count; i++)
        {
            types.Add(VoxelType.AIR);
        }
        currentWorld?.SetVoxels(pos, types);
    }
    /// <summary>
    /// Attempt to place a block in the current world, at world-space position.
    /// </summary>
    public void TryPlace(Vector3 pos, VoxelType type)
    {
        currentWorld?.SetVoxel(pos, type);
    }
    public void TryPlaceList(List<Vector3> pos, List<VoxelType> types)
    {
        currentWorld?.SetVoxels(pos, types);
    }

    /// <summary>
    /// Calculates the player's current chunk coordinate. If it changes,
    /// then we notify the player's current world that it happened, so that
    /// it can update the player's view of the world (add chunks, remove chunks)
    /// </summary>
    protected void UpdateChunkCoord()
    {
        // Ensure JobManager is initialized before taking any actions
        if (JobManager.Manager is null)
        {
            return;
        }

        Vector3Int chunkCoord = new(
            Mathf.FloorToInt(transform.position.x / (currentWorld.chunkSize / currentWorld.resolution)),
            Mathf.FloorToInt(transform.position.y / (currentWorld.chunkHeight / currentWorld.resolution)),
            Mathf.FloorToInt(transform.position.z / (currentWorld.chunkSize / currentWorld.resolution))
        );

        if (currentChunkCoord != chunkCoord)
        {
            currentChunkCoord = chunkCoord;
            currentWorld?.UpdatePlayerChunkPos(currentChunkCoord, renderDist, unloadDist);
        }
    }
}
