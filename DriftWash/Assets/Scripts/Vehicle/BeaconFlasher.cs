using UnityEngine;
using System.Collections;

public class BeaconFlasher : MonoBehaviour
{
    [Header("Target Mesh")]
    public MeshRenderer bulbMeshRenderer;

    [Header("Arcade Pulse Durations")]
    public float fadeInDuration = 0.25f;
    public float fadeOutDuration = 0.25f;
    public float stayOffDuration = 0.4f;

    [Header("Glow Colours")]
    [ColorUsage(true, false)] public Color baseOrangeColor = new Color(1f, 0.4f, 0f, 1f);
    [ColorUsage(true, true)] public Color activeGlowColor = new Color(2f, 0.8f, 0f) * 4f;

    private Material bulbMaterial;
    private bool isFlasherRunning = true;
    private Coroutine flashCoroutine;

    void Start()
    {
        if (bulbMeshRenderer != null)
        {
            bulbMaterial = bulbMeshRenderer.material;
            if (bulbMaterial.HasProperty("_EmissionColor"))
            {
                bulbMaterial.EnableKeyword("_EMISSION");
            }

            // Instantly sync up to whatever was clicked in the Main Menu settings on startup
            RefreshBeaconActiveState();
        }
        else
        {
            Debug.LogError("[BeaconFlasher] Missing a Mesh Renderer assignment!");
            enabled = false;
        }
    }

    // This public function can be called by any script at any time to instantly update the light
    public void RefreshBeaconActiveState()
    {
        // 1 = On (Default), 0 = Off
        int beaconActiveSetting = PlayerPrefs.GetInt("BeaconActive", 1);

        if (beaconActiveSetting == 0)
        {
            // Turn off the light completely
            isFlasherRunning = false;
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
            if (bulbMaterial != null)
            {
                bulbMaterial.SetColor("_EmissionColor", Color.black);
            }
            Debug.Log("[BeaconFlasher] Light forced OFF.");
        }
        else
        {
            // Turn on the light loop
            isFlasherRunning = true;
            if (flashCoroutine == null)
            {
                flashCoroutine = StartCoroutine(FlashRoutineLoopUnscaled());
                Debug.Log("[BeaconFlasher] Light forced ON.");
            }
        }
    }

    IEnumerator FlashRoutineLoopUnscaled()
    {
        while (isFlasherRunning && bulbMaterial != null)
        {
            float elapsedTime = 0f;

            // Fade In (Uses unscaled delta time so it works while paused!)
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / fadeInDuration);
                bulbMaterial.SetColor("_EmissionColor", Color.Lerp(baseOrangeColor, activeGlowColor, t));
                yield return null;
            }

            elapsedTime = 0f;

            // Fade Out
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / fadeOutDuration);
                bulbMaterial.SetColor("_EmissionColor", Color.Lerp(activeGlowColor, baseOrangeColor, t));
                yield return null;
            }

            bulbMaterial.SetColor("_EmissionColor", baseOrangeColor);

            // Stay off for the rest duration
            yield return new WaitForSecondsRealtime(stayOffDuration);
        }
    }

    private void OnDestroy()
    {
        if (bulbMaterial != null) Destroy(bulbMaterial);
    }
}
