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

    public override void Update()
    {
        base.Update();
        
    }
}
