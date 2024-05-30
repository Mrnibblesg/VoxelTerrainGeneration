
//UNCOMMENT THIS LINE TO ENABLE THE AUTOMATIC PROFILER
#define PROFILER_ENABLED

//The profiler will be a hook that things in the app can hook in to to tell it to measure a task.
//Important functions that perform the work of tasks will use preprocessor directives
//so that profiler code gets cut out of the actual build.
//I need the profiler to accept data from 
public class Profiler
{
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

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>whether it was successful or not</returns>
    bool RecordToFile()
    {
        return false;
    }
}
