using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


// View for equipping traits before respawn.
// Should show all available equips based on items picked up in previous life.
public class EquipTraitsView : MenuBase
{

  public TraitSlotToBugTraitInfoDictionary traitInfos;
  public TraitSlotToBugTraitInfoDictionary nextTraitInfos;
  public SkillInfo[] skillInfoGameObjects;
  public UIBug uiBug;
  Dictionary<TraitSlot, TraitButton> traitButtons;
  public Transform traitItemButtonsContainer;
  public Transform traitInfosContainer;
  public TraitItemButton traitItemButtonPrefab;
  List<TraitSlotToTraitDictionary> traitItems;
  TraitSlotToTraitDictionary nextTraits;
  bool inItemMenu = false;
  int buttonCount = 5;
  public void Awake()
  {
    for (int i = 0; i < buttonCount; i++)
    {
      Instantiate(traitItemButtonPrefab, traitItemButtonsContainer);
    }
  }

  public void Init(
    TraitSlotToTraitDictionary cachedPupa,
    List<TraitSlotToTraitDictionary> ti
    )
  {
    traitItems = ti;
    foreach (TraitSlot slot in cachedPupa.Keys)
    {
      traitInfos[slot].Init(cachedPupa[slot], slot);
    }
    nextTraits = new TraitSlotToTraitDictionary(cachedPupa);
    uiBug.Init(cachedPupa);
  }

  public void OnTraitButtonClicked(TraitSlot slot)
  {
    EnableTraitItemMenu();
  }

  public void OnConfirmButtonClicked()
  {
    GameMaster.Instance.ConfirmEquipAndRespawn(nextTraits);
  }

  void EnableTraitItemMenu()
  {
    inItemMenu = true;
    for (int i = 0; i < traitItemButtonsContainer.childCount; i++)
    {
      traitItemButtonsContainer.GetChild(i).GetComponent<Button>().interactable = true;
    }
    for (int i = 0; i < traitInfosContainer.childCount; i++)
    {
      traitInfosContainer.GetChild(i).GetComponent<Button>().interactable = false;
    }
    EventSystem.current.SetSelectedGameObject(traitItemButtonsContainer.GetChild(0).gameObject);
  }

  void DisableTraitItemMenu()
  {
    inItemMenu = false;
    for (int i = 0; i < traitItemButtonsContainer.childCount; i++)
    {
      traitItemButtonsContainer.GetChild(i).GetComponent<Button>().interactable = false;
    }
    for (int i = 0; i < traitInfosContainer.childCount; i++)
    {
      traitInfosContainer.GetChild(i).GetComponent<Button>().interactable = true;
      EventSystem.current.SetSelectedGameObject(traitInfosContainer.GetChild(0).gameObject);
    }
  }

  public void OnTraitItemClicked(Trait trait, TraitSlot slot)
  {
    if (trait != null)
    {
      nextTraits[slot] = trait;
    }
    else
    {
      nextTraits[slot] = GameMaster.Instance.cachedPupa[slot];
    }
    uiBug.Init(nextTraits); // probably overkill but w/e
    DisableTraitItemMenu();
  }

  public void ShowItemsForSlot(TraitSlot slot)
  {
    List<TraitSlotToTraitDictionary> traitItemsForSlot = new List<TraitSlotToTraitDictionary>();
    foreach (TraitSlotToTraitDictionary item in traitItems)
    {
      if (item[slot] != null)
      {
        traitItemsForSlot.Add(item);
      }
    }
    for (int i = 1; i < traitItemButtonsContainer.childCount; i++)
    {
      traitItemButtonsContainer.GetChild(i).GetComponent<Button>().interactable = false;
      if (i > traitItemsForSlot.Count)
      {
        traitItemButtonsContainer.GetChild(i).gameObject.SetActive(false);
      }
      else
      {
        traitItemButtonsContainer.GetChild(i).gameObject.SetActive(true);
        traitItemButtonsContainer.GetChild(i).GetComponent<TraitItemButton>().Init(traitItemsForSlot[i - 1][slot], slot, this);
      }
    }
    traitItemButtonsContainer.GetChild(0).GetComponent<TraitItemButton>().Init(null, slot, this);
  }

  public void OnTraitButtonSelected(TraitSlot slot)
  {
    if (inItemMenu) { return; }
    ShowItemsForSlot(slot);
  }
  public void OnTraitButtonDeselected()
  {
    if (inItemMenu) { return; }
    uiBug.UnhighlightSlot();
  }

  public void OnTraitItemButtonSelected(Trait trait, TraitSlot slot)
  {
    if (!inItemMenu) { return; }
    if (trait != null)
    {
      uiBug.HighlightSlot(slot, trait);
      traitInfos[slot].Init(trait, slot, trait.traitName + " " + slot);
    }
    else
    {
      uiBug.HighlightSlot(slot, GameMaster.Instance.cachedPupa[slot]);
      traitInfos[slot].Init(GameMaster.Instance.cachedPupa[slot], slot);
    }
  }
}
