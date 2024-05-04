using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum VoxelType 
{
    AIR,
    GRASS,
    DIRT,
    STONE,
    GLASS
}

public static class VoxelExtensions
{
    /// <summary>
    /// Get the corresponding voxel attribute for the base voxel type.
    /// </summary>
    /// <remarks>
    /// This method uses an enhanced switch to resolve the attributes.
    /// To add more attributes, simply add a line in the existing format. :)
    /// </remarks>
    /// <param name="voxel"></param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public static VoxelAttributes getVoxelAttributes(this VoxelType voxel)
    {
        return voxel switch
        {
            VoxelType.AIR => new VoxelAttributes(new Color(0, 0, 0, 1)),
            VoxelType.GRASS => new VoxelAttributes(new Color(0, 0.5f, 0)),
            VoxelType.DIRT => new VoxelAttributes(new Color(0.46f, 0.333f, 0.169f)),
            VoxelType.STONE => new VoxelAttributes(new Color(0.3f, 0.3f, 0.3f)),
            VoxelType.GLASS => new VoxelAttributes(new Color(0.99f, 0.99f, 0.99f, 0.1f)),

            _ => throw new System.NotImplementedException()
        };
    }
}