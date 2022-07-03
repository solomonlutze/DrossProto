using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

  public Button button;
  // public TextMeshProUGUI nameLabel;
  protected InventoryEntry item;
  protected InventoryScreen inventoryScreen;
  protected int? slotNumber;

  // Use this for initialization
  public Color defaultColor;
  public Color equippedColor;
  void Start()
  {
    button.onClick.AddListener(HandleClick);
    GetComponent<Image>().color = defaultColor;
  }

  void Update()
  {
    if (item.equipped)
    {
      GetComponent<Image>().color = equippedColor;
    }
    else
    {
      GetComponent<Image>().color = defaultColor;
    }
  }

  public void Init(InventoryEntry itemEntryInfo, InventoryScreen parentScreen, int slot = -1)
  {
    item = itemEntryInfo;
    slotNumber = slot;
    // nameLabel.text = itemEntryInfo.itemName;
    inventoryScreen = parentScreen;
  }

  protected virtual void HandleClick()
  {
    inventoryScreen.EquipItem(item, slotNumber);
  }

  public virtual void OnPointerEnter(PointerEventData data)
  {
    return;
  }

  public virtual void OnPointerExit(PointerEventData data)
  {
    return;
  }
}
