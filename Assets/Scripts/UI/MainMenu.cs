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
    public Slider waterHeightSlider;
    public Button generateWorldButton;
    public Button quitButton;

    // Class-level variables to store the slider values
    private int seed;
    private int height;
    private int resolution;
    private int chunkSize;
    private int waterHeight;

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
        seed = string.IsNullOrEmpty(seedInput.text) ? 0 : int.Parse(seedInput.text);
        height = (int)heightSlider.value;
        resolution = (int)resolutionSlider.value;
        chunkSize = (int)chunkSizeSlider.value;
        waterHeight = (int)waterHeightSlider.value;

        // Debug to ensure values are captured correctly
        Debug.Log($"Generating world with seed: {seed}, height: {height}, resolution: {resolution}, chunk size: {chunkSize}, water height: {waterHeight}");

        

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
            .SetDimensions(resolution, height, chunkSize, chunkSize, waterHeight)
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