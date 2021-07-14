using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


// Should be populated with a set of attributes
// Has a reference to AttributeInfo prefab and AttributesInfoContents
// For each attribute, creates and inits an AttributesInfo, and parents it to AttributesInfoContent
public class StartingBugSelectScreen : MonoBehaviour
{

  public Transform selectStartingBugButtonContainer;
  // public Button confirmSelectStartingBugButton;
  public SelectStartingBugButton selectStartingBugButton;

  public SuperTextMesh descriptionText;
  public BugPresetData[] startingBugs;

  public CharacterVisuals_Old visuals;

  public int highlightedBug;

  public void Init()
  {
    Debug.Log("init");
    foreach (Transform child in selectStartingBugButtonContainer.transform)
    {
      Debug.Log("destroy child");
      Destroy(child.gameObject);
    }
    for (int i = 0; i < startingBugs.Length; i++)
    {
      SelectStartingBugButton btn = Instantiate(selectStartingBugButton, selectStartingBugButtonContainer);
      btn.parentScreen = this;
      btn.idx = i;
      btn.text.text = startingBugs[i].displayName;
    }
    // visuals.ClearCharacterVisuals();
  }
  public void OnEnable()
  {
    Debug.Log("enabled starting bug select screen");
    StartCoroutine(SelectFirstButton());
  }

  // Required bc the button cannot be selected the same frame it's created!!
  // Do not ask me about it!!
  // Do not delete it!!
  private IEnumerator SelectFirstButton()
  {
    yield return null;
    EventSystem.current.SetSelectedGameObject(selectStartingBugButtonContainer.GetChild(0).gameObject);
  }

  public void HighlightBug(int idx)
  {
    // if (EventSystem.current.currentSelectedGameObject == null)
    // {
    EventSystem.current.SetSelectedGameObject(selectStartingBugButtonContainer.GetChild(idx).gameObject);
    Debug.Log("selected bug");
    // }
    highlightedBug = idx;
    descriptionText.text = startingBugs[idx].description;
    visuals.SetCharacterVisuals(startingBugs[idx].loadout);
  }

  public void UnhighlightBug()
  {
    highlightedBug = -1;
    descriptionText.text = "";
    visuals.ClearCharacterVisuals();
  }

  public void SelectBug(int idx)
  {
    GameMaster.Instance.SelectBugPresetAndBegin(startingBugs[idx]);
    // start the game as selected bug
  }
}
//     // attributeInfoGameObjects = new Dictionary<CharacterAttribute, GameObject>();
//     attributeDataObjects = new Dictionary<CharacterAttribute, AttributeData>();
//     traitButtons = new Dictionary<TraitSlot, TraitButton>();
//     UnityEngine.Object[] dataObjects = Resources.LoadAll("Data/TraitData/Attributes");
//     foreach (UnityEngine.Object obj in dataObjects)
//     {
//         AttributeData attrObj = obj as AttributeData;
//         if (attrObj == null) { continue; }
//         attributeDataObjects.Add(attrObj.attribute, attrObj);
//     }
//     foreach (TraitSlot slot in Enum.GetValues(typeof(TraitSlot)))
//     {
//         GameObject button = Instantiate(traitButtonPrefab, traitButtonsContainer);
//         // button.transform.parent = traitButtonsContainer;
//         traitButtons.Add(slot, button.GetComponentInChildren<TraitButton>());
//     }
// }

// public void Init(CharacterAttributeToIntDictionary attributes, CharacterAttributeToIntDictionary nextAttributes, TraitSlotToTraitDictionary pupaTraits, TraitPickupItem traitPickupItem = null)
// {
//     foreach (CharacterAttribute attribute in attributes.Keys)
//     {
//         AddOrUpdateAttributeInfoObject(attribute, attributes[attribute], nextAttributes[attribute]);
//     }
//     displayedPickupItem = traitPickupItem;
//     if (pupaTraits != null)
//     {
//         Trait itemTrait;
//         foreach (TraitSlot slot in pupaTraits.Keys)
//         {
//             itemTrait = null;
//             if (traitPickupItem != null && traitPickupItem.traits.ContainsKey(slot)) { itemTrait = traitPickupItem.traits[slot]; }
//             traitButtons[slot].gameObject.SetActive(true);
//             traitButtons[slot].Init(slot, pupaTraits[slot], itemTrait, this, attributeDataObjects);
//         }
//     }
//     else
//     {
//         // foreach (TraitButton button in traitButtons.Values)
//         // {
//         //     button.gameObject.SetActive(false);
//         // }
//     }
// }

// public void AddOrUpdateAttributeInfoObject(CharacterAttribute attribute, int value, int nextValue)
// {
//     if (!attributeInfoGameObjects.ContainsKey(attribute))
//     {
//         attributeInfoGameObjects.Add(attribute, Instantiate(attributeInfoPrefab).gameObject);
//         attributeInfoGameObjects[attribute].transform.parent = attributeInfosContainer;
//     }

//     if (!attributeDataObjects.ContainsKey(attribute))
//     {
//         return;
//     }

//     GameObject go = attributeInfoGameObjects[attribute];

//     go.GetComponent<AttributeInfo>().Init(attributeDataObjects[attribute], value, nextValue);
// }

// public void OnTraitButtonClicked(Trait trait, TraitSlot slot)
// {
//     GameMaster.Instance.GetPlayerController().EquipTrait(trait, slot);
//     Destroy(displayedPickupItem.gameObject);
//     GameMaster.Instance.canvasHandler.SetAllCanvasesInactive();
// }

// public void ShowHighlightedTraitDelta(Trait remove, Trait add)
// {
//     if (add != null && remove != null)
//     {
//         traitToRemove = remove;
//         traitToAdd = add;
//         int proposedAttributeAdd = 0;
//         int proposedAttributeRemove = 0;
//         foreach (CharacterAttribute attribute in remove.attributeModifiers.Keys.Union(add.attributeModifiers.Keys))
//         {
//             proposedAttributeAdd = 0;
//             proposedAttributeRemove = 0;
//             if (traitToAdd != null && traitToRemove != null)
//             {
//                 traitToAdd.attributeModifiers.TryGetValue(attribute, out proposedAttributeAdd);
//                 traitToRemove.attributeModifiers.TryGetValue(attribute, out proposedAttributeRemove);
//             }
//             attributeInfoGameObjects[attribute].GetComponent<AttributeInfo>().HighlightDelta(proposedAttributeAdd - proposedAttributeRemove);
//         }
//     }
// }

// public void UnshowHighlightedTraitDelta()
// {

//     foreach (CharacterAttribute attribute in traitToRemove.attributeModifiers.Keys.Union(traitToAdd.attributeModifiers.Keys))
//     {
//         attributeInfoGameObjects[attribute].GetComponent<AttributeInfo>().UnhighlightDelta();
//     }
//     traitToRemove = null;
//     traitToAdd = null;
// }
