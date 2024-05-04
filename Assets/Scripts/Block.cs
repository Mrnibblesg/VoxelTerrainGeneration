using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum Block 
{
    AIR,
    GRASS,
    DIRT,
    STONE,
    GLASS
}

public static class BlockExtensions
{
    /// <summary>
    /// Get the corresponding block attribute for the base block type.
    /// </summary>
    /// <remarks>
    /// This method uses an enhanced switch to resolve the attributes.
    /// To add more attributes, simply add a line in the existing format. :)
    /// </remarks>
    /// <param name="block"></param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public static BlockAttributes getBlockAttributes(this Block block)
    {
        return block switch
        {
            Block.AIR => new BlockAttributes(new Color(0, 0, 0, 1)),
            Block.GRASS => new BlockAttributes(new Color(0, 0.5f, 0)),
            Block.DIRT => new BlockAttributes(new Color(0.46f, 0.333f, 0.169f)),
            Block.STONE => new BlockAttributes(new Color(0.3f, 0.3f, 0.3f)),
            Block.GLASS => new BlockAttributes(new Color(0.99f, 0.99f, 0.99f, 0.1f)),

            _ => throw new System.NotImplementedException()
        };
    }
}