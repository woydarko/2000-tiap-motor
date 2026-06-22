using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class ListSceneObjects
{
    [MenuItem("Tools/List All Scene Objects")]
    static void ListObjects()
    {
        var scenes = new[]
        {
            "Assets/Scenes/in-game.unity",
            "Assets/_Recovery/0.unity"
        };

        foreach (var scenePath in scenes)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"=== Scene: {scenePath} ===");
            foreach (var go in scene.GetRootGameObjects())
                PrintHierarchy(go, 0);
        }
    }

    static void PrintHierarchy(GameObject go, int depth)
    {
        Debug.Log($"{new string('-', depth * 2)}{go.name}");
        foreach (Transform child in go.transform)
            PrintHierarchy(child.gameObject, depth + 1);
    }
}
