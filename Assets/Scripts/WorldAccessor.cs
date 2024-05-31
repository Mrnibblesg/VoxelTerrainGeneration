using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldAccessor : MonoBehaviour
{
    // Singleton
    private static WorldAccessor Accessor { get; set; }
    private static Dictionary<string, World> worldDictionary = new Dictionary<string, World>();

    private void Start()
    {
        if (Accessor is not null)
        {
            Destroy(this);
        }

        else
        {
            DontDestroyOnLoad(gameObject);
            Accessor = this;
        }
    }

    public static void AddWorld(string name, World world)
    {
        worldDictionary.Add(name, world);
    }

    public static World GetWorld(string name)
    { 
        return worldDictionary[name];
    }

    public static World GetFirst()
    {
        foreach (KeyValuePair<string, World> entry in worldDictionary)
        {
            return entry.Value;
        }

        return null;
    }

    public static void RemoveWorld(string name)
    {
        worldDictionary.Remove(name);
    }

    public static void ClearWorlds()
    {
        worldDictionary.Clear();
    }

    /// <summary>
    /// Put the player in the first non-menu world in the dictionary.
    /// If there are no worlds, create a new one.
    /// </summary>
    /// <returns></returns>
    public static World Join(AbstractAgent player) {
        World world = worldDictionary.Values.FirstOrDefault(world => !world.worldName.Equals("Menu")) ?? WorldBuilder.CreatePresetWorld();
        world.AddPlayer(player);

        return world;
    }

    // Iterate through the dictionary and return the first world that the player is in
    public static World Identify(AbstractAgent player)
    {
        foreach (KeyValuePair<string, World> entry in worldDictionary)
        {
            if (!entry.Value.worldName.Equals("Menu") && entry.Value.Contains(player))
            {
                return entry.Value;
            }
        }

        return null;
    }
}
