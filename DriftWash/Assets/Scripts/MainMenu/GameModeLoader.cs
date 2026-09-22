using UnityEngine;

public class GameModeLoader : MonoBehaviour
{
    void Awake()
    {
        // 1. Check PlayerPrefs to see what mode index was saved by the main menu buttons
        // 0 = Full Floor Mode, 1 = Smaller Sections (Puddles) Mode
        int modeChoice = PlayerPrefs.GetInt("SelectedGameMode", 0);

        LevelProgressTracker floorScript = GetComponent<LevelProgressTracker>();
        PuddleProgressTracker puddleScript = GetComponent<PuddleProgressTracker>();

        // 2. Enable the correct tracking script component
        if (floorScript != null) floorScript.enabled = (modeChoice == 0);
        if (puddleScript != null) puddleScript.enabled = (modeChoice == 1);

        // 3. FIXED: Handle the large dirty floor visibility!
        MeshRenderer floorRenderer = GetComponent<MeshRenderer>();
        if (floorRenderer != null && floorRenderer.material != null)
        {
            Material mat = floorRenderer.material;

            if (modeChoice == 1)
            {
                // If we are in Puddle Mode, push the shader graph Lerp completely to the CLEAN texture track!
                // If your custom shader uses a different mask input string name, make sure it matches here.
                if (mat.HasProperty("_MaskTexture")) mat.SetTexture("_MaskTexture", Texture2D.whiteTexture);

                Debug.Log("[GameModeLoader] Full floor dirt disabled. Ready for puddle hunting!");
            }
        }
    }
}
