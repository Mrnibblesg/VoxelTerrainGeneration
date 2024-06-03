using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A task is a representation of some world action. For example,
/// a task can be a movement to a specific spot, waiting,
/// breaking/placing of a single or mass amounts of voxels,
/// requesting to join a different world, and more.
/// </summary>
/// <remarks>
/// When implementing "perform," please first check the agent is Taskable or call the base class.
/// </remarks>
public abstract class WorldTask
{
    public virtual bool IsComplete { get; protected set; }

    /// <summary>
    /// Per the task as it's defined in the concrete task's implementation.
    /// </summary>
    /// <param name="agent"></param>
    public virtual void Perform(Agent agent)
    {
        this.CheckTaskable(agent);
    }
    
    /// <summary>
    /// Interrupt the current task (if there's one being performed.)
    /// </summary>
    public virtual void Interrupt()
    {
        IsComplete = true;
    }

    protected void CheckTaskable(Agent agent)
    {
        if (agent is not ITaskable)
        {
            throw new AgentNotTaskableException($"{nameof(agent)} is not taskable!");
        }
    }
}

public class AgentNotTaskableException : Exception {
    public AgentNotTaskableException(string message) : base(message)
    {
    }
}