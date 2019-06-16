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
     var environmentTile = target as EnvironmentTile;

     environmentTile.changesFloorLayer = GUILayout.Toggle(environmentTile.changesFloorLayer, "Changes Floor Layer");

     if(environmentTile.changesFloorLayer) {
       environmentTile.targetFloorLayer = (FloorLayer) EditorGUILayout.EnumPopup(environmentTile.targetFloorLayer);
     }
   }
 }