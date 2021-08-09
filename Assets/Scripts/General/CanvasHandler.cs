using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// WIP - NOT IN USE YET.
// Ported over from another project.

// Controls all canvases. All "public" functions on canvases should be exposed here.
// Any component that needs to interact with a canvas should interact with CanvasHandler.
public class CanvasHandler : MonoBehaviour
{

  public InventoryScreen inventoryScreen;

  public AttributesView attributesView;
  public StartingBugSelectScreen startingBugSelectScreen;
  private List<GameObject> canvasList = new List<GameObject>();

  // Use this for initialization
  void Start()
  {
    // canvasList.Add(inventoryScreen.gameObject);
    canvasList.Add(attributesView.gameObject);
    canvasList.Add(startingBugSelectScreen.gameObject);
    SetAllCanvasesInactive();
  }

  // Update is called once per frame
  void Update()
  {
    switch (GameMaster.Instance.GetGameStatus())
    {
      case Constants.GameState.ChooseBug:
        if (!startingBugSelectScreen.gameObject.activeSelf)
        {
          DisplaySelectBugScreen();
        }
        break;
      case Constants.GameState.Menu:
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I))
        {
          CloseMenus();
        }
        break;
      case Constants.GameState.Play:
        if (Input.GetKeyDown(KeyCode.I))
        {
          {
            GameMaster.Instance.SetGameUnpaused();
            DisplayAttributesView();
          }
        }
        break;
    }

  }

  // TODO: this should def actually advance dialogue
  // TODO: Should this go in DialogueCanvas?
  public void AdvanceDialogue()
  {
    SetAllCanvasesInactive();
    GameMaster.Instance.SetGameStatus(Constants.GameState.Play);
  }

  public void CloseMenus()
  {
    GameMaster.Instance.SetGameUnpaused();
    SetAllCanvasesInactive();
  }

  public void SetAllCanvasesInactive()
  {
    foreach (GameObject canvas in canvasList)
    {
      canvas.SetActive(false);
    }
  }

  // Dialogue canvas functions

  public void DisplayAttributesView()
  {
    // BulletinBoardCanvas.GetComponent<BulletinBoardCanvas>().SetBulletinBoardText();
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (player != null)
    {
      // DisplayAttributesView(player.attributes, Character.CalculateAttributes(player.pupa), player.characterSkills, Character.CalculateSkills(player.pupa), player.pupa, null);
    }
  }

  public void DisplaySelectBugScreen()
  {
    SetAllCanvasesInactive();
    startingBugSelectScreen.Init();
    startingBugSelectScreen.gameObject.SetActive(true);
  }

  public void DisplayAttributesView(
    CharacterAttributeToIntDictionary currentAttributes,
    CharacterAttributeToIntDictionary nextAttributes,
    List<CharacterSkillData> skillDatas,
    List<CharacterSkillData> pupaSkillDatas,
    TraitSlotToTraitDictionary pupaTraits,
    TraitPickupItem traitPickupItem)
  {
    GameMaster.Instance.SetGameMenu();
    SetAllCanvasesInactive();
    attributesView.gameObject.SetActive(true);
    attributesView.Init(currentAttributes, nextAttributes, skillDatas, pupaSkillDatas, pupaTraits, traitPickupItem);
    attributesView.gameObject.SetActive(true);
  }

  public void DisplayAttributesViewForTraitItem(TraitPickupItem traitItem)
  {
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (player != null)
    {
      // DisplayAttributesView(player.attributes, Character.CalculateAttributes(player.pupa), player.characterSkills, Character.CalculateSkills(player.pupa), player.pupa, traitItem);
    }
  }
}
