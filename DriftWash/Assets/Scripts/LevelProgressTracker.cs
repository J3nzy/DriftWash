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

    [Header("Material Flash Effects")]
    public MeshRenderer floorMeshRenderer;
    public Material glowMaterial; // <-- Drag your new FloorGlowMaterial here!
    public float shineDuration = 1.0f;

    private float realCleanPercentage = 0f;
    private bool levelFinished = false;
    private Texture2D readableTexture;

    private Material originalMaterial;
    private Material runtimeGlowInstance;

    void Start()
    {
        if (maskTexture == null) return;
        readableTexture = new Texture2D(maskTexture.width, maskTexture.height, TextureFormat.R8, false);

        if (floorMeshRenderer != null)
        {
            originalMaterial = floorMeshRenderer.sharedMaterial;
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

                    RenderTexture.active = maskTexture;
                    GL.Clear(true, true, Color.white);
                    RenderTexture.active = previousActive;

                    StartCoroutine(AnimateMaterialSwapGlow());
                    Debug.Log("LEVEL COMPLETE! Full floor sheet polished!");
                }
            }
        }
    }

    System.Collections.IEnumerator AnimateMaterialSwapGlow()
    {
        if (floorMeshRenderer == null || glowMaterial == null) yield break;

        // Create an instance of the glow material so we don't permanently alter the project asset
        runtimeGlowInstance = new Material(glowMaterial);

        // SWAP IN: Put the bright glowing material onto the floor mesh
        floorMeshRenderer.material = runtimeGlowInstance;

        float elapsedTime = 0f;
        float halfDuration = shineDuration * 0.5f;
        Color targetGlowColor = runtimeGlowInstance.HasProperty("_EmissionColor") ? runtimeGlowInstance.GetColor("_EmissionColor") : Color.white * 5f;

        // Fade In: Build up the emission intensity
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;
            if (runtimeGlowInstance.HasProperty("_EmissionColor"))
                runtimeGlowInstance.SetColor("_EmissionColor", Color.Lerp(Color.black, targetGlowColor, t));
            yield return null;
        }

        elapsedTime = 0f;

        // Fade Out: Bring down the emission brightness
        while (elapsedTime < halfDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / halfDuration;
            if (runtimeGlowInstance.HasProperty("_EmissionColor"))
                runtimeGlowInstance.SetColor("_EmissionColor", Color.Lerp(targetGlowColor, Color.black, t));
            yield return null;
        }

        // SWAP OUT: Put your beautiful original clean tile material back
        floorMeshRenderer.material = originalMaterial;

        // Clean up the temporary material instance from memory
        Destroy(runtimeGlowInstance);
    }

    void OnDestroy() { if (readableTexture != null) Destroy(readableTexture); }
}
