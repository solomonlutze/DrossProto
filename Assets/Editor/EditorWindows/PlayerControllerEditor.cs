using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(PlayerController))]
[CanEditMultipleObjects]
public class PlayerControllerEditor : CharacterEditor
{
    override public void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlayerController character = target as PlayerController;

        if (GUILayout.Button("Refresh character visuals"))
        {
            character.characterVisuals.SetCharacterVisuals(character.traits);
        }
        EditorUtility.SetDirty(character);
    }
}