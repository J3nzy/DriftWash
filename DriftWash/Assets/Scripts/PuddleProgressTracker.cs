using UnityEngine;

public class PuddleProgressTracker : MonoBehaviour
{
    [Header("UI Components")]
    public UnityEngine.UI.Image progressBarImage;
    public TMPro.TextMeshProUGUI progressText;

    [Header("Floor Shine Animation")]
    public MeshRenderer floorMeshRenderer;
    public Color shineGlowColor = new Color(15f, 15f, 15f, 1f); // Boosted to 15x for white tiles!
    public float shineDuration = 1.0f;

    private int totalPuddlesInLevel = 0;
    private int puddlesCleanedCount = 0;
    private bool levelFinished = false;
    private Material floorMaterial;

    void Start()
    {
        if (floorMeshRenderer != null)
        {
            floorMaterial = floorMeshRenderer.material;
            if (floorMaterial.HasProperty("_EmissionColour"))
            {
                floorMaterial.SetColor("_EmissionColour", Color.black);
            }
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
            StartCoroutine(AnimateFloorShineGlow());
            Debug.Log("[Puddle Mode] LEVEL COMPLETE! All store spills vaporized!");
        }
    }

    System.Collections.IEnumerator AnimateFloorShineGlow()
    {
        if (floorMaterial == null) yield break;
        float elapsedTime = 0f;
        float halfDuration = shineDuration * 0.5f;

        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            Color currentGlow = Color.Lerp(Color.black, shineGlowColor, elapsedTime / halfDuration);
            floorMaterial.SetColor("_EmissionColour", currentGlow);
            yield return null;
        }

        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            Color currentGlow = Color.Lerp(shineGlowColor, Color.black, elapsedTime / halfDuration);
            floorMaterial.SetColor("_EmissionColour", currentGlow);
            yield return null;
        }
        floorMaterial.SetColor("_EmissionColour", Color.black);
    }

    void UpdateUIElements(float displayScore)
    {
        if (progressBarImage != null) progressBarImage.fillAmount = displayScore / 100f;
        if (progressText != null) progressText.text = displayScore.ToString("F0") + "%";
    }
}
