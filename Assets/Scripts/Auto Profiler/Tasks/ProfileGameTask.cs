using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileGameTask : ITask
{
    private Queue<ITask> tasks;
    public ProfileGameTask()
    {
        
    }
    public void Perform(ITaskable<AbstractAgent> agent)
    {
        //agent.
    }
    public void Interrupt()
    {

    }
    public bool IsComplete()
    {
        return false;
    }
}
