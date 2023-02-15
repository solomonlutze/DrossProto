using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class FoodItemData : ItemData
{
  public override InventoryItemType type
  {
    get { return InventoryItemType.Food; }
    set { }
  }
#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create a FoodItem Asset
        [MenuItem("Assets/Create/Item/FoodItem")]
        public static void CreateFoodItem()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Food Item", "New Food Item", "Asset", "Save Food Item", "Assets/resources/Data/ItemData/Food");
            if (path == "")
                return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<FoodItemData>(), path);
        }
#endif
}
