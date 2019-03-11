using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
// Don't instantiate TraitItems! Apply direct effects and generate objects etc as needed
// Don't extend TraitItem! Attach this script to a Trait Item prefab to indicate its properties. Create a new TraitItem prefab for each Trait Item that is collectable.
// TraitItem should be attached to 
[Serializable]
public class TraitItemData : ItemData { 

	public override InventoryItemType type {
		get { return InventoryItemType.Trait; }
		set { }
	}

	[StringInList(typeof(PropertyDrawerHelpers), "AllPassiveTraitNames")] public string passiveTrait;
	[StringInList(typeof(PropertyDrawerHelpers), "AllActiveTraitNames")] public string activeTrait;
	
	#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create an TraitItem Asset
        [MenuItem("Assets/Create/Item/TraitItem")]
        public static void CreateTraitItem()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Trait Item", "New Trait Item", "Asset", "Save Trait Item", "Assets/resources/Data/ItemData/Trait");
            if (path == "")
                return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TraitItemData>(), path);
        }
    #endif
}
