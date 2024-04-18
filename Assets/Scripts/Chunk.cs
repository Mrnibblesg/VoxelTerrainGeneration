using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    Voxel[,,] voxels;
    public int height = 128;
    private int size;
    //Separate from start script so we can choose when to
    //generate after creation
    public void Initialize(int size)
    {
        this.size = size;
        voxels = new Voxel[size, height, size];
        InitializeVoxels();
    }

    void InitializeVoxels()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    voxels[x, y, z] = new Voxel(
                        transform.position + new Vector3(x, y, z),
                        Color.white
                    );
                }
            }
        }
    }
}
