using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A task is a representation of some world action. For example,
/// a task can be a movement to a specific spot, waiting,
/// breaking/placing of a single or mass amounts of voxels,
/// requesting to join a different world, and more.
/// </summary>
public interface ITask
{
    /// <summary>
    /// Per the task as it's defined in the concrete task's implementation.
    /// </summary>
    /// <param name="agent"></param>
    public void Perform(ITaskable<AbstractAgent> agent);
    /// <summary>
    /// Interrupt the current task (if there's one being performed.)
    /// </summary>
    public void Interrupt();

    /// <returns>Whether the task is completed</returns>
    public bool IsComplete();
}
