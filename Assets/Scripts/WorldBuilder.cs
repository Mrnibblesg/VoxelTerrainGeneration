using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder
{
    //Defined as voxels per world unit
    public int Resolution { get; set; }
    private static int DEFAULT_RESOLUTION = 1;

    public int WorldHeight { get; set; }
    private static int DEFAULT_WORLD_HEIGHT = 4;

    //Dimensions of chunk in the amount of voxels
    public int ChunkSize { get; set; }
    private static int DEFAULT_CHUNK_SIZE = 16;

    public int ChunkHeight { get; set; }
    private static int DEFAULT_CHUNK_HEIGHT = 16;

    public int WaterHeight { get; set; }
    private static int DEFAULT_WATER_HEIGHT = 4;

    public string WorldName { get; set; }
    private static string DEFAULT_WORLD_NAME = "Default";

    /// <summary>
    /// Default constructor initializes the world builder with default values.
    /// </summary>
    public WorldBuilder()
    {
        this.Resolution = DEFAULT_RESOLUTION;
        this.WorldHeight = DEFAULT_WORLD_HEIGHT;
        this.ChunkSize = DEFAULT_CHUNK_SIZE;
        this.ChunkHeight = DEFAULT_CHUNK_HEIGHT;
        this.WaterHeight = DEFAULT_WATER_HEIGHT;
        this.WorldName = DEFAULT_WORLD_NAME;
    }

    public WorldBuilder SetDimensions(int resolution, int height, int chunkSize, int chunkHeight, int waterHeight)
    {
        this.Resolution = (int) Mathf.Pow(2, resolution - 1); //Resolution in powers of 2 to avoid odd numbers which make rendering annoying due to rounding errors
        this.WorldHeight = height;
        this.ChunkSize = chunkSize;
        this.ChunkHeight = chunkSize; //chunkHt;
        this.WaterHeight = waterHeight;

        return this;
    }

    public WorldBuilder SetWorldName(string name)
    {
        this.WorldName = name;
        return this;
    }

    /// <summary>
    /// Creates a world. Side-effect: adds the world to the world accessor.
    /// </summary>
    public World Build()
    {
        World world = new World(WorldHeight, ChunkSize, ChunkHeight, WaterHeight, Resolution, WorldName);

        // Add the world to the world accessor
        WorldAccessor.AddWorld(WorldName, world);

        return world;
    }

    public static World CreatePresetWorld()
    {
        return new WorldBuilder().Build();
    }

    public static World InitializeMenuWorld()
    {
        World defaultWorld = new WorldBuilder().SetWorldName("Menu").Build();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.GetComponent<MenuCameraController>().CurrentWorld = defaultWorld;
            player.SetActive(true); // Disable player control in the main menu
        }

        return defaultWorld;
    }
}
