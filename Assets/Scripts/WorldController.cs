using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Like world, but at an even higher level.
//TODO it doesn't make sense for the WorldController to be a monobehavior and exist in a particular world.
public class WorldController : MonoBehaviour
{
    public static WorldController Controller { get; private set; }

    //Dimensions of world in the amount of chunks
    public int worldSize { get; private set; }
    public int worldHeight { get; private set; }

    //Dimensions of chunk in the amount of voxels
    public int chunkSize { get; private set; }
    public int chunkHeight { get; private set; }

    private Dictionary<String, World> worlds;
    
    void Awake()
    { 
        if (Controller != null)
        {
            throw new Exception("Only one instance of the WorldGenerator is allowed");
        }
        Controller = this;
        worlds = new();
    }

    public void SetDimensions(int size, int height, int chunkSz, int chunkHt)
    {
        worldSize = size;
        worldHeight = height;
        chunkSize = chunkSz;
        chunkHeight = chunkSz * 4;//chunkHt; //TODO Chunk height not currently configurable in the main menu. Doing this for now.

        Chunk.size = chunkSize;
        Chunk.height = chunkHeight;
    }

    //Different worlds should reside in different scenes.
    //TODO: we need a better way of giving a player their initial world.
    /// <summary>
    /// Creates a world from the main menu.
    /// </summary>
    ///
    public void CreateWorld() //race condition between this and Player.Start TODO. It's hard to set the player's initial world.
    {
        World w = new World(worldHeight, chunkSize, chunkHeight);
        GameObject player = GameObject.FindGameObjectWithTag("Player") ?? throw new Exception("There must exist a player with the \"Player\" tag.");
        //ensure the player is loaded into the correct scene if we're having
        //different worlds exist in separate scenes.
        player.GetComponent<Player>().CurrentWorld = w;

        worlds.Add("World " + worlds.Count + 1, w);
    }
}
