using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BuildNpcPrefab
{
    // Cari object di scene termasuk yang nonaktif
    static GameObject FindInScene(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name != name) continue;
            if (go.scene.IsValid() && go.hideFlags == HideFlags.None) return go;
        }
        return null;
    }

    [MenuItem("Tools/Build NPC Prefab")]
    static void Build()
    {
        var npcRoot = FindInScene("NPC");
        if (npcRoot == null)
        {
            Debug.LogError("Object 'NPC' (parent) tidak ditemukan di scene!");
            return;
        }
        npcRoot.SetActive(true); // pastikan aktif biar prefab tersimpan aktif

        // NpcController di ROOT 'NPC' (yang bergerak)
        var ctrl = npcRoot.GetComponent<NpcController>();
        if (ctrl == null) ctrl = npcRoot.AddComponent<NpcController>();

        // seatOffset TIDAK diubah — pakai nilai yang sudah di-set di Inspector
        // (biar posisi tidak berubah tiap bake)

        // Simpan sebagai prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        string path = "Assets/Prefabs/Npc.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(npcRoot, path);
        Debug.Log($"NPC prefab tersimpan: {path}");

        // Wire ke ParkingManager.npcPrefab
        var pm = Object.FindFirstObjectByType<ParkingManager>();
        if (pm != null)
        {
            pm.npcPrefab = prefab;
            EditorUtility.SetDirty(pm);
            Debug.Log("ParkingManager.npcPrefab → NPC prefab (kotak susu)");
        }
        else Debug.LogWarning("ParkingManager tidak ditemukan!");

        // Nonaktifkan NPC asli di scene
        npcRoot.SetActive(false);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Selesai. Kotak susu sekarang jadi NPC utama.");
    }
}
