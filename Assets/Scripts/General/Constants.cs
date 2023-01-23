using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class DrossConstants
{
  public enum GameState { Play = 0, Dead = 1, Dialogue = 2, Menu = 3, ChooseBug = 4, Pause = 5, EquipTraits = 6 }
  public const float DEFAULT_DETECTION_RANGE = 20f;
  public const float CARAPACE_DAMAGE_REDUCTION = 1f;
  public const float CARAPACE_BREAK_STUN_DURATION = 5.0f;
  public const float BROKEN_PART_MOVE_DISTANCE_MULTIPLIER = .75f;
  public const float BROKEN_PART_EFFECT_DURATION_MULTIPLIER = 1.5f;
  public const float STUN_DAMAGE_MULTIPLIER = 3.0f;
  public static int numberOfFloorLayers = Enum.GetNames(typeof(FloorLayer)).Length;
}

// public class DrossConstantsData : ScriptableObject
// {
//   public enum GameState { Play = 0, Dead = 1, Dialogue = 2, Menu = 3, ChooseBug = 4, Pause = 5, EquipTraits = 6 }
//   public const float DEFAULT_DETECTION_RANGE = 20f;
//   public const float CARAPACE_DAMAGE_REDUCTION = 1f;
//   public const float CARAPACE_BREAK_STUN_DURATION = 5.0f;
//   public const float BROKEN_PART_EFFECT_DURATION_MULTIPLIER = 1.5f;
//   public const float STUN_DAMAGE_MULTIPLIER = 3.0f;
//   public static int numberOfFloorLayers = Enum.GetNames(typeof(FloorLayer)).Length;
// #if UNITY_EDITOR
//   [MenuItem("Assets/Create/DrossConstantsData")]
//   public static void CreateDrossConstantsData()
//   {
//     string path = EditorUtility.SaveFilePanelInProject("Save Dross Constants Data", "New Character Skill Data", "Asset", "Save Character Skill Data", "Assets/resources/Data/CharacterData/Skills/CharacterSkillData");
//     if (path == "")
//       return;
//     AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DrossConstantsData>(), path);
//   }
// #endif
// }
