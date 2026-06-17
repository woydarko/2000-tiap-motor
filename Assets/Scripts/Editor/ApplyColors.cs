using UnityEngine;
using UnityEditor;

public static class ApplyColors
{
    [MenuItem("Tools/Apply Scene Colors")]
    static void Apply()
    {
        SetColor("wall",   new Color(0.96f, 0.94f, 0.90f)); // putih tulang
        SetColor("street", new Color(0.45f, 0.45f, 0.45f)); // abu-abu
        SetColor("Ground", new Color(0.55f, 0.35f, 0.18f)); // coklat

        Debug.Log("Colors applied!");
    }

    static void SetColor(string goName, Color color)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"{goName} not found"); return; }

        var renderer = go.GetComponent<Renderer>();
        if (renderer == null) { Debug.LogWarning($"{goName} has no Renderer"); return; }

        // Buat material baru dengan URP/Lit shader
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;

        // Save material ke Assets/Materials/
        AssetDatabase.CreateAsset(mat, $"Assets/Materials/Mat_{goName}.mat");
        renderer.sharedMaterial = mat;

        EditorUtility.SetDirty(go);
    }

    [MenuItem("Tools/Apply Scene Colors", true)]
    static bool Validate()
    {
        // Pastikan folder Materials ada
        System.IO.Directory.CreateDirectory(Application.dataPath + "/Materials");
        return true;
    }
}
