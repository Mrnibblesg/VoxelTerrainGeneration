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


    //Different classes might use jobs differently and expect different params in callbacks.
    //Let classes define their own structs for their own uses.
    private struct JobData
    {
        public JobHandle handle;
        public Action<object> callback;
        public object callbackData;
    }

    private List<JobData> jobs;

    private void Awake()
    {
        if (Manager is not null)
        {
            Destroy(this);
        }

        else
        {
            DontDestroyOnLoad(this.gameObject);
            Manager = this;
            jobs = new();
        }
    }

    void Update()
    {
        for (int i = jobs.Count-1; i >= 0; i--)
        {
            JobData current = jobs[i];
            if (current.handle.IsCompleted)
            {
                current.handle.Complete();
                current.callback?.Invoke(current.callbackData);
                jobs.RemoveAt(i);
            }
        }
    }
    
    public void AddJob(JobHandle handle, Action<object> callback, object callbackData)
    {
        jobs.Add(
            new JobData
            {
                handle = handle,
                callback = callback,
                callbackData = callbackData
            }
        );
    }
}
