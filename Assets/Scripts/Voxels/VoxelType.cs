using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum VoxelType 
{
    AIR,
    GRASS,
    COLD_GRASS,
    HOT_GRASS,
    DIRT,
    SAND,
    STONE,
    SANDSTONE,
    SNOW,
    ICE,
    WATER_SOURCE
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
            VoxelType.COLD_GRASS => new VoxelAttributes(new Color(0, 0.449f, 0.2301f)),
            VoxelType.HOT_GRASS => new VoxelAttributes(new Color(0, 0.5f, 0)),
            VoxelType.DIRT => new VoxelAttributes(new Color(0.46f, 0.333f, 0.169f)),
            VoxelType.SAND => new VoxelAttributes(new Color(0.46f, 0.333f, 0.169f)),
            VoxelType.STONE => new VoxelAttributes(new Color(0.3f, 0.3f, 0.3f)),
            VoxelType.SANDSTONE => new VoxelAttributes(new Color(0.3f, 0.3f, 0.3f)),
            VoxelType.WATER_SOURCE => new VoxelAttributes(new Color(0, 0, 0.5f, 0.5f)),

            _ => throw new System.NotImplementedException()
        };
    }
}