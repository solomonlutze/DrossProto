using UnityEngine;
using UnityEditor;

public class BugSpeciesToAnimationDataMap : ScriptableObject
{
  public BugSpeciesToBugAnimationDictionary bugSpeciesToAnimationData;
#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create a BugSpeciesToAnimationData Asset
  [MenuItem("Assets/Create/BugSpeciesToAnimationData")]
  public static void CreateBugSpeciesToAnimationData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save BugAnimationSpeciesToAnimationData", "New BugSpeciesToAnimationData", "Asset", "Save BugSpeciesToAnimationData", "Assets/resources/Prefabs/Characters/Animation/BugSpeciesToAnimationData");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BugSpeciesToAnimationDataMap>(), path);
  }
#endif
}