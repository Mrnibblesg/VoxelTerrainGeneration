using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder
{
    //Defined as voxels per world unit
    private static WorldParameters DEFAULT_PARAMS = new WorldParameters
    {
        Resolution = 1,
        WorldHeightInChunks=4,
        ChunkSize=16,
        ChunkHeight=16,
        WaterHeight=4,
        Seed=1,
        Name="Default",

    };
    private WorldParameters worldParams = DEFAULT_PARAMS;

    /// <summary>
    /// Default constructor initializes the world builder with default values.
    /// </summary>
    public WorldBuilder()
    {
        worldParams = DEFAULT_PARAMS;
    }

    public WorldBuilder SetParameters(WorldParameters wp)
    {
        worldParams = wp;
        return this;
    }

    public WorldBuilder SetSeed(int seed)
    {
        worldParams.Seed = seed;
        return this;
    }

    public WorldBuilder SetWorldName(string name)
    {
        worldParams.Name = name;
        return this;
    }

    /// <summary>
    /// Creates a world. Side-effect: adds the world to the world accessor.
    /// </summary>
    public World Build()
    {
        World world = new World(worldParams);
        Debug.Log("Creating world " + worldParams.Name);
        // Add the world to the world accessor
        WorldAccessor.AddWorld(worldParams.Name, world);

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
