using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

//Acts as a stand-in for a player to perform actions autonomously.
//Everything from movement, block breaking/placing, mass block breaks/places
//teleporting, world reloading is done automatically.
public class ProfilerAgent : AuthoritativeAgent
{
    public override World CurrentWorld
    {
        set
        {
            currentWorld = value;
            Vector3 startPosition = new(
                0.5f,
                value.worldHeight * value.chunkHeight / value.resolution,
                0.5f
            );
            transform.position = startPosition;

            UpdateChunkCoord();
        }
    }
/*    public void Start()
    {
        // Check in WorldAccessor for a world
        World world = WorldAccessor.Identify(this);

        if (world is null)
        {
            world = WorldAccessor.Join(this);
        }
        Debug.Log("One");
        CurrentWorld = world;
    }*/

}
