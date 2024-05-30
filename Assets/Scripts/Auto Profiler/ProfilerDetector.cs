using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilerDetector : MonoBehaviour
{
    // Start is called before the first frame update
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
