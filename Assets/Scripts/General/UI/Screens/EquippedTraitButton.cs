using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedTraitButton : MonoBehaviour
{
  public Button button;
  public SuperTextMesh slotNameLabel;
  public SuperTextMesh nameLabel;
  private InventoryScreen inventoryScreen;
  private TraitSlot slot;

  public Color defaultColor;
  void Start()
  {
    if (button != null)
    {
      button.onClick.AddListener(HandleClick);
    }
    GetComponent<Image>().color = defaultColor;
  }

  public virtual void Init(UpcomingLifeTrait upcomingTrait, InventoryScreen parentScreen, TraitSlot ts)
  {
    slot = ts;
    InventoryEntry ie = upcomingTrait.inventoryItem;
    slotNameLabel.text = ts.ToString();
    string buttonText = "";
    if (upcomingTrait != null && upcomingTrait.trait != null)
    {
      buttonText += upcomingTrait.trait.traitName + "\n";
      string itemText = "no item";
      if (upcomingTrait.inventoryItem != null && !string.IsNullOrEmpty(upcomingTrait.inventoryItem.itemName))
      {
        itemText = upcomingTrait.inventoryItem.itemName;
      }
      buttonText += "(" + itemText + ")\n";
    }
    else
    {
      buttonText += "(Assign Trait)";
    }
    nameLabel.text = buttonText;
    this.inventoryScreen = parentScreen;
  }

  protected virtual void HandleClick()
  {
    inventoryScreen.OpenEquipTraitMenu(slot);
  }
}
