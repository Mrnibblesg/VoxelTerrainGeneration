using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakTask : ITask
{
    public void Perform(ITaskable agent)
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
