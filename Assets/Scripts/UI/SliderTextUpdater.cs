using UnityEngine;
using UnityEngine.UI;
using TMPro; // Include this if you are using TextMeshPro for the text display

public class SliderTextUpdater : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI displayText; // Change to 'public Text text;' if you are using standard UI Text

    void Start()
    {
        if (slider != null && displayText != null)
        {
            slider.onValueChanged.AddListener(UpdateTextDisplay);
            UpdateTextDisplay(slider.value); // Update text on start to display the initial value
        }
    }

    void UpdateTextDisplay(float value)
    {
        displayText.text = value.ToString("0"); // "0" format for integer values, use "0.0" for one decimal place
    }
}
