using UnityEngine;

public class GameModeLoader : MonoBehaviour
{
    [Header("Puddle Setup")]
    [Tooltip("Drag your '_PuddleContainer' parent object here from the Hierarchy.")]
    public GameObject puddleContainer;

    void Awake()
    {
        // 0 = Full Floor Mode, 1 = Smaller Sections (Puddles) Mode
        int modeChoice = PlayerPrefs.GetInt("SelectedGameMode", 0);

        LevelProgressTracker floorScript = GetComponent<LevelProgressTracker>();
        PuddleProgressTracker puddleScript = GetComponent<PuddleProgressTracker>();

        // Enable the correct tracking script component
        if (floorScript != null) floorScript.enabled = (modeChoice == 0);
        if (puddleScript != null) puddleScript.enabled = (modeChoice == 1);

        // Show or hide the physical puddle objects dynamically based on the mode choice
        if (puddleContainer != null)
        {
            puddleContainer.SetActive(modeChoice == 1);
            Debug.Log("[GameModeLoader] Puddle container visibility updated. Active = " + (modeChoice == 1));
        }

        // Handle the large dirty floor texture visibility
        MeshRenderer floorRenderer = GetComponent<MeshRenderer>();
        if (floorRenderer != null && floorRenderer.material != null)
        {
            Material mat = floorRenderer.material;
            if (modeChoice == 1)
            {
                if (mat.HasProperty("_MaskTexture")) mat.SetTexture("_MaskTexture", Texture2D.whiteTexture);
                Debug.Log("[GameModeLoader] Full floor dirt disabled. Ready for puddle hunting!");
            }
        }
    }

    void Start()
    {
        // FIXED FOR PUDDLE MODE: Instantly locate the car flasher and force a configuration update 
        // to bypass any conflicting scene asset start thread timings
        BeaconFlasher flasher = Object.FindAnyObjectByType<BeaconFlasher>();
        if (flasher != null)
        {
            flasher.RefreshBeaconActiveState();
        }
    }
}
