using UnityEngine;

public class PuddleProgressTracker : MonoBehaviour
{
    [Header("UI Components")]
    public UnityEngine.UI.Slider progressSlider; // Assign your PuddleProgressSlider here
    public TMPro.TextMeshProUGUI progressText;

    [Header("Material Flash Effects")]
    public MeshRenderer floorMeshRenderer;
    public Material glowMaterial; // <-- Drag your FloorGlowMaterial here!
    public float shineDuration = 1.0f;

    private int totalPuddlesInLevel = 0;
    private int puddlesCleanedCount = 0;
    private bool levelFinished = false;

    private Material originalMaterial;
    private Material runtimeGlowInstance;

    void Start()
    {
        if (floorMeshRenderer != null)
        {
            originalMaterial = floorMeshRenderer.sharedMaterial;
        }

        GameObject[] puddles = GameObject.FindGameObjectsWithTag("DirtPuddle");
        totalPuddlesInLevel = puddles.Length;

        // Initialize slider at 0
        if (progressSlider != null) progressSlider.value = 0f;
        if (progressText != null) progressText.text = "0%";
    }

    public void OnPuddleCleaned()
    {
        if (levelFinished) return;
        puddlesCleanedCount++;

        // Calculate a 0 to 1 value for the slider, and a 0 to 100 percentage for text
        float absoluteRatio = totalPuddlesInLevel > 0 ? (float)puddlesCleanedCount / totalPuddlesInLevel : 1f;
        float playerFriendlyPercentage = Mathf.Clamp01(absoluteRatio) * 100f;

        UpdateUIElements(absoluteRatio, playerFriendlyPercentage);

        if (puddlesCleanedCount >= totalPuddlesInLevel)
        {
            levelFinished = true;
            StartCoroutine(AnimateMaterialSwapGlow());
            Debug.Log("[Puddle Mode] LEVEL COMPLETE! All store spills vaporized!");
        }
    }

    System.Collections.IEnumerator AnimateMaterialSwapGlow()
    {
        if (floorMeshRenderer == null || glowMaterial == null) yield break;

        runtimeGlowInstance = new Material(glowMaterial);
        floorMeshRenderer.material = runtimeGlowInstance;

        float elapsedTime = 0f;
        float halfDuration = shineDuration * 0.5f;
        Color targetGlowColor = runtimeGlowInstance.HasProperty("_EmissionColor") ? runtimeGlowInstance.GetColor("_EmissionColor") : Color.white * 5f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;
            if (runtimeGlowInstance.HasProperty("_EmissionColor"))
                runtimeGlowInstance.SetColor("_EmissionColor", Color.Lerp(Color.black, targetGlowColor, t));
            yield return null;
        }

        elapsedTime = 0f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;
            if (runtimeGlowInstance.HasProperty("_EmissionColor"))
                runtimeGlowInstance.SetColor("_EmissionColor", Color.Lerp(targetGlowColor, Color.black, t));
            yield return null;
        }

        floorMeshRenderer.material = originalMaterial;
        Destroy(runtimeGlowInstance);
    }

    void UpdateUIElements(float sliderValue, float displayPercentage)
    {
        if (progressSlider != null) progressSlider.value = sliderValue;
        if (progressText != null) progressText.text = displayPercentage.ToString("F0") + "%";
    }
}
