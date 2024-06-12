using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A simple struct to hold parameters of worlds.
public struct WorldParameters
{
    public float Resolution;
    public int WorldHeightInChunks;
    public int ChunkSize;
    public int ChunkHeight;
    public int WaterHeight;
    public int Seed;
    public string Name;

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
