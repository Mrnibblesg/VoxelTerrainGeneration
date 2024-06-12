using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;
public class MainMenu : MonoBehaviour
{
    public TMP_InputField seedInput;
    public TMP_InputField addressInput;
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
        float resolution = Mathf.Pow(2, (float)resolutionSlider.value - 1);
        worldParameters = new WorldParameters
        {
            Resolution = resolution,
            WorldHeightInChunks = (int)heightSlider.value * (int)resolution,
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

    public void JoinWorld()
    {
        SceneManager.LoadScene("Za Warudo", LoadSceneMode.Single);

        NetworkManager.singleton.networkAddress = addressInput.text;

        NetworkManager.singleton.StartClient();
    }

    public void PlayUsingPlayerAgent(Scene scene, LoadSceneMode mode)
    {
        NetworkManager.singleton.StartHost();

        // Build the world
        World w = new WorldBuilder()
            .SetParameters(worldParameters)
            .Build();

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