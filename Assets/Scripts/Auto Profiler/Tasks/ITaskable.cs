using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objects that implement ITaskable make it easy to perform tasks.
/// Tasks aren't linked to any monobehaviors in any way, however tasks take time
/// to complete (excluding wait) because they might require smooth movements.
/// Thus, Mono objects implement this class so they can manage a task
/// data structure as well as manage tasks
/// 
/// We only allow things that are an AbstractAgent to be tasked with things.
/// </summary>
public interface ITaskable
{
    //Task the object with a new task.
    //The object will complete it at its own discretion.
    public void AddTask(WorldTask t);
}
