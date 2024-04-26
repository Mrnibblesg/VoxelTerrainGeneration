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

    public static Voxel Clone(Voxel other)
    {
        return new Voxel(other.pos, other.col, other.active);
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

    public Voxel SetPosition(Vector3 pos)
    {
        this.pos = pos;

        return this;
    }
}
