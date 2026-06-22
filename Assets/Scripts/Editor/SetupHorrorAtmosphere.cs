using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class SetupHorrorAtmosphere
{
    [MenuItem("Tools/Setup Horror Atmosphere")]
    static void Setup()
    {
        // --- Directional Light ---
        var dirLight = Object.FindFirstObjectByType<Light>();
        if (dirLight != null && dirLight.type == LightType.Directional)
        {
            dirLight.intensity = 0.05f;
            dirLight.color = new Color(0.4f, 0.5f, 0.7f);
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // --- Ambient & Fog ---
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.02f, 0.03f, 0.06f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.08f);
        RenderSettings.fogDensity = 0.04f;

        // --- Lightning Light ---
        GameObject lightningGO = new GameObject("LightningLight");
        Light lightningLight = lightningGO.AddComponent<Light>();
        lightningLight.type = LightType.Directional;
        lightningLight.intensity = 0f;
        lightningLight.color = new Color(0.85f, 0.9f, 1f);
        lightningLight.shadows = LightShadows.None;
        lightningGO.transform.rotation = Quaternion.Euler(60f, 20f, 0f);

        // --- Flicker Point Light ---
        GameObject flickerGO = new GameObject("FlickerLamp");
        Light flickerLight = flickerGO.AddComponent<Light>();
        flickerLight.type = LightType.Point;
        flickerLight.intensity = 0.8f;
        flickerLight.range = 12f;
        flickerLight.color = new Color(1f, 0.85f, 0.5f);
        // Taruh di tengah area parkir
        var ground = GameObject.Find("Ground");
        flickerGO.transform.position = ground != null
            ? ground.transform.position + Vector3.up * 4f
            : new Vector3(0f, 4f, 0f);

        // --- Post Processing Volume ---
        GameObject volumeGO = GameObject.Find("HorrorVolume");
        if (volumeGO == null) volumeGO = new GameObject("HorrorVolume");
        var volume = volumeGO.GetComponent<Volume>() ?? volumeGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, "Assets/HorrorVolumeProfile.asset");

        var vignette = profile.Add<Vignette>();
        vignette.intensity.value = 0.45f;
        vignette.intensity.overrideState = true;

        var grain = profile.Add<FilmGrain>();
        grain.intensity.value = 0.35f;
        grain.intensity.overrideState = true;

        var colorAdj = profile.Add<ColorAdjustments>();
        colorAdj.saturation.value = -30f;
        colorAdj.saturation.overrideState = true;
        colorAdj.colorFilter.value = new Color(0.9f, 0.93f, 1f);
        colorAdj.colorFilter.overrideState = true;

        var bloom = profile.Add<Bloom>();
        bloom.intensity.value = 0.3f;
        bloom.intensity.overrideState = true;
        bloom.threshold.value = 0.9f;
        bloom.threshold.overrideState = true;

        volume.profile = profile;
        AssetDatabase.SaveAssets();

        // --- HorrorAtmosphere controller ---
        GameObject atmosGO = new GameObject("HorrorAtmosphere");
        var atmos = atmosGO.AddComponent<HorrorAtmosphere>();
        atmos.directionalLight = dirLight;
        atmos.lightningLight = lightningLight;
        atmos.flickerLight = flickerLight;
        atmos.postProcessVolume = volume;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Horror Atmosphere setup selesai! Adjust di Inspector HorrorAtmosphere.");
    }
}
