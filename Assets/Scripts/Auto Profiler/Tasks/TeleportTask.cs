using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTask : WorldTask
{
    public override void Perform(Agent agent)
    {

        this.IsComplete = true;
    }
    public override void Interrupt()
    {

    }
}
