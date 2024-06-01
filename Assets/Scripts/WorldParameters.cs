using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A simple struct to hold parameters of worlds.
public struct WorldParameters
{
    public int Resolution { get; set; }
    public int WorldHeightInChunks { get; set; }
    public int ChunkSize { get; set; }
    public int ChunkHeight { get; set; }
    public int WaterHeight { get; set; }
    public int Seed { get; set; }
    public string Name { get; set; }


}
