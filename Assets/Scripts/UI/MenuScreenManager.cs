using UnityEngine;

public class MenuScreenManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject createWorldPanel;
    public GameObject joinWorldPanel;

    public void OpenMainMenu()
    {
        mainMenuPanel.SetActive(true);
        createWorldPanel.SetActive(false);
        joinWorldPanel.SetActive(false);
    }

    public void OpenCreateWorld()
    {
        mainMenuPanel.SetActive(false);
        createWorldPanel.SetActive(true);
        joinWorldPanel.SetActive(false);
    }

    public void OpenJoinWorld()
    {
        mainMenuPanel.SetActive(false);
        createWorldPanel.SetActive(false);
        joinWorldPanel.SetActive(true);
    }
}
