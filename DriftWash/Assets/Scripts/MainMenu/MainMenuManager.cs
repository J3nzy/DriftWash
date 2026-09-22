using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;
    public GameObject modeSelectPanel;

    private string selectedSceneName;

    void Start()
    {
        ShowPanel(mainMenuPanel);

        // If a new player opens the game for the first time, default the light to On (1)
        if (!PlayerPrefs.HasKey("BeaconActive"))
        {
            PlayerPrefs.SetInt("BeaconActive", 1);
            PlayerPrefs.Save();
        }
    }

    // --- BUTTON NAVIGATION FUNCTIONS ---

    public void OpenLevelSelect()
    {
        ShowPanel(levelSelectPanel);
    }

    public void OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    public void BackToMainMenu()
    {
        ShowPanel(mainMenuPanel);
    }

    public void BackToLevelSelect()
    {
        ShowPanel(levelSelectPanel);
    }

    public void ExitGame()
    {
        Debug.Log("Exiting Game Application...");
        Application.Quit();
    }

    // --- BEACON SETTING FUNCTIONS ---

    public void SetBeaconState(bool isOn)
    {
        PlayerPrefs.SetInt("BeaconActive", isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Settings Update: Beacon Active = " + isOn);
    }

    // --- LEVEL SELECT MODE FUNCTIONS ---

    public void OnLevelButtonClicked(string sceneName)
    {
        selectedSceneName = sceneName;
        // Pop open the mode picker panel (Full Floor vs Smaller Sections)
        ShowPanel(modeSelectPanel);
    }

    public void SelectFullFloorMode()
    {
        // 0 = Full Floor Mode
        PlayerPrefs.SetInt("SelectedGameMode", 0);
        PlayerPrefs.Save();
        LoadTargetLevel();
    }

    public void SelectSmallerSectionsMode()
    {
        // 1 = Smaller Sections (Puddles) Mode
        PlayerPrefs.SetInt("SelectedGameMode", 1);
        PlayerPrefs.Save();
        LoadTargetLevel();
    }

    private void LoadTargetLevel()
    {
        if (!string.IsNullOrEmpty(selectedSceneName))
        {
            SceneManager.LoadScene(selectedSceneName);
        }
        else
        {
            Debug.LogError("[MainMenuManager] No level scene name specified!");
        }
    }

    // Helper function to cleanly swap canvas panels
    private void ShowPanel(GameObject targetPanel)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(mainMenuPanel == targetPanel);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(levelSelectPanel == targetPanel);
        if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == targetPanel);
        if (modeSelectPanel != null) modeSelectPanel.SetActive(modeSelectPanel == targetPanel);
    }
}
