using UnityEngine;
using UnityEditor;

public enum AnimationName
{
  Idle,
  Walk,
  Dash,
  Attack,
  Block,
  Fly
}
public class BugAnimationData : ScriptableObject
{
  public AnimationNameToAnimationClipDictionary animationStateNamesToClips;


#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create a BugAnimationData Asset
  [MenuItem("Assets/Create/BugAnimationData")]
  public static void CreateBugAnimationData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Bug Animation Data", "New Bug Animation Data", "Asset", "Save Bug Animation Data", "Assets/resources/Prefabs/Characters/Animation/BugAnimationData");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BugAnimationData>(), path);
  }
#endif
}