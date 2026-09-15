using UnityEngine;

public class PuddleProgressTracker : MonoBehaviour
{
    [Header("UI Components")]
    public UnityEngine.UI.Image progressBarImage;
    public TMPro.TextMeshProUGUI progressText;

    [Header("Material Flash Effects")]
    public MeshRenderer floorMeshRenderer;
    public Material glowMaterial; // <-- Drag your new FloorGlowMaterial here!
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

        UpdateUIElements(0f);
    }

    public void OnPuddleCleaned()
    {
        if (levelFinished) return;
        puddlesCleanedCount++;

        float playerFriendlyPercentage = Mathf.Clamp01((float)puddlesCleanedCount / totalPuddlesInLevel) * 100f;
        UpdateUIElements(playerFriendlyPercentage);

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

    void UpdateUIElements(float displayScore)
    {
        if (progressBarImage != null) progressBarImage.fillAmount = displayScore / 100f;
        if (progressText != null) progressText.text = displayScore.ToString("F0") + "%";
    }
}
