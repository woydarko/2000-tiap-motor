using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class ResetLighting
{
    [MenuItem("Tools/Reset Lighting to Default")]
    static void Reset()
    {
        // Directional light
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.intensity = 1f;
                l.color = Color.white;
            }
        }

        // Ambient
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.ambientLight = Color.white;

        // Fog off
        RenderSettings.fog = false;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Lighting reset to default.");
    }
}
