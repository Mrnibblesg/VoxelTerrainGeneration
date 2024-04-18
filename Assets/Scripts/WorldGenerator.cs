using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int worldSize;
    public int chunkSize = 32;
    private Dictionary<Vector3, Chunk> chunks;
    
    void Start()
    {
        chunks = new Dictionary<Vector3, Chunk>();
        GenerateWorld();
    }

    //Generates a world with dimensions worldSize x worldSize chunks
    void GenerateWorld()
    {
        for (int x = 0; x < worldSize; x++)
        {
            for (int z = 0; z < worldSize; z++)
            {
                GameObject chunkObj = new GameObject($"Chunk{x},{z}");
                Vector3 position = new Vector3(x * chunkSize, 0, z * chunkSize);
                chunkObj.transform.position = position;

                Chunk newChunk = chunkObj.AddComponent<Chunk>();
                newChunk.Initialize(chunkSize);
                chunks.Add(position, newChunk);
            }
        }
    }
}
