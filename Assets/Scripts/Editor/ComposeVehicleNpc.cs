using UnityEngine;
using UnityEditor;

public static class ComposeVehicleNpc
{
    // Langkah 1: tempel NPC ke dalam Vehicle biar bisa diatur visual bareng
    [MenuItem("Tools/Compose/1. Tempel NPC ke Vehicle")]
    static void Attach()
    {
        var vehicle = GameObject.Find("Vehicle");
        var npc = GameObject.Find("NPC");
        if (vehicle == null || npc == null)
        {
            Debug.LogError("Butuh object 'Vehicle' dan 'NPC' aktif di scene!");
            return;
        }

        // Ambil seatOffset existing (kalau ada) sebagai posisi awal
        var ctrl = npc.GetComponent<NpcController>();
        Vector3 startLocal = ctrl != null ? ctrl.seatOffset : Vector3.zero;

        npc.transform.SetParent(vehicle.transform);
        npc.transform.localPosition = startLocal;
        npc.transform.localRotation = Quaternion.identity;

        Selection.activeGameObject = npc;
        Debug.Log("NPC ditempel ke Vehicle. Geser/atur NPC di Scene view sampai kotak susu pas di dalam trolley. Lalu jalankan '2. Simpan Seat Offset'.");
    }

    // Langkah 2: baca posisi lokal NPC -> simpan ke seatOffset, lalu lepas lagi
    [MenuItem("Tools/Compose/2. Simpan Seat Offset & Lepas")]
    static void Capture()
    {
        var vehicle = GameObject.Find("Vehicle");
        var npc = GameObject.Find("NPC");
        if (vehicle == null || npc == null) { Debug.LogError("Vehicle/NPC tidak ada!"); return; }

        if (npc.transform.parent != vehicle.transform)
        {
            Debug.LogWarning("NPC belum jadi child Vehicle. Jalankan langkah 1 dulu.");
            return;
        }

        var ctrl = npc.GetComponent<NpcController>();
        if (ctrl == null) ctrl = npc.AddComponent<NpcController>();

        ctrl.seatOffset = npc.transform.localPosition;
        EditorUtility.SetDirty(ctrl);

        // Lepas NPC balik ke root
        npc.transform.SetParent(null);

        Debug.Log($"Seat Offset tersimpan = {ctrl.seatOffset}. NPC dilepas balik ke root. Sekarang jalankan 'Build Vehicle Prefab' dan 'Build NPC Prefab'.");
    }
}
