using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(AiStateController))]
[CanEditMultipleObjects]
public class AiStateControllerEditor : CharacterEditor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AiStateController character = target as AiStateController;
        if (!character.traits.Values.Any(pair => pair != null))
        {

            if (GUILayout.Button("Generate and assign lymph"))
            {
                // string guid = AssetDatabase.CreateFolder("Assets/resources/Data/TraitData/Traits", character.gameObject.name);
                // string newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
                // Debug.Log("new folder path: " + newFolderPath);

                foreach (TraitSlot slot in Enum.GetValues(typeof(TraitSlot)))
                {
                    character.traits[slot] = Trait.CreateTraitForBug(character, slot);
                }
            }
        }
        // environmentTile.changesFloorLayer = GUILayout.Toggle(environmentTile.changesFloorLayer, "Changes Floor Layer");
        // if (environmentTile.changesFloorLayer)
        // {
        //     int oldChanges = environmentTile.changesFloorLayerByAmount;
        //     environmentTile.changesFloorLayerByAmount = EditorGUILayout.Popup(
        //       "Changes floor layer by amount",
        //       environmentTile.changesFloorLayerByAmount + 6,
        //       new string[] { "-6", "-5", "-4", "-3", "-2", "-1", "0", "1", "2", "3", "4", "5", "6" }
        //     ) - 6;
        // }

        EditorUtility.SetDirty(character);
    }
}