using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitTask : ITask
{
    //Time in seconds
    float duration;
    bool completed = false;
    public WaitTask(float duration)
    {
        this.duration = duration;
    }
    //Expected behavior: This is called, control doesn't return to the flow of
    //tasks until the wait is done.
    public void Perform(ITaskable<AbstractAgent> agent)
    {
        PerformWait(agent);
    }
    public IEnumerator PerformWait(ITaskable<AbstractAgent> agent)
    {
        Debug.Log("Start waiting");
        yield return new WaitForSeconds(duration);
        Debug.Log("Finished waiting");
    }
    public void Interrupt()
    {

    }
    public bool IsComplete()
    {
        return false;
    }
}
