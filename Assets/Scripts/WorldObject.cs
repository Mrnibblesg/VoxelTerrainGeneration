using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A WorldObject is some object that exists in a particular world.
/// An object that inherits this and does nothing else won't do anything special.
/// </summary>
public abstract class WorldObject : MonoBehaviour
{
    public Vector3Int chunkCoord;

    protected World currentWorld;
    public virtual World CurrentWorld {
        get
        {
            return currentWorld;
        }
        set
        {
            currentWorld = value;
        }
    }

    private void Update()
    {
    }
}
