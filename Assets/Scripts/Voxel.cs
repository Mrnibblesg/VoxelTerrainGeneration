using System;
using UnityEngine;


public struct Voxel
{
    //CORNERS
    // X - Left/Right - 0/1
    // Y - Bottom/Top - 0/1
    // Z - Front/Back - 0/1
    public static readonly Vector3Int LBF = new(0, 0, 0);
    public static readonly Vector3Int RBF = new(1, 0, 0);

    public static readonly Vector3Int LTF = new(0, 1, 0);
    public static readonly Vector3Int RTF = new(1, 1, 0);

    public static readonly Vector3Int LBB = new(0, 0, 1);
    public static readonly Vector3Int RBB = new(1, 0, 1);

    public static readonly Vector3Int LTB = new(0, 1, 1);
    public static readonly Vector3Int RTB = new(1, 1, 1);

    public Block type;
    public bool isAir;
    public bool hasTransparency;
    public bool exposed;
    public Voxel(Block type, bool isAir = false, bool hasTransparency = false)
    {
        this.type = type;
        this.isAir = isAir;
        this.hasTransparency = isAir || hasTransparency;
        
        this.exposed = true;
    }

    public static Voxel Clone(Voxel other)
    {
        return new Voxel(other.type, other.isAir, other.hasTransparency);
    }

    public Voxel SetAir(bool isAir)
    {
        this.isAir = isAir;
        return this;
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
    public Voxel SetType(Block type)
    {
        this.type = type;

        return this;
    }
}
