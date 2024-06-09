using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilerDetector : MonoBehaviour
{
    public bool showIfProfilerEnabled;
    private void Awake()
    {
#if PROFILER_ENABLED
        gameObject.SetActive(showIfProfilerEnabled);
#else
        gameObject.SetActive(!showIfProfilerEnabled);
#endif
    }
}
