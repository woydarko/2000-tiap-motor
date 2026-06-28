using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BuildVehiclePrefab
{
    static GameObject FindInScene(string name)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name != name) continue;
            if (go.scene.IsValid() && go.hideFlags == HideFlags.None) return go;
        }
        return null;
    }

    [MenuItem("Tools/Build Vehicle Prefab")]
    static void Build()
    {
        var vehicle = FindInScene("Vehicle");
        if (vehicle == null)
        {
            Debug.LogError("Object 'Vehicle' tidak ditemukan di scene!");
            return;
        }
        vehicle.SetActive(true); // pastikan aktif biar prefab tersimpan aktif

        // NavMeshAgent di PARENT (Vehicle root), bukan di mesh child
        var agent = vehicle.GetComponent<NavMeshAgent>();
        if (agent == null) agent = vehicle.AddComponent<NavMeshAgent>();
        agent.radius = 0.5f;
        agent.height = 1.8f;
        agent.baseOffset = 0f;
        agent.speed = 4f;
        agent.angularSpeed = 300f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.15f;
        agent.autoBraking = true;

        // Simpan sebagai prefab
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        string path = "Assets/Prefabs/Vehicle.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(vehicle, path);
        Debug.Log($"Vehicle prefab tersimpan: {path}");

        // Wire ke ParkingManager.motorPrefab
        var pm = Object.FindFirstObjectByType<ParkingManager>();
        if (pm != null)
        {
            pm.motorPrefab = prefab;
            EditorUtility.SetDirty(pm);
            Debug.Log("ParkingManager.motorPrefab → Vehicle prefab");
        }
        else Debug.LogWarning("ParkingManager tidak ditemukan!");

        // Nonaktifkan Vehicle asli di scene (biar cuma clone yang muncul saat play)
        vehicle.SetActive(false);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Selesai. Trolley sekarang jadi kendaraan utama.");
    }
}
