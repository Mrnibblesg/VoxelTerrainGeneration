using UnityEngine;

public class MenuScreenManager : MonoBehaviour
{

    public GameObject[] menuPanels;

    /// <summary>
    /// Sets the supplied menu to active, while deactivating all other ones.
    /// </summary>
    /// <param name="menu"></param>
    public void SetActive(GameObject menu)
    {
        for (int i = 0; i < menuPanels.Length; i++)
        {
            menuPanels[i].SetActive(false);
        }
        menu.SetActive(true);

    }
}
