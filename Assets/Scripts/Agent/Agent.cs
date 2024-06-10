using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Anything that is an AbstractAgent can interact with the world in some way.
/// An object that inherits this and does nothing else won't do anything.
/// </summary>
public abstract class Agent : WorldObject
{
    private NetworkedAgent networkedAgent;
    protected NetworkedAgent NetworkedAgent {
        get
        {
            return networkedAgent;
        }
        set
        {
            this.networkedAgent = value;
            value.Agent = this;
        }
    }
    
    public virtual bool IsTaskable {
        get {
            return this is ITaskable;
        }
    }

    public ITaskable Taskable
    {
        get
        {
            if (this is ITaskable taskable)
            {
                return taskable;
            }

            return null;
        }
    }

    /// <summary>
    /// Attempt to break a block in the current world, at world-space position.
    /// </summary>
    public void TryBreak(Vector3 pos)
    {
        VerifyNetworked();

        NetworkedAgent.TryBreak(pos);
    }

    public void TryTwoPointBreak(Vector3 p1, Vector3 p2)
    {
        VerifyNetworked();

        NetworkedAgent.TryTwoPointBreak(p1, p2);
    }

    /// <summary>
    /// Attempt to place a block in the current world, at world-space position.
    /// </summary>
    public void TryPlace(Vector3 pos, VoxelType type)
    {
        VerifyNetworked();

        NetworkedAgent.TryPlace(pos, type);
    }

    public void TryTwoPointReplace(Vector3 p1, Vector3 p2, VoxelType type)
    {
        VerifyNetworked();

        NetworkedAgent.TryTwoPointReplace(p1, p2, type);
    }

    private bool VerifyNetworked()
    {
        if (NetworkedAgent.IsUnityNull())
        {
            Debug.LogError("WARNING: Agent attempted a networked action without a NetworkedAgent attached!");
        }

        return !NetworkedAgent.IsUnityNull();
    }
    /// <summary>
    /// A way to move this agent. There's position offset & there's force to be
    /// added to the object. By default, force isn't used, because AbstractAgent
    /// type objects don't necessarily have a RigidBody.
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="force"></param>
    public virtual void Move(Vector3 offset, Vector3 force = new())
    {
        transform.Translate(offset);
    }

}
