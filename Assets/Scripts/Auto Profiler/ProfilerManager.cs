
//TO ENABLE/DISABLE THE AUTOMATIC PROFILER then set/remove the
//scripting variable "PROFILER_ENABLED" in
//Edit>Project Settings>Player>Other settings>Script compilation.

//The profiler will be a hook that things in the app can hook in to to tell it to measure a task.
//Important functions that perform the work of tasks will use preprocessor directives
//so that profiler code gets cut out of the actual build.
//I need the profiler to accept data from 
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfilerManager : MonoBehaviour
{
    public static ProfilerManager Manager { get; private set; }
    ProfilerAgent agent;
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
    /// Start() is called.
    /// </summary>
    void Awake()
    {
        if (Manager is not null)
        {
            Destroy(this);
        }

        else
        {
            DontDestroyOnLoad(this.gameObject);
            Manager = this;
        }
    }

    //Create the agent.
    //Load the scene.
    //Spawn the agent.
    //Start performing actions.
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
        Agent.Initialize(FinishProfiling);
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
        Agent.CurrentWorld = w;
    }

    /// <summary>
    /// Runs the entire test suite. Performs player actions autonomously.
    /// Results are recorded to a file at the end.
    /// </summary>
    /// <returns>If the suite was ran successfully.</returns
    public bool RunTestSuite()
    {
        Agent.AddTask(new ProfileGameTask(Agent));
        return true;
    }

    //struct for world params would be helpful so I don't need to pass in a billion variables
    private bool RunScenario(int runs, int resolution, int renderDistance, int worldHeight, int chunkSize)
    {

        return true;
    }

    /// <summary>
    /// Save the current data under the given scenario name
    /// </summary>
    public void CompleteScenario(string scenarioName)
    {

    }

    public void FinishProfiling()
    {

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
