using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BugSkeletonImagesData))]
public class BugSkeletonImagesDataEditor : Editor
{
  // possibly necessary to support property drawer?
  public override void OnInspectorGUI()
  {
    this.serializedObject.Update();

    BugSkeletonImagesData data = target as BugSkeletonImagesData;
    DrawDefaultInspector();
    // DrawTraitSelect();
    if (GUILayout.Button("Assign from folder"))
    {
      string path = EditorUtility.OpenFolderPanel("Select sprites folder", "Assets/resources/Art/Characters/Sprites", "");
      Debug.Log("path: " + path);
      data.LoadAndAssignSprites(path);
      GUIUtility.ExitGUI();
    }
    this.serializedObject.ApplyModifiedProperties();
  }


}
