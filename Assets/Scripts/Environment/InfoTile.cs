using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif



[System.Serializable]
public class InfoTile : Tile
{
  public string areaName;
  public FloorTilemapType floorTilemapType = FloorTilemapType.Info;

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an EnvironmentTile Asset
  [MenuItem("Assets/Create/InfoTile")]
  public static void CreateInfoTile()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Info Tile", "NewInfoTile", "Asset", "Save Environment Tile", "resources/Art/Tiles/InfoTiles");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<InfoTile>(), path);
  }
#endif

}