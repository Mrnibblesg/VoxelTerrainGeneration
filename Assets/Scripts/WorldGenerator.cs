using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int worldSize;
    public int chunkSize = 32;
    private Dictionary<Vector3, Chunk> chunks;
    // Start is called before the first frame update
    void Start()
    {
        chunks = new Dictionary<Vector3, Chunk>();

    }

    //Generates a world with dimensions worldSize x worldSize chunks
    void GenerateWorld()
    {
        
    }
}
