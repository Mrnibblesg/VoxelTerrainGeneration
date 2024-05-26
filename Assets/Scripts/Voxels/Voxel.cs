using System;
using UnityEngine;


public struct Voxel
{
    public VoxelType type;
    public Voxel(VoxelType type)
    {
        this.type = type;
    }

    public static Voxel Clone(Voxel other)
    {
        return new Voxel(other.type);
    }
    public Voxel SetType(VoxelType type)
    {
        this.type = type;

        return this;
    }
    public static bool Equals(Voxel a, Voxel b)
    {
        return a.type == b.type;
    }
}
