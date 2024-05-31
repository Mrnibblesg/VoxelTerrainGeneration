using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A WorldlyObject is some object that exists in a particular world.
/// An object that inherits this and does nothing else won't do anything.
/// </summary>
public abstract class WorldlyObject : MonoBehaviour
{
    protected Vector3Int currentChunkCoord;

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
}
