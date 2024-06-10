using Mirror;
using UnityEngine;

public class NetworkedAgent : NetworkBehaviour
{
    public Agent Agent { get; set; }

    /// <summary>
    /// Attempt to break a block in the current world, at world-space position.
    /// </summary>
    [Command]
    public void TryBreak(Vector3 pos)
    {
        this.Break(pos, Agent.CurrentWorld.parameters.Name);
    }    

    [Command]
    public virtual void TryTwoPointBreak(Vector3 p1, Vector3 p2)
    {
        this.TwoPointBreak(p1, p2 , Agent.CurrentWorld.parameters.Name);
    }

    [Command]
    public void TryTwoPointReplace(Vector3 p1, Vector3 p2, VoxelType type)
    {
        this.TwoPointReplace(p1, p2, type, Agent.CurrentWorld.parameters.Name);
    }

    /// <summary>
    /// Attempt to place a block in the current world, at world-space position.
    /// </summary>
    [Command]
    public virtual void TryPlace(Vector3 pos, VoxelType type)
    {
        this.Place(pos, type, Agent.CurrentWorld.parameters.Name);
    }

    [ClientRpc]
    public void Break(Vector3 pos, string worldName)
    {
        WorldAccessor.GetWorld(worldName).SetVoxel(pos, VoxelType.AIR);
    }

    [ClientRpc]
    public void TwoPointBreak(Vector3 p1, Vector3 p2, string worldName)
    {
        this.TwoPointReplace(p1, p2, VoxelType.AIR, worldName);
    }

    [ClientRpc]
    public void Place(Vector3 pos, VoxelType type, string worldName)
    {
        WorldAccessor.GetWorld(worldName).SetVoxel(pos, type);
    }

    [ClientRpc]
    public void TwoPointReplace(Vector3 p1, Vector3 p2, VoxelType type, string worldName)
    {
        WorldAccessor.GetWorld(worldName).SetVoxels(p1, p2, type);
    }
}
