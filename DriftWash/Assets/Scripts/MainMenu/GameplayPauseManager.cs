using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameplayPauseManager : MonoBehaviour
{
    [Header("UI Canvas Panels")]
    public GameObject pauseMenuPanel;
    public GameObject pauseButtonsGroup;
    public GameObject pauseSettingsSubPanel;

    [Header("Settings Components")]
    public UnityEngine.UI.Toggle beaconToggle;

    private bool isPaused = false;
    private bool isUpdatingUIInternally = false; // Prevents the script from fighting itself

    void Start()
    {
        // Set up the default clean layout states on startup
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseButtonsGroup != null) pauseButtonsGroup.SetActive(true);
        if (pauseSettingsSubPanel != null) pauseSettingsSubPanel.SetActive(false);

        // Sync up the checkbox visual checkmark state to match PlayerPrefs
        SyncToggleGraphicOnly();

        // Ensure time is running normally on level start
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Listen for the modern Input System package escape key check
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePauseState();
        }
    }

    public void TogglePauseState()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        // Reset back to the primary list of pause buttons whenever the window opens or closes
        if (pauseButtonsGroup != null) pauseButtonsGroup.SetActive(true);
        if (pauseSettingsSubPanel != null) pauseSettingsSubPanel.SetActive(false);

        // Freeze game physics when paused (0), run normal speed when unpaused (1)
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void OpenPauseSettings()
    {
        if (pauseButtonsGroup != null) pauseButtonsGroup.SetActive(false);
        if (pauseSettingsSubPanel != null) pauseSettingsSubPanel.SetActive(true);

        // Safety refresh the checkbox visual checkmark when manually opening the sub-panel
        SyncToggleGraphicOnly();
    }

    public void ClosePauseSettings()
    {
        if (pauseButtonsGroup != null) pauseButtonsGroup.SetActive(true);
        if (pauseSettingsSubPanel != null) pauseSettingsSubPanel.SetActive(false);
    }

    private void SyncToggleGraphicOnly()
    {
        if (beaconToggle != null)
        {
            isUpdatingUIInternally = true; // Raise the guard shield
            beaconToggle.isOn = PlayerPrefs.GetInt("BeaconActive", 1) == 1;
            isUpdatingUIInternally = false; // Drop the guard shield
        }
    }

    // HOOK THIS UP TO YOUR PAUSE MENU TOGGLE Object's OnValueChanged Event!
    public void SetBeaconStateFromPause(bool isOn)
    {
        // If the script is just updating the visual checkbox checkmark graphic internally,
        // ignore it so we don't accidentally wipe out the player's saved preference!
        if (isUpdatingUIInternally) return;

        // Save the manual click choice permanently to memory
        PlayerPrefs.SetInt("BeaconActive", isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("[PauseSettings] Manual setting toggled via click. Active = " + isOn);

        // Instantly locate the vehicle beacon flasher script and tell it to update right now
        BeaconFlasher flasher = Object.FindAnyObjectByType<BeaconFlasher>();
        if (flasher != null)
        {
            flasher.RefreshBeaconActiveState();
        }
    }

    public void ReturnToMainMenuWindow()
    {
        Time.timeScale = 1f; // Always reset time scale back to 1 before moving scenes!
        SceneManager.LoadScene("MainMenuScene");
    }
}
