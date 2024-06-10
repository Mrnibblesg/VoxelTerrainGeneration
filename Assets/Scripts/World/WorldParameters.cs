using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A simple struct to hold parameters of worlds.
public struct WorldParameters
{
    public float Resolution { get; set; }
    public int WorldHeightInChunks { get; set; }
    public int ChunkSize { get; set; }
    public int ChunkHeight { get; set; }
    public int WaterHeight { get; set; }
    public int Seed { get; set; }
    public string Name { get; set; }

    public void Deconstruct(out float resolution, out int worldHeightInChunks,
        out int chunkSize, out int chunkHeight,
        out int waterHeight, out int seed, out string name)
    {
        (
            resolution,
            worldHeightInChunks,
            chunkSize,
            chunkHeight,
            waterHeight,
            seed,
            name
        )
        
        = (
            this.Resolution,
            this.WorldHeightInChunks,
            this.ChunkSize,
            this.ChunkHeight,
            this.WaterHeight,
            this.Seed,
            this.Name
        );
    }
}
