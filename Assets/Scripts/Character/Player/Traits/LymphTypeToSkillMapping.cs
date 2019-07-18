using UnityEngine;
using UnityEditor;

[System.Serializable]
public class LymphTypeToSkillMapping : ScriptableObject {

  // [SerializeField]
  // public LymphTypeToLymphTypeSkillsDictionary skillMapping;
  #if UNITY_EDITOR
    // Should only need this once! should only need one of these!!
    [MenuItem("Assets/Create/LymphTypeToSkillMapping")]
    public static void CreateLymphTypeToSkillMapping()
    {
      string path = EditorUtility.SaveFilePanelInProject("Save Lymph Type To Skill Mapping", "New Lymph Type To Skill Mapping", "Asset", "Save Lymph Type To Skill Mapping", "Assets/resources/Data/TraitData");
      if (path == "")
        return;
      AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LymphTypeToSkillMapping>(), path);
    }
  #endif
}
