using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class RenameSpots
{
    [MenuItem("Tools/Rename Spots Sequentially")]
    static void Rename()
    {
        var parent = GameObject.Find("spot parkir");
        if (parent == null)
        {
            Debug.LogError("Object 'spot parkir' tidak ditemukan!");
            return;
        }

        int n = 1;
        // Urut sesuai urutan di Hierarchy (sibling index)
        foreach (Transform child in parent.transform)
        {
            if (child.GetComponent<ParkingSpot>() != null)
            {
                child.name = $"spot {n}";
                n++;
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"Rename selesai: {n - 1} spot jadi 'spot 1' ... 'spot {n - 1}'.");
    }
}
