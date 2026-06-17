using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public static class SetupFirstPersonScene
{
    [MenuItem("Tools/Setup First Person Scene")]
    static void Setup()
    {
        // --- Ground plane ---
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10);

        // --- Player ---
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0, 1, 0);

        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.center = new Vector3(0, 0, 0);

        var pi = player.AddComponent<PlayerInput>();

        var controller = player.AddComponent<FirstPersonController>();

        // --- Camera holder (untuk vertical look) ---
        var camHolder = new GameObject("CameraHolder");
        camHolder.transform.SetParent(player.transform);
        camHolder.transform.localPosition = new Vector3(0, 0.8f, 0);

        // --- Main Camera ---
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(camHolder.transform);
        camGO.transform.localPosition = Vector3.zero;
        camGO.transform.localRotation = Quaternion.identity;
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();

        // Assign camera holder ke script
        controller.cameraHolder = camHolder.transform;

        // Assign Input Actions asset
        var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
            "Assets/InputSystem_Actions.inputactions");
        if (inputActions != null)
        {
            pi.actions = inputActions;
            pi.defaultActionMap = "Player";
            pi.notificationBehavior = PlayerNotifications.SendMessages;
        }
        else
        {
            Debug.LogWarning("InputSystem_Actions.inputactions not found — assign manually.");
        }

        // Hapus Main Camera default kalau ada
        var oldCam = GameObject.FindWithTag("MainCamera");
        if (oldCam != null && oldCam != camGO)
            Object.DestroyImmediate(oldCam);

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Selection.activeGameObject = player;
        Debug.Log("First Person setup selesai! Tekan Play untuk test.");
    }
}
