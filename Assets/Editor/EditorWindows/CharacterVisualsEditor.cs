using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterVisuals))]
public class CharacterVisualsEditor : Editor
{
  // possibly necessary to support property drawer?
  public override void OnInspectorGUI()
  {
    this.serializedObject.Update();

    CharacterVisuals data = target as CharacterVisuals;
    DrawDefaultInspector();
    if (GUILayout.Button("Set Images From Skeleton Data"))
    {
      string path = EditorUtility.OpenFilePanel("Select skeleton data", "Assets/resources/Data/ArtData/CharacterArt", "");
      Debug.Log("path: " + path);
      string modifiedPath = path.Substring((Application.dataPath + "/resources/").Length);
      modifiedPath = modifiedPath.Substring(0, modifiedPath.Length - (".Asset").Length);
      Debug.Log("relative path: " + modifiedPath);

      BugSkeletonImagesData imagesData = Resources.Load<BugSkeletonImagesData>(modifiedPath);
      data.SetCharacterVisualsFromSkeletonImagesData(imagesData);
      GUIUtility.ExitGUI();
    }
    if (GUILayout.Button("Set renderer layers"))
    {
      WorldObject.ChangeLayersRecursively(data.transform, data.gameObject.layer);
    }
    this.serializedObject.ApplyModifiedProperties();
  }


}
