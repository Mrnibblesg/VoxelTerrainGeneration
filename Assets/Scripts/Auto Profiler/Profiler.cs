
//TO ENABLE/DISABLE THE AUTOMATIC PROFILER (only for use in the player) then set/remove the
//scripting variable "PROFILER_ENABLED" in
//Edit>Project Settings>Player>Other settings>Script compilation.

//The profiler will be a hook that things in the app can hook in to to tell it to measure a task.
//Important functions that perform the work of tasks will use preprocessor directives
//so that profiler code gets cut out of the actual build.
//I need the profiler to accept data from 
using System.Diagnostics;

public class Profiler
{
    public static Profiler Controller { get; private set; }
    public Profiler()
    {

    }
    /// <summary>
    /// Runs the entire test suite. Performs player actions autonomously.
    /// Results are recorded to a file at the end.
    /// </summary>
    /// <returns>If the suite was ran successfully.</returns>
    bool RunTestSuite()
    {
#if !PROFILER_ENABLED
        return false;
#endif
        //Stress test scenario

        return RecordToFile();
    }

    //struct for world params would be helpful so I don't need to pass in a billion variables
    bool RunScenario(int runs, int resolution, int renderDistance, int worldHeight, int chunkSize)
    {

        return true;
    }

    /// <summary>
    /// Save the current data as a part of the given scenario.
    /// </summary>
    void SaveData(string scenarioName)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>whether it was successful or not</returns>
    bool RecordToFile()
    {
        Debug.Print("Profiler finished");
        return false;
    }
}
