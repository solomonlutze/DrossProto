using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;


// WIP - NOT IN USE YET.
// Ported over from another project.

// Controls all canvases. All "public" functions on canvases should be exposed here.
// Any component that needs to interact with a canvas should interact with CanvasHandler.
public class CanvasHandler : MonoBehaviour
{


  public int rewiredPlayerId = 0;
  private Rewired.Player rewiredPlayer; public InventoryScreen inventoryScreen;

  public BugStatusView bugStatusScreen;
  public PauseMenu pauseScreen;
  public StartingBugSelectScreen startingBugSelectScreen;
  private List<GameObject> canvasList = new List<GameObject>();

  // Use this for initialization
  void Start()
  {
    // canvasList.Add(inventoryScreen.gameObject);
    rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerId);
    canvasList.Add(bugStatusScreen.gameObject);
    canvasList.Add(startingBugSelectScreen.gameObject);
    canvasList.Add(pauseScreen.gameObject);
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
        if (rewiredPlayer.GetButtonDown("UICancel"))
        {
          CloseMenus();
        }
        break;
      case Constants.GameState.Play:
        if (Input.GetKeyDown(KeyCode.I))
        {
          {
            GameMaster.Instance.SetGameUnpaused();
            DisplayBugStatusView();
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

  public void DisplayBugStatusView()
  {
    // BulletinBoardCanvas.GetComponent<BulletinBoardCanvas>().SetBulletinBoardText();
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (player != null)
    {
      DisplayBugStatusView(player.attributes, Character.CalculateAttributes(player.pupa), null, null, player.pupa, null);
    }
  }

  public void DisplayPauseMenu()
  {
    GameMaster.Instance.SetGameMenu();
    pauseScreen.gameObject.SetActive(true);
  }

  public void ClosePauseMenu()
  {
    pauseScreen.gameObject.SetActive(false);
  }
  public void DisplaySelectBugScreen()
  {
    SetAllCanvasesInactive();
    startingBugSelectScreen.Init();
    startingBugSelectScreen.gameObject.SetActive(true);
  }

  public void DisplayBugStatusView(
    CharacterAttributeToIntDictionary currentAttributes,
    CharacterAttributeToIntDictionary nextAttributes,
    List<CharacterSkillData> skillDatas,
    List<CharacterSkillData> pupaSkillDatas,
    TraitSlotToTraitDictionary pupaTraits,
    TraitPickupItem traitPickupItem)
  {
    GameMaster.Instance.SetGameMenu();
    SetAllCanvasesInactive();
    // bugStatusScreen.Init(currentAttributes, nextAttributes, skillDatas, pupaSkillDatas, pupaTraits, traitPickupItem);
    bugStatusScreen.gameObject.SetActive(true);
  }

  public void DisplayBugStatusView(
    TraitSlotToTraitDictionary currentTraits,
    TraitPickupItem traitPickupItem = null)
  {
    GameMaster.Instance.SetGameMenu();
    SetAllCanvasesInactive();
    bugStatusScreen.Init(currentTraits, traitPickupItem);
    bugStatusScreen.gameObject.SetActive(true);
  }

  public void DisplayBugStatusViewForTraitItem(TraitPickupItem traitItem)
  {
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (player != null)
    {
      DisplayBugStatusView(player.pupa, traitItem);
    }
  }
}
