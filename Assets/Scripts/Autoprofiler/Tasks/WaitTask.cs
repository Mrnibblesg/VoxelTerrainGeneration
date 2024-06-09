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

    public override void Perform(Agent agent)
    {
        base.Perform(agent);
        if (!waiting)
        {
            agent.StartCoroutine(PerformWait(agent));
            waiting = true;
        }
    }
    public IEnumerator PerformWait(Agent agent)
    {
        yield return new WaitForSeconds(duration);
        IsComplete = true;
    }
    public override void Interrupt()
    {

    }
}
