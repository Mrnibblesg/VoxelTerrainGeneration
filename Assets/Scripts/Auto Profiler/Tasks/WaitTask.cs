using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitTask : WorldTask
{
    //Time in seconds
    float duration;
    bool waiting = false;
    public WaitTask(float duration)
    {
        IsComplete = false;
        this.duration = duration;
    }
    //Expected behavior: This is called, control doesn't return to the flow of
    //tasks until the wait is done.
    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        if (!waiting)
        {
            Debug.Log("Performing wait!");
            agent.StartCoroutine(PerformWait(agent));
            waiting = true;
        }
    }
    public IEnumerator PerformWait(Agent agent)
    {
        Debug.Log("Start waiting");
        yield return new WaitForSeconds(duration);
        Debug.Log("Finished waiting");
        IsComplete = true;
    }
    public override void Interrupt()
    {

    }
}
