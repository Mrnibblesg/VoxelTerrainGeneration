
//TO ENABLE/DISABLE THE AUTOMATIC PROFILER then set/remove the
//scripting variable "PROFILER_ENABLED" in
//Edit>Project Settings>Player>Other settings>Script compilation.

//The profiler will be a hook that things in the app can hook in to to tell it to measure a task.
//Important functions that perform the work of tasks will use preprocessor directives
//so that profiler code gets cut out of the actual build.

//This ProfilerManager uses the FrameTimingManager, which is always active for Development Player builds.
//It must be explicitly set in the unity editor if you want it active in the editor or a release build.
//See https://docs.unity3d.com/Manual/frame-timing-manager.html for more details.

//Due to the nature of profiling, it's impossible to get a 100% accurate reading on how well
//something actually works, you can only get a general sense. Keep this in mind as you
//parse the profiler data.

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfilerManager : MonoBehaviour
{
    public bool recording = false;
    private FrameTiming[] frameTiming;
    //For each scenario, measure: 

    private long scenarioStartTime;
    
    private double scenarioMaxFrameTime;
    private double scenarioMaxGPUFrameTime;

    private List<double> CPUframeTimes;

    private struct ScenarioData
    {
        public string name;
        public double maxCPUFrameTime;
        public double maxGPUFrameTime;
        public double averageCPUFrameTime;
        public long scenarioDuration;

    }
    private List<ScenarioData> dataForScenarios;

    public static ProfilerManager Manager { get; private set; }
    ProfilerAgent agent;
    private bool worstChunks = false;
    public ProfilerAgent Agent {
        get { return agent; }
        protected set
        {
            if (agent != null)
            {
                Destroy(agent.gameObject);
            }
            agent = value;
        }
    }

    /// <summary>
    /// Create this to start a profiling session. It doesn't begin until
    /// Start() is called. If PROFILER_ENABLED is false, it is not possible to use this class.
    /// </summary>
    void Awake()
    {
#if !PROFILER_ENABLED
        Destroy(this);
#endif
        if (Manager is not null)
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        Manager = this;

        dataForScenarios = new();
        scenarioStartTime = DateTime.Now.Ticks;

        scenarioMaxFrameTime = -1;
        scenarioMaxGPUFrameTime = -1;

        CPUframeTimes = new();
        frameTiming = new FrameTiming[1];
    }

    /// <summary>
    /// Used to measure stats for the currently running scenario.
    /// </summary>
    private void Update()
    {
        if (!recording)
        {
            return;
        }
        FrameTimingManager.CaptureFrameTimings();
        uint amt = FrameTimingManager.GetLatestTimings(1, frameTiming);
        if (amt > 0)
        {
            double CPUFrameTime = frameTiming[0].cpuFrameTime;
            CPUframeTimes.Add(CPUFrameTime);
            scenarioMaxFrameTime = Math.Max(scenarioMaxFrameTime, CPUFrameTime);
            scenarioMaxGPUFrameTime = Math.Max(scenarioMaxGPUFrameTime, frameTiming[0].gpuFrameTime);

        }
    }

    /// <summary>
    /// Creates everything needed to profile, and starts the profiling process.
    /// If profiling is disabled then it does nothing.
    /// </summary>
    public void StartProfiling()
    {
#if !PROFILER_ENABLED
        return;
#endif
        //Load a scene for the profiler agent to play in :3
        SceneManager.LoadScene("Za Warudo", LoadSceneMode.Single);
        SceneManager.sceneLoaded += FinishSetup;

    }
    public void FinishSetup(Scene scene,  LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= FinishSetup;
        Agent = SpawnAgent();
        //Agent.CurrentWorld = new WorldBuilder().Build();
        Agent.Initialize();
        Agent.gameObject.SetActive(true);

        //Begin
        RunTestSuite();
    }
    private ProfilerAgent SpawnAgent()
    {
        return Instantiate(Resources.Load("Prefabs/ProfilerAgent"), null, true)
            .GetComponent<ProfilerAgent>();
    }

    /// <summary>
    /// We have a hard time doing certain tasks like world creation and world joining,
    /// so for now we tell the profiler manager to do this for our agent.
    /// </summary>
    public void SetProfilerAgentWorld(WorldParameters parameters)
    {
        World w = new WorldBuilder()
            .SetParameters(parameters)
            .Build();
        w.SetWorstChunks(worstChunks);
        Agent.CurrentWorld = w;
    }
    public void SetProfilerAgentRenderDist(int dist)
    {
        Agent.RenderDist = dist;
        Agent.UnloadDist = dist + 1;
        Agent.CurrentWorld.UpdateAuthAgentChunkPos(Agent);
    }
    public void SetProfilerAgentWorldWorstChunks(bool value)
    {
        worstChunks = value;
    }

    /// <summary>
    /// Runs the entire test suite. Performs player actions autonomously.
    /// Results are recorded to a file at the end.
    /// </summary>
    /// <returns>If the suite was ran successfully.</returns
    public bool RunTestSuite()
    {
        Agent.AddTask(new ProfileGameTask());
        recording = true;
        return true;
    }

    /// <summary>
    /// Save the current data under the given scenario name
    /// </summary>
    public void CompleteScenario(string scenarioName)
    {
        long now = DateTime.Now.Ticks;

        //Duration in ms
        long duration = (now - scenarioStartTime) / TimeSpan.TicksPerMillisecond;
        double avg = CPUframeTimes.Sum() / CPUframeTimes.Count;

        

        ScenarioData data = new ScenarioData()
        {
            name = scenarioName,
            maxCPUFrameTime = scenarioMaxFrameTime,
            maxGPUFrameTime = scenarioMaxGPUFrameTime,
            averageCPUFrameTime = avg,
            scenarioDuration = duration,

        };

        dataForScenarios.Add(data);

        //Set up for the next scenario
        CPUframeTimes.Clear();

        scenarioMaxFrameTime = -1;
        scenarioMaxGPUFrameTime = -1;
        scenarioStartTime = now;

    }

    public void FinishProfiling()
    {
        recording = false;
        RecordToFile();
    }

    /// <summary>
    ///     
    /// </summary>
    /// <returns>whether it was successful or not</returns>
    private bool RecordToFile()
    {

        //Debug.Log("Success!");
        return false;
    }
}
