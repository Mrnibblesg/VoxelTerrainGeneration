using System;
using UnityEngine;


public struct Voxel
{
    public VoxelType type;
    public bool hasTransparency;
    public bool exposed;
    public Voxel(VoxelType type, bool hasTransparency = false)
    {
        this.type = type;
        this.hasTransparency = hasTransparency;
        this.exposed = true;
    }

    public static Voxel Clone(Voxel other)
    {
        return new Voxel(other.type, other.hasTransparency);
    }

    public Voxel SetTransparency(bool hasTransparency)
    {
        this.hasTransparency = hasTransparency;
        return this;
    }
    public Voxel SetExposed(bool exposed)
    {
        this.exposed = exposed;
        return this;
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
