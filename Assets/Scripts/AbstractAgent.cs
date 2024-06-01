using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anything that is an AbstractAgent can interact with the world in some way.
/// An object that inherits this and does nothing else won't do anything.
/// </summary>
public abstract class AbstractAgent : WorldlyObject
{
    /// <summary>
    /// Attempt to break a block in the current world, at world-space position.
    /// </summary>
    public virtual void TryBreak(Vector3 pos)
    {
        CurrentWorld?.SetVoxel(pos, VoxelType.AIR);
    }
    public virtual void TryBreakList(List<Vector3> pos)
    {
        List<VoxelType> types = new List<VoxelType>();
        for (int i = 0; i < pos.Count; i++)
        {
            types.Add(VoxelType.AIR);
        }
        CurrentWorld?.SetVoxels(pos, types);
    }
    /// <summary>
    /// Attempt to place a block in the current world, at world-space position.
    /// </summary>
    public virtual void TryPlace(Vector3 pos, VoxelType type)
    {
        CurrentWorld?.SetVoxel(pos, type);
    }
    public virtual void TryPlaceList(List<Vector3> pos, List<VoxelType> types)
    {
        CurrentWorld?.SetVoxels(pos, types);
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
