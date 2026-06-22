using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HorrorAtmosphere : MonoBehaviour
{
    [Header("=== NIGHT LIGHTING ===")]
    public Light directionalLight;
    [Range(0f, 0.5f)] public float nightIntensity = 0.08f;
    public Color nightLightColor = new Color(0.75f, 0.8f, 1f); // moonlight — putih sedikit biru
    [Range(0f, 0.1f)] public float ambientIntensity = 0.02f;   // gelap tapi tidak pitch black

    [Header("=== FOG ===")]
    public bool enableFog = true;
    public Color fogColor = new Color(0.08f, 0.08f, 0.1f);     // abu gelap, hampir hitam
    [Range(0f, 0.15f)] public float fogDensity = 0.025f;       // tipis — batas pandang ~30u

    [Header("=== LIGHTNING ===")]
    public bool enableLightning = true;
    public Light lightningLight;
    [Range(2f, 15f)] public float lightningIntensityMin = 4f;
    [Range(2f, 15f)] public float lightningIntensityMax = 9f;
    [Range(0.05f, 0.3f)] public float lightningDuration = 0.12f;
    [Range(5f, 40f)] public float lightningIntervalMin = 10f;
    [Range(10f, 90f)] public float lightningIntervalMax = 25f;

    [Header("=== FLICKER LAMP ===")]
    public bool enableFlicker = true;
    public Light flickerLight;
    [Range(0f, 3f)] public float flickerIntensityMin = 0.2f;
    [Range(0f, 3f)] public float flickerIntensityMax = 0.9f;
    [Range(0.02f, 0.3f)] public float flickerSpeed = 0.07f;

    [Header("=== POST PROCESSING ===")]
    public Volume postProcessVolume;
    [Range(0f, 0.7f)] public float vignetteIntensity = 0.5f;
    [Range(0f, 0.5f)] public float filmGrainIntensity = 0.25f;
    [Range(-60f, 0f)] public float saturation = -20f;           // sedikit desaturate, bukan hitam putih
    [Range(0f, 1f)] public float bloomIntensity = 0.15f;        // bloom tipis saja

    Vignette _vignette;
    FilmGrain _filmGrain;
    ColorAdjustments _colorAdj;
    Bloom _bloom;

    void Start()
    {
        ApplyLighting();
        ApplyFog();
        ApplyPostProcessing();

        if (enableLightning && lightningLight != null)
            StartCoroutine(LightningRoutine());
        if (enableFlicker && flickerLight != null)
            StartCoroutine(FlickerRoutine());
    }

    void ApplyLighting()
    {
        if (directionalLight != null)
        {
            directionalLight.intensity = nightIntensity;
            directionalLight.color = nightLightColor;
        }
        // Ambient gelap tapi netral — tidak tint warna ke material
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.white * ambientIntensity;
    }

    void ApplyFog()
    {
        RenderSettings.fog = enableFog;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }

    void ApplyPostProcessing()
    {
        if (postProcessVolume == null) return;
        var profile = postProcessVolume.profile;

        if (profile.TryGet(out _vignette))
        {
            _vignette.intensity.overrideState = true;
            _vignette.intensity.value = vignetteIntensity;
            _vignette.color.value = Color.black;
            _vignette.color.overrideState = true;
        }
        if (profile.TryGet(out _filmGrain))
        {
            _filmGrain.intensity.overrideState = true;
            _filmGrain.intensity.value = filmGrainIntensity;
            _filmGrain.type.value = FilmGrainLookup.Thin1;
            _filmGrain.type.overrideState = true;
        }
        if (profile.TryGet(out _colorAdj))
        {
            _colorAdj.saturation.overrideState = true;
            _colorAdj.saturation.value = saturation;
            // Tidak pakai colorFilter — biar warna material asli tetap keliatan
        }
        if (profile.TryGet(out _bloom))
        {
            _bloom.intensity.overrideState = true;
            _bloom.intensity.value = bloomIntensity;
            _bloom.threshold.overrideState = true;
            _bloom.threshold.value = 1.1f; // hanya area sangat terang yang bloom
        }
    }

    void Update()
    {
        if (directionalLight != null) directionalLight.intensity = nightIntensity;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogColor = fogColor;
        RenderSettings.ambientLight = Color.white * ambientIntensity;

        if (_vignette != null) _vignette.intensity.value = vignetteIntensity;
        if (_filmGrain != null) _filmGrain.intensity.value = filmGrainIntensity;
        if (_colorAdj != null) _colorAdj.saturation.value = saturation;
        if (_bloom != null) _bloom.intensity.value = bloomIntensity;
    }

    IEnumerator LightningRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(lightningIntervalMin, lightningIntervalMax));
            yield return StartCoroutine(FlashLightning());

            // Kadang double flash — lebih natural
            if (Random.value > 0.6f)
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
                yield return StartCoroutine(FlashLightning());
            }
        }
    }

    IEnumerator FlashLightning()
    {
        float intensity = Random.Range(lightningIntensityMin, lightningIntensityMax);
        lightningLight.intensity = intensity;
        yield return new WaitForSeconds(lightningDuration);
        lightningLight.intensity = 0f;
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            flickerLight.intensity = Random.Range(flickerIntensityMin, flickerIntensityMax);
            yield return new WaitForSeconds(flickerSpeed);
        }
    }
}
