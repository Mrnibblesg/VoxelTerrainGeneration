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
        this.RpcBreak(pos, Agent.CurrentWorld.parameters.Name);
    }

    [Command]
    public void TryTwoPointReplace(Vector3 p1, Vector3 p2, VoxelType type)
    {
        this.RpcTwoPointReplace(p1, p2, type, Agent.CurrentWorld.parameters.Name);
    }

    /// <summary>
    /// Attempt to place a block in the current world, at world-space position.
    /// </summary>
    [Command]
    public virtual void TryPlace(Vector3 pos, VoxelType type)
    {
        this.RpcPlace(pos, type, Agent.CurrentWorld.parameters.Name);
    }

    [ClientRpc]
    public void RpcBreak(Vector3 pos, string worldName)
    {
        Break(pos, worldName);
    }

    [ClientRpc]
    public void RpcPlace(Vector3 pos, VoxelType type, string worldName)
    {
        Place(pos, type, worldName);
    }

    [ClientRpc]
    public void RpcTwoPointReplace(Vector3 p1, Vector3 p2, VoxelType type, string worldName)
    {
        TwoPointReplace(p1, p2, type, worldName);
    }

    public static void Break(Vector3 pos, string worldName)
    {
        WorldAccessor.GetWorld(worldName).SetVoxel(pos, VoxelType.AIR);
    }

    public static void Place(Vector3 pos, VoxelType type, string worldName)
    {
        WorldAccessor.GetWorld(worldName).SetVoxel(pos, type);
    }

    public static void TwoPointReplace(Vector3 p1, Vector3 p2, VoxelType type, string worldName)
    {
        WorldAccessor.GetWorld(worldName).SetVoxels(p1, p2, type);
    }

}
