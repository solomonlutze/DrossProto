using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonoItem : MonoBehaviour {
    abstract public InventoryItemType type { get; set; }
    public string itemName;
    public Character owner;
}


public abstract class ItemData : ScriptableObject {
    abstract public InventoryItemType type { get; set; }
    public string itemName;
    public Character owner;
}