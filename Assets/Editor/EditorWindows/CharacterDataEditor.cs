using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterData))]
public class CharacterDataEditor : Editor
{
  // possibly necessary to support property drawer?
  public override void OnInspectorGUI()
  {
    this.serializedObject.Update();

    CharacterData data = target as CharacterData;
    foreach (CharacterStat stat in data.defaultStats.Keys)
    {
      Debug.Log("stat: " + stat);
    }
    DrawDefaultInspector();
    // DrawTraitSelect();
    if (GUILayout.Button("Refresh attribute datas"))
    {
      // CharacterData data = target as CharacterData;
      data.RefreshAttributeDatas();
    }
    this.serializedObject.ApplyModifiedProperties();
  }


}
