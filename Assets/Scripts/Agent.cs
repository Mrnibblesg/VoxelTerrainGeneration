using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anything that is an AbstractAgent can interact with the world in some way.
/// An object that inherits this and does nothing else won't do anything.
/// </summary>
public abstract class Agent : WorldObject
{
    public virtual bool IsTaskable {
        get {
            return this is ITaskable;
        }
    }

    public ITaskable Taskable
    {
        get
        {
            if (this is ITaskable taskable)
            {
                return taskable;
            }

            return null;
        }
    }

    /// <summary>
    /// Attempt to break a block in the current world, at world-space position.
    /// </summary>
    public virtual void TryBreak(Vector3 pos)
    {
        CurrentWorld?.SetVoxel(pos, VoxelType.AIR);
    }
    public virtual void TryTwoPointBreak(Vector3 p1, Vector3 p2)
    {
        TryTwoPointReplace(p1, p2, VoxelType.AIR);
    }
    /// <summary>
    /// Attempt to place a block in the current world, at world-space position.
    /// </summary>
    public virtual void TryPlace(Vector3 pos, VoxelType type)
    {
        CurrentWorld?.SetVoxel(pos, type);
    }
    public virtual void TryTwoPointReplace(Vector3 p1, Vector3 p2, VoxelType type)
    {
        CurrentWorld?.SetVoxels(p1, p2, type);
    }

    /// <summary>
    /// A way to move this agent. There's position offset & there's force to be
    /// added to the object. By default, force isn't used, because AbstractAgent
    /// type objects don't necessarily have a RigidBody.
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="force"></param>
    public virtual void Move(Vector3 offset, Vector3 force = new())
    {
        transform.Translate(offset);
    }

}
