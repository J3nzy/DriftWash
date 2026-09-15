using UnityEngine;
using System.Collections;

public class BeaconFlasher : MonoBehaviour
{
    [Header("Target Mesh")]
    public MeshRenderer bulbMeshRenderer;

    [Header("Arcade Pulse Durations")]
    [Tooltip("How long (in seconds) the light takes to swell up to full brightness.")]
    public float fadeInDuration = 0.25f;

    [Tooltip("How long (in seconds) the light takes to dim completely back to normal glass.")]
    public float fadeOutDuration = 0.25f;

    [Tooltip("How long (in seconds) the light stays completely dark before flashing again.")]
    public float stayOffDuration = 0.4f; // <-- Control your exact rest time directly here!

    [Header("Glow Colours")]
    [ColorUsage(true, false)] public Color baseOrangeColor = new Color(1f, 0.4f, 0f, 1f);
    [ColorUsage(true, true)] public Color activeGlowColor = new Color(2f, 0.8f, 0f) * 4f;

    private Material bulbMaterial;
    private bool isFlasherRunning = true;

    void Start()
    {
        // Check if the user turned off the beacon in the main menu settings
        int beaconActiveSetting = PlayerPrefs.GetInt("BeaconActive", 1);
        if (beaconActiveSetting == 0)
        {
            // Turn off the light permanently and disable this script component
            if (bulbMeshRenderer != null)
            {
                bulbMeshRenderer.material.SetColor("_EmissionColor", baseOrangeColor);
            }
            enabled = false;
            return;
        }

        if (bulbMeshRenderer != null)
        {
            bulbMaterial = bulbMeshRenderer.material;
            if (bulbMaterial.HasProperty("_EmissionColor"))
            {
                bulbMaterial.EnableKeyword("_EMISSION");
            }
            StartCoroutine(FlashRoutineLoop());
        }
        else
        {
            Debug.LogError("[BeaconFlasher] Missing a Mesh Renderer assignment!");
            enabled = false;
        }
    }

    IEnumerator FlashRoutineLoop()
    {
        while (isFlasherRunning && bulbMaterial != null)
        {
            float elapsedTime = 0f;

            // PHASE 1: Smooth, satisfying Fade In
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / fadeInDuration);
                bulbMaterial.SetColor("_EmissionColor", Color.Lerp(baseOrangeColor, activeGlowColor, t));
                yield return null;
            }

            elapsedTime = 0f;

            // PHASE 2: Smooth, satisfying Fade Out
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / fadeOutDuration);
                bulbMaterial.SetColor("_EmissionColor", Color.Lerp(activeGlowColor, baseOrangeColor, t));
                yield return null;
            }

            // Hard reset to completely off
            bulbMaterial.SetColor("_EmissionColor", baseOrangeColor);

            // PHASE 3: The exact rest window delay!
            yield return new WaitForSeconds(stayOffDuration);
        }
    }

    private void OnDestroy()
    {
        if (bulbMaterial != null) Destroy(bulbMaterial);
    }
}
