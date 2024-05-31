using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Something that is an AuthoritativeAgent is an agent treated with even more
/// importance than an AbstractAgent. AuthoritativeAgents communicate with the world and
/// have the world loaded around themselves.
/// I.e., AbstractAgents are one step lower on the
/// hierarchy and are only loaded & simulated when there's an AuthoritativeAgent
/// nearby with them in their render distance.
/// 
/// An object that inherits this and does nothing else will render surrounding 
/// chunks in its current set world and do nothing else. If it doesn't have a
/// world set, then it will do nothing, falling into the void.
/// </summary>
public abstract class AuthoritativeAgent : AbstractAgent
{
    protected int renderDist = 7;
    protected int unloadDist = 8;

    public override World CurrentWorld
    {
        get
        {
            return currentWorld;
        }
        set
        {
            currentWorld = value;
            UpdateChunkCoord();
        }
    }

    public virtual void Update()
    {
        if (CurrentWorld is not null)
        {
            UpdateChunkCoord();
        }
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
            Mathf.FloorToInt(transform.position.x / (CurrentWorld.chunkSize / CurrentWorld.resolution)),
            Mathf.FloorToInt(transform.position.y / (CurrentWorld.chunkHeight / CurrentWorld.resolution)),
            Mathf.FloorToInt(transform.position.z / (CurrentWorld.chunkSize / CurrentWorld.resolution))
        );

        if (currentChunkCoord != chunkCoord)
        {
            currentChunkCoord = chunkCoord;
            CurrentWorld?.UpdateAgentChunkPos(currentChunkCoord, renderDist, unloadDist);
        }
    }
}
