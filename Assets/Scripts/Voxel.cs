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

    public Color col;
    public bool active;
    public Voxel(Color col, bool active = true)
    {
        this.col = col;
        this.active = active;
    }

    public static Voxel Clone(Voxel other)
    {
        return new Voxel(other.col, other.active);
    }

    public Voxel SetActive(bool active)
    {
        this.active = active;

        return this;
    }

    public Voxel SetColor(Color col)
    {
        this.col = col;

        return this;
    }
}
