using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class WireAllSpots
{
    [MenuItem("Tools/Wire All Spots")]
    static void Wire()
    {
        // Kumpulkan semua ParkingSpot di scene (termasuk yang nonaktif)
        var spots = Object.FindObjectsByType<ParkingSpot>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (spots.Length == 0)
        {
            Debug.LogError("Tidak ada ParkingSpot di scene!");
            return;
        }

        var pm = Object.FindFirstObjectByType<ParkingManager>();
        if (pm == null)
        {
            Debug.LogError("ParkingManager tidak ditemukan!");
            return;
        }

        pm.spots = spots;
        EditorUtility.SetDirty(pm);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log($"Wired {spots.Length} spot ke ParkingManager.");
    }
}
