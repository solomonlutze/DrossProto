using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EnvironmentTile))]
public class EnvironmentTileEditor : Editor
{
  override public void OnInspectorGUI()
  {
    DrawDefaultInspector();
    EnvironmentTile environmentTile = target as EnvironmentTile;
    environmentTile.changesFloorLayer_old = GUILayout.Toggle(environmentTile.changesFloorLayer_old, "Changes Floor Layer");
    if (environmentTile.changesFloorLayer_old)
    {
      int oldChanges = environmentTile.changesFloorLayerByAmount;
      environmentTile.changesFloorLayerByAmount = EditorGUILayout.Popup(
        "Changes floor layer by amount",
        environmentTile.changesFloorLayerByAmount + 6,
        new string[] { "-6", "-5", "-4", "-3", "-2", "-1", "0", "1", "2", "3", "4", "5", "6" }
      ) - 6;
    }

    EditorUtility.SetDirty(environmentTile);
  }
}