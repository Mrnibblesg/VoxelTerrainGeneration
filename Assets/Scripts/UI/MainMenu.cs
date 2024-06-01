using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
public class MainMenu : MonoBehaviour
{
    public TMP_InputField seedInput;
    public Slider heightSlider;
    public Slider resolutionSlider;
    public Slider chunkSizeSlider;
    public Slider chunkHeightSlider;
    public Slider waterHeightSlider;
    public Button generateWorldButton;
    public Button quitButton;

    // Class-level variables to store the slider values
    private WorldParameters worldParameters;

    void Start()
    {
        generateWorldButton.onClick.AddListener(GenerateWorld);
        quitButton.onClick.AddListener(QuitGame);

        InitializeDefaultWorld();
    }

    void InitializeDefaultWorld()
    {
        WorldBuilder.InitializeMenuWorld();
    }

    void GenerateWorld()
    {
        worldParameters = new WorldParameters
        {
            Resolution = (int)resolutionSlider.value,
            WorldHeightInChunks = (int)heightSlider.value,
            ChunkSize = (int)chunkSizeSlider.value,
            ChunkHeight = (int)chunkHeightSlider.value,
            WaterHeight = (int)waterHeightSlider.value,
            Seed = string.IsNullOrEmpty(seedInput.text) ? 0 : int.Parse(seedInput.text),
            Name = "New World"
        };

        // Debug to ensure values are captured correctly
        Debug.Log($"Generating world with seed: {worldParameters.Seed}," +
            $"height: {worldParameters.WorldHeightInChunks}," +
            $"resolution: {worldParameters.Resolution}," +
            $"chunk size: {worldParameters.ChunkSize}," +
            $"water height: {worldParameters.WaterHeight}");

        

        // Load the world generation scene
        SceneManager.LoadScene("Za Warudo", LoadSceneMode.Single);

        //When we generate a world with this method, we want to place in an agent the player can control.
        //Other methods like the profiler will create a different agent.
        SceneManager.sceneLoaded += PlayUsingPlayerAgent;
    }

    public void PlayUsingPlayerAgent(Scene scene, LoadSceneMode mode)
    {
        // Build the world
        World w = new WorldBuilder()
            .SetParameters(worldParameters)
            .Build();

        //Spawn a new clone of the player prefab, and save a reference to its script
        Player p = Instantiate(Resources.Load("Prefabs/Player"), null, true)
            .GetComponent<Player>();

        p.CurrentWorld = w;
        p.gameObject.SetActive(true);
        SceneManager.sceneLoaded -= PlayUsingPlayerAgent;
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}