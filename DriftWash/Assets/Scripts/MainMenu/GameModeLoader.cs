using UnityEngine;

public class GameModeLoader : MonoBehaviour
{
    void Awake()
    {
        // Check PlayerPrefs to see what game mode integer was saved by the menu buttons
        // 0 = Full Floor Mode, 1 = Puddle Mode
        int modeChoice = PlayerPrefs.GetInt("SelectedGameMode", 0);

        // Grab references to both tracking scripts attached to this same object
        LevelProgressTracker floorScript = GetComponent<LevelProgressTracker>();
        PuddleProgressTracker puddleScript = GetComponent<PuddleProgressTracker>();

        // Safely enable or disable them based on the player's menu choice
        if (floorScript != null)
        {
            floorScript.enabled = (modeChoice == 0);
        }

        if (puddleScript != null)
        {
            puddleScript.enabled = (modeChoice == 1);
        }

        Debug.Log("[GameModeLoader] Active script initialized. Selected Mode Index: " + modeChoice);
    }
}
