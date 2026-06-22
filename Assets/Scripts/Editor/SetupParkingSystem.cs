using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class SetupParkingSystem
{
    [MenuItem("Tools/Setup Parking System")]
    static void Setup()
    {
        // --- MoneySystem singleton ---
        var moneySys = new GameObject("MoneySystem");
        moneySys.AddComponent<MoneySystem>();

        // --- ParkingSpot components ke spot objects ---
        string[] spotNames = { "spot 1", "spot 2", "spot 3" };
        var spots = new ParkingSpot[3];
        for (int i = 0; i < spotNames.Length; i++)
        {
            var go = GameObject.Find(spotNames[i]);
            if (go == null) { Debug.LogError($"{spotNames[i]} not found!"); return; }
            spots[i] = go.GetComponent<ParkingSpot>() ?? go.AddComponent<ParkingSpot>();
        }

        // --- Spawn points (kiri & kanan street) ---
        var streetGO = GameObject.Find("street");
        Vector3 streetPos = streetGO != null ? streetGO.transform.position : Vector3.zero;
        Vector3 streetScale = streetGO != null ? streetGO.transform.localScale : Vector3.one;

        float halfLen = streetScale.z * 5f; // plane default size 10, scale * 10 / 2

        var spawnLeftGO  = new GameObject("SpawnLeft");
        var spawnRightGO = new GameObject("SpawnRight");
        spawnLeftGO.transform.position  = streetPos + new Vector3(0, 0, -halfLen);
        spawnRightGO.transform.position = streetPos + new Vector3(0, 0,  halfLen);

        // --- ParkingManager ---
        var managerGO = new GameObject("ParkingManager");
        var manager = managerGO.AddComponent<ParkingManager>();
        manager.spots = spots;
        manager.spawnPoint1 = spawnLeftGO.transform;
        manager.spawnPoint2 = spawnRightGO.transform;
        manager.spawnInterval = 15f;

        // Set prefab dari object yang sudah ada di scene
        var motorGO = GameObject.Find("motor");
        var npcGO   = GameObject.Find("npc");

        if (motorGO != null)
        {
            // Jadikan prefab (simpan ke Assets)
            string motorPath = "Assets/Prefabs/motor.prefab";
            string npcPath   = "Assets/Prefabs/npc.prefab";
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");

            var motorPrefab = PrefabUtility.SaveAsPrefabAsset(motorGO, motorPath);
            var npcPrefab   = PrefabUtility.SaveAsPrefabAsset(npcGO,   npcPath);

            manager.motorPrefab = motorPrefab;
            manager.npcPrefab   = npcPrefab;

            // Tambah NpcController ke npc prefab jika belum ada
            if (npcPrefab.GetComponent<NpcController>() == null)
                npcPrefab.AddComponent<NpcController>();

            // Nonaktifkan original di scene (prefab sudah dibuat)
            motorGO.SetActive(false);
            npcGO.SetActive(false);
        }
        else
        {
            Debug.LogWarning("motor / npc object tidak ditemukan di scene!");
        }

        // --- PlayerInteract ke Player ---
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var pi = player.GetComponent<PlayerInteract>() ?? player.AddComponent<PlayerInteract>();
        }

        // --- UI Canvas ---
        SetupUI(out var moneyUI);

        // Link moneyUI ke PlayerInteract
        if (player != null)
        {
            var pi = player.GetComponent<PlayerInteract>();
            if (pi != null) pi.moneyUI = moneyUI;
        }

        EditorUtility.SetDirty(managerGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("Parking System setup selesai!");
    }

    static void SetupUI(out MoneyUI moneyUI)
    {
        // Canvas
        var canvasGO = new GameObject("ParkingUI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        moneyUI = canvasGO.AddComponent<MoneyUI>();

        // Total money text (pojok kiri atas)
        var moneyTextGO = new GameObject("MoneyText");
        moneyTextGO.transform.SetParent(canvasGO.transform, false);
        var moneyTMP = moneyTextGO.AddComponent<TextMeshProUGUI>();
        moneyTMP.text = "Rp 0";
        moneyTMP.fontSize = 28;
        moneyTMP.color = Color.white;
        var moneyRect = moneyTextGO.GetComponent<RectTransform>();
        moneyRect.anchorMin = new Vector2(0, 1);
        moneyRect.anchorMax = new Vector2(0, 1);
        moneyRect.pivot     = new Vector2(0, 1);
        moneyRect.anchoredPosition = new Vector2(20, -20);
        moneyRect.sizeDelta = new Vector2(250, 50);
        moneyUI.totalMoneyText = moneyTMP;

        // Collect popup (tengah layar)
        var popupGO = new GameObject("CollectPopup");
        popupGO.transform.SetParent(canvasGO.transform, false);
        var popupTMP = popupGO.AddComponent<TextMeshProUGUI>();
        popupTMP.text = "+Rp 2.000";
        popupTMP.fontSize = 36;
        popupTMP.color = Color.yellow;
        popupTMP.alignment = TextAlignmentOptions.Center;
        var popupRect = popupGO.GetComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.4f);
        popupRect.anchorMax = new Vector2(0.5f, 0.4f);
        popupRect.sizeDelta = new Vector2(300, 60);
        popupRect.anchoredPosition = Vector2.zero;
        popupGO.SetActive(false);
        moneyUI.collectPopupText = popupTMP;

        // Interact prompt (tengah bawah)
        var promptGO = new GameObject("InteractPrompt");
        promptGO.transform.SetParent(canvasGO.transform, false);
        var bg = promptGO.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.6f);
        var promptRect = promptGO.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.5f, 0);
        promptRect.anchorMax = new Vector2(0.5f, 0);
        promptRect.pivot = new Vector2(0.5f, 0);
        promptRect.anchoredPosition = new Vector2(0, 80);
        promptRect.sizeDelta = new Vector2(280, 45);

        var promptTextGO = new GameObject("PromptText");
        promptTextGO.transform.SetParent(promptGO.transform, false);
        var promptTMP = promptTextGO.AddComponent<TextMeshProUGUI>();
        promptTMP.text = "[E] Tagih Parkir - Rp 2.000";
        promptTMP.fontSize = 18;
        promptTMP.color = Color.white;
        promptTMP.alignment = TextAlignmentOptions.Center;
        var ptRect = promptTextGO.GetComponent<RectTransform>();
        ptRect.anchorMin = Vector2.zero;
        ptRect.anchorMax = Vector2.one;
        ptRect.offsetMin = ptRect.offsetMax = Vector2.zero;

        promptGO.SetActive(false);
        moneyUI.interactPrompt = promptGO;
        moneyUI.interactPromptText = promptTMP;
    }
}
