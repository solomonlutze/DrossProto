using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class TextureData
{
    public Texture texture;
    public Vector2 scale = Vector2.one;
    public float intensityMin = 1.0f;
    public float intensityMax = 1.0f;
    public float opacity = 1.0f;
    public Vector2 scrollSpeed = Vector2.zero;

}

public class ShaderData : ScriptableObject {
    public TextureData red0TextureData;
    public TextureData red50TextureData;
    public TextureData red100TextureData;
    public TextureData red150TextureData;
    public TextureData green0TextureData;
    public TextureData green50TextureData;
    public TextureData green100TextureData;
    public TextureData green150TextureData;
    public TextureData blue0TextureData;
    public TextureData blue50TextureData;
    public TextureData blue100TextureData;
    public TextureData blue150TextureData;
    #if UNITY_EDITOR
    // The following is a helper that adds a menu item to create an ShaderData Asset
    [MenuItem("Assets/Create/Art/ShaderData")]
    public static void CreateShaderData()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Shader Data", "New Shader Data", "Asset", "Save Shader Data", "Assets/resources/Data/ArtData/ShaderData");
        if (path == "")
            return;
      AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ShaderData>(), path);
    }
    #endif
}
