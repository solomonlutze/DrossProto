using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(EnvironmentTile))]
public class EnvironmentTileEditor : Editor
{
  private SerializedProperty m_Color;
  private SerializedProperty m_ColliderType;

  private RandomTile tile { get { return (target as RandomTile); } }
  /// <summary>
  /// OnEnable for RandomTile.
  /// </summary>
  public void OnEnable()
  {
    m_Color = serializedObject.FindProperty("m_Color");
    m_ColliderType = serializedObject.FindProperty("m_ColliderType");
  }
  override public void OnInspectorGUI()
  {
    serializedObject.Update();
    DrawDefaultInspector();
    EditorGUI.BeginChangeCheck();
    int count = EditorGUILayout.DelayedIntField("Number of Sprites", tile.m_Sprites != null ? tile.m_Sprites.Length : 0);
    if (count < 0)
      count = 0;
    if (tile.m_Sprites == null || tile.m_Sprites.Length != count)
    {
      Array.Resize<Sprite>(ref tile.m_Sprites, count);
    }

    if (count == 0)
      return;

    EditorGUILayout.LabelField("Place random sprites.");
    EditorGUILayout.Space();

    for (int i = 0; i < count; i++)
    {
      tile.m_Sprites[i] = (Sprite)EditorGUILayout.ObjectField("Sprite " + (i + 1), tile.m_Sprites[i], typeof(Sprite), false, null);
    }

    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(m_Color);
    EditorGUILayout.PropertyField(m_ColliderType);

    if (EditorGUI.EndChangeCheck())
    {
      EditorUtility.SetDirty(tile);
      serializedObject.ApplyModifiedProperties();
    }
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
