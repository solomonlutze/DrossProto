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

  public UIBug uiBug;

  public int highlightedBug;

  public void Init()
  {
    foreach (Transform child in selectStartingBugButtonContainer.transform)
    {
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
    highlightedBug = idx;
    descriptionText.text = startingBugs[idx].description;
    uiBug.Init(startingBugs[idx].loadout);
  }

  public void UnhighlightBug()
  {
    highlightedBug = -1;
    descriptionText.text = "";
  }

  public void SelectBug(int idx)
  {
    GameMaster.Instance.SelectBugPresetAndBegin(startingBugs[idx]);
  }
}
