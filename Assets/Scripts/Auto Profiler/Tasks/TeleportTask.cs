using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTask : ITask
{
    public void Perform(ITaskable<AbstractAgent> agent)
    {

    }
    public void Interrupt()
    {

    }
    public bool IsComplete()
    {
        return false;
    }
}
