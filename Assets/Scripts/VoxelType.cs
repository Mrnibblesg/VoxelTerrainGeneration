using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelType : MonoBehaviour
{
    public static readonly Dictionary<Type, Color> TypeColor;
    public enum Type{ 
        AIR, 
        GRASS,
        DIRT,
        STONE,
        GLASS
    }
    static VoxelType()
    {
        TypeColor = new Dictionary<Type, Color>
        {
            { Type.AIR, new Color(0, 0, 0, 1) },
            { Type.GRASS, new Color(0, 0.5f, 0) },
            { Type.DIRT, new Color(0.46f, 0.333f, 0.169f) },
            { Type.STONE, new Color(0.3f, 0.3f, 0.3f) },
            { Type.GLASS, new Color(0.99f, 0.99f, 0.99f, 0.1f) },
        };
    }
}
