using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitTask : WorldTask
{
    //Time in seconds
    float duration;
    public WaitTask(float duration)
    {
        this.duration = duration;
    }
    //Expected behavior: This is called, control doesn't return to the flow of
    //tasks until the wait is done.
    public override void Perform(Agent agent)
    {
        base.Perform(agent);

        PerformWait(agent);
    }
    public IEnumerator PerformWait(Agent agent)
    {
        Debug.Log("Start waiting");
        yield return new WaitForSeconds(duration);
        Debug.Log("Finished waiting");
    }
    public override void Interrupt()
    {

    }
}
