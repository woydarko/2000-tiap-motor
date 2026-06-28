using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Unity.AI.Navigation;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BakeNavMesh
{
    [MenuItem("Tools/Bake NavMesh (map baru)")]
    static void Bake()
    {
        var surface = Object.FindFirstObjectByType<NavMeshSurface>();
        if (surface == null)
        {
            Debug.LogError("NavMeshSurface tidak ditemukan di scene!");
            return;
        }

        // Ambil semua geometri di scene (dirt, street, wall dari FBX)
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;

        // Bake
        surface.BuildNavMesh();

        EditorUtility.SetDirty(surface);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("NavMesh selesai di-bake dari map baru (dirt/street walkable, wall jadi penghalang).");
    }
}
