using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class CurrencyItemData : ItemData {
	public override InventoryItemType type {
		get { return InventoryItemType.Currency; }
		set { }
	}
	#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a CurrencyItem Asset
        [MenuItem("Assets/Create/Item/CurrencyItem")]
        public static void CreateCurrencyItem()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Currency Item", "New Currency Item", "Asset", "Save Currency Item", "Assets/resources/Data/ItemData/Currency");
            if (path == "")
                return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CurrencyItemData>(), path);
        }
    #endif
}
