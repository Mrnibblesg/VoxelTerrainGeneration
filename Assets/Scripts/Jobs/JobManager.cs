using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

//The purpose of this manager is to provide classes that may not inherit Monobehaviour
//to use jobs and make use of a callback upon its completion.

//Schedule a job from your own class, then pass the handle here if you need to
//make use of a callback.
public class JobManager : MonoBehaviour
{
    public static JobManager Manager { get; private set; }
    private Dictionary<JobHandle, Action> running;

    private void Awake()
    {
        if (Manager != null)
        {
            throw new Exception("Only one instance of the JobManager is allowed!");
        }
        Manager = this;

        running = new();
    }

    void Update()
    {
        List<JobHandle> complete = new();
        foreach (KeyValuePair<JobHandle, Action> p in running)
        {
            if (p.Key.IsCompleted)
            {
                p.Key.Complete();
                p.Value.Invoke();
                complete.Add(p.Key);
            } 
        }

        foreach (JobHandle j in complete)
        {
            running.Remove(j);
        }

        complete.Clear();
    }
    
    public void addJob(JobHandle handle, Action callback)
    {
        running.Add(handle, callback);
    }
}
