using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ConsumableItemData : ItemData {

    public CharacterStatModification[] ownStatModifications;
	public override InventoryItemType type {
		get { return InventoryItemType.Consumable; }
		set { }
	}

	public void Use(Character user) {
        foreach (CharacterStatModification mod in ownStatModifications) {
            user.ModCharacterStat(mod);
        }
	}

	#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a ConsumableItem Asset
        [MenuItem("Assets/Create/Item/ConsumableItem")]
        public static void CreateConsumableItem()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Consumable Item", "New Consumable Item", "Asset", "Save Consumable Item", "Assets/resources/Data/ItemData/Consumable");
            if (path == "")
                return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ConsumableItemData>(), path);
        }
    #endif
}
