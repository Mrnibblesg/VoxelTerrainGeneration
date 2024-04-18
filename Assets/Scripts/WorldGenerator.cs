using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int chunkSize = 32;
    public Transform Origin;
    Object voxel;
    // Start is called before the first frame update
    void Start()
    {
        voxel = (Object)Resources.Load("Prefabs/Voxel");
        
/*        generateChunk(0, 0);
        generateChunk(2, 0);
        generateChunk(-1, 0);*/

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                generateChunk(i, j);
            }
        }
    }
    //Very inefficient. Very laggy.
    void generateChunk(int chunkX, int chunkZ)
    {
        //Check if the chunk exists already?
        GameObject chunkOrigin = new GameObject(chunkX + " " + chunkZ);
        chunkOrigin.transform.SetParent(Origin);
        int chunkCornerX = chunkX * chunkSize;
        int chunkCornerZ = chunkZ * chunkSize;
        chunkOrigin.transform.SetPositionAndRotation(
            new Vector3(chunkCornerX, 0, chunkCornerZ),
            Quaternion.identity);
        

        for (int x = 0; x < 32; x++)
        {
            for (int z = 0; z < 32; z++)
            {
                Instantiate(
                    voxel,
                    new Vector3(chunkCornerX + x, 0, chunkCornerZ + z),
                    Quaternion.identity,
                    chunkOrigin.transform);
            }
        }
    }
}
