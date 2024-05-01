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
            { Type.AIR, new Color(0, 0, 0, 1) }, //Air
            { Type.GRASS, new Color(0, 0.5f, 0) }, //Grass
            { Type.DIRT, new Color(0.46f, 0.333f, 0.169f) }, //Dirt
            { Type.STONE, new Color(0.3f, 0.3f, 0.3f) }, //Stone
            { Type.GLASS, new Color(0.99f, 0.99f, 0.99f, 0.9f) }, //Glass
        };
    }
}
