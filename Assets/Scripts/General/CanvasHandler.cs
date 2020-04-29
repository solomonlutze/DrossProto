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
  private List<GameObject> canvasList = new List<GameObject>();

  // Use this for initialization
  void Start()
  {
    // canvasList.Add(inventoryScreen.gameObject);
    canvasList.Add(attributesView.gameObject);
    SetAllCanvasesInactive();
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      Debug.Log("Quit!"); Application.Quit();
    }
    else if (Input.GetKeyDown(KeyCode.I))
    {
      if (attributesView.gameObject.activeSelf)
      {
        SetAllCanvasesInactive();
        GameMaster.Instance.SetGameStatus(Constants.GameState.Play);
      }
      else
      {
        DisplayAttributesView();
      }
    }

  }

  // TODO: this should def actually advance dialogue
  // TODO: Should this go in DialogueCanvas?
  public void AdvanceDialogue()
  {
    SetAllCanvasesInactive();
    GameMaster.Instance.SetGameStatus(Constants.GameState.Play);
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
      DisplayAttributesView(player.attributes, Character.CalculateAttributes(player.pupa), player.pupa, null);
    }
  }

  public void DisplayAttributesView(
      CharacterAttributeToIntDictionary currentAttributes,
      CharacterAttributeToIntDictionary nextAttributes,
      TraitSlotToTraitDictionary pupaTraits,
      TraitPickupItem traitPickupItem)
  {
    SetAllCanvasesInactive();
    {
      attributesView.gameObject.SetActive(true);
      attributesView.Init(currentAttributes, nextAttributes, pupaTraits, traitPickupItem);
      attributesView.gameObject.SetActive(true);
      GameMaster.Instance.SetGameStatus(Constants.GameState.Menu);
    }
  }

  public void DisplayAttributesViewForTraitItem(TraitPickupItem traitItem)
  {
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (player != null)
    {
      DisplayAttributesView(player.attributes, Character.CalculateAttributes(player.pupa), player.pupa, traitItem);
    }
  }
}
