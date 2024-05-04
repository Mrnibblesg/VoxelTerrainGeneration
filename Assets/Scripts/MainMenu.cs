using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class MainMenu : MonoBehaviour
{
    public TMP_InputField seedInput;
    public Slider heightSlider;
    public Slider resolutionSlider;
    public Slider chunkSizeSlider;
    public Button generateWorldButton;
    public Button quitButton;

    // Class-level variables to store the slider values
    private int seed;
    private int height;
    private int resolution;
    private int chunkSize;

    void Start()
    {
        generateWorldButton.onClick.AddListener(GenerateWorld);
        quitButton.onClick.AddListener(QuitGame);
    }

    void GenerateWorld()
    {
        seed = string.IsNullOrEmpty(seedInput.text) ? 0 : int.Parse(seedInput.text);
        height = (int)heightSlider.value;
        resolution = (int)resolutionSlider.value;
        chunkSize = (int)chunkSizeSlider.value;

        // Debug to ensure values are captured correctly
        Debug.Log($"Generating world with seed: {seed}, height: {height}, resolution: {resolution}, chunk size: {chunkSize}");

        // Load the world generation scene
        SceneManager.LoadScene("Za Warudo", LoadSceneMode.Single);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();
        if (worldGenerator != null)
        {
            worldGenerator.SetDimensions(resolution, height, chunkSize, chunkSize); // chunk height and size the same, will make cubes
            // resolution is controlling world size right now, could change to render distance?
            worldGenerator.StartGeneration();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded; // Detach the event to prevent it from being called again
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
