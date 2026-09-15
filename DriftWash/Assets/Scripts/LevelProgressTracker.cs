using UnityEngine;

public class LevelProgressTracker : MonoBehaviour
{
    [Header("Texture Setup")]
    public RenderTexture maskTexture;

    [Header("UI Components")]
    public UnityEngine.UI.Image progressBarImage;
    public TMPro.TextMeshProUGUI progressText;

    [Header("Win Target")]
    public float texturePercentToWin = 55f;

    [Header("Floor Shine Animation")]
    public MeshRenderer floorMeshRenderer;
    public Color shineGlowColor = new Color(15f, 15f, 15f, 1f); // Boosted to 15x for white tiles!
    public float shineDuration = 1.0f;

    private float realCleanPercentage = 0f;
    private bool levelFinished = false;
    private Texture2D readableTexture;
    private Material floorMaterial;

    void Start()
    {
        if (maskTexture == null) return;
        readableTexture = new Texture2D(maskTexture.width, maskTexture.height, TextureFormat.R8, false);

        if (floorMeshRenderer != null)
        {
            floorMaterial = floorMeshRenderer.material;
            if (floorMaterial.HasProperty("_EmissionColour"))
            {
                floorMaterial.SetColor("_EmissionColour", Color.black);
            }
        }

        StartCoroutine(ProgressEvaluationLoop());
    }

    System.Collections.IEnumerator ProgressEvaluationLoop()
    {
        while (!levelFinished)
        {
            yield return new WaitForSeconds(0.5f);

            if (maskTexture != null && readableTexture != null)
            {
                RenderTexture previousActive = RenderTexture.active;
                RenderTexture.active = maskTexture;
                readableTexture.ReadPixels(new Rect(0, 0, maskTexture.width, maskTexture.height), 0, 0);
                readableTexture.Apply();
                RenderTexture.active = previousActive;

                byte[] pixelData = readableTexture.GetRawTextureData();
                int cleanCount = 0;

                for (int i = 0; i < pixelData.Length; i++)
                {
                    if (pixelData[i] > 200)
                    {
                        cleanCount++;
                    }
                }

                realCleanPercentage = ((float)cleanCount / pixelData.Length) * 100f;
                float playerFriendlyPercentage = Mathf.Clamp01(realCleanPercentage / texturePercentToWin) * 100f;

                if (progressText != null) progressText.text = playerFriendlyPercentage.ToString("F0") + "%";
                if (progressBarImage != null) progressBarImage.fillAmount = playerFriendlyPercentage / 100f;

                if (realCleanPercentage >= texturePercentToWin)
                {
                    levelFinished = true;
                    if (progressBarImage != null) progressBarImage.fillAmount = 1f;
                    if (progressText != null) progressText.text = "100%";

                    // Force texture clean and trigger white tile flash
                    RenderTexture.active = maskTexture;
                    GL.Clear(true, true, Color.white);
                    RenderTexture.active = previousActive;

                    StartCoroutine(AnimateFloorShineGlow());
                    Debug.Log("LEVEL COMPLETE! Full floor sheet polished!");
                }
            }
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

    void OnDestroy() { if (readableTexture != null) Destroy(readableTexture); }
}
