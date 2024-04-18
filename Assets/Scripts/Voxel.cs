using UnityEngine;

public struct Voxel
{
    public Vector3 pos;
    public Color col;
    public bool active;
    public Voxel(Vector3 pos, Color col, bool active = true)
    {
        this.pos = pos;
        this.col = col;
        this.active = active;
    }
}
