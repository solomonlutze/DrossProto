using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ScriptableObjectArchitecture;
using Rewired;

public struct ContextualAction
{
  public string actionName;
  public Action actionToCall;

  public ContextualAction(string n, Action a)
  {
    actionName = n;
    actionToCall = a;
  }
}

public class PlayerController : Character
{

  public int rewiredPlayerId = 0;
  private Rewired.Player rewiredPlayer;
  private List<GameObject> interactables;
  private Inventory inventory;

  public Transform cameraFollowTarget;
  public SpawnPoint spawnPoint;

  public string lastActivatedTrait = null;
  public List<ContextualAction> availableContextualActions;
  private int selectedContextualActionIdx = 0;
  public int selectedSkillIdx = 0;
  public int selectedAttackSkillIdx = 0;
  private int selectedSpellIdx = 0;

  public IntVariable currentFloorLayer;
  public GameEvent changedFloorLayerEvent;

  public IntVariable trophyGrubCount;
  public GameEvent changedTrophyGrubCountEvent;

  [Header("Trait Info", order = 1)]
  public TraitSlotToTraitDictionary pupa;

  [Header("Trait-Related Prefabs")]
  public EnvironmentTile burrowHoleTile;
  // Use this for initialization
  override protected void Start()
  {
    rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerId);
    base.Start();
  }

  public void Init(TraitSlotToTraitDictionary overrideTraits = null)
  {
    if (overrideTraits != null)
    {
      traits = new TraitSlotToTraitDictionary(overrideTraits);
      pupa = new TraitSlotToTraitDictionary(overrideTraits);
    }
    characterVisuals.SetCharacterVisuals(traits);
    base.Init();
    currentTile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
    if (currentTile.infoTileType != null)
    {
      foreach (MusicStem stem in Enum.GetValues(typeof(MusicStem)))
      {
        if (!currentTile.infoTileType.musicStems.Contains(stem))
        {
          AkSoundEngine.PostEvent(stem.ToString() + "_Mute", GameMaster.Instance.gameObject);
        }
        AkSoundEngine.PostEvent("PlayClergyLoop", GameMaster.Instance.gameObject);
      }
      GridManager.Instance.PlayerChangedTile(CalculateCurrentTileLocation());
      // GridManager.Instance.LoadAndUnloadChunks(CalculateCurrentTileLocation());
    }
    availableContextualActions = new List<ContextualAction>();
    interactables = new List<GameObject>();
    // inventory = GetComponent<Inventory>();
    // inventory.owner = this;
    GameMaster.Instance.playerObliterated = false;
  }
  // Player specific non-physics biz.

  // TODO: setting orientTowards can prolly be its own function
  override protected void Update()
  {
    if (!InCrit())
    {
      // orientTowards = GameMaster.Instance.camera2D.ScreenToWorldPoint(Input.mousePosition);
      if (movementInput != Vector2.zero)
      {

        orientTowards = movementInput;
      }
    }
    base.Update();
    PopulateContextualActions();
    // GridManager.Instance.DEBUGHighlightTile(GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.Left), Color.red);
    // GridManager.Instance.DEBUGHighlightTile(GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.LowerLeft), Color.yellow);
    // GridManager.Instance.DEBUGHighlightTile(GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.LowerRight), Color.green);
    // GridManager.Instance.DEBUGHighlightTile(GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.Right), Color.cyan);
    // GridManager.Instance.DEBUGHighlightTile(GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.UpperRight), Color.blue);
    // GridManager.Instance.DEBUGHighlightTile(GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.UpperLeft), Color.magenta);
    HandleInput();
  }

  // TODO: implement other game states e.g. dialogue
  // TODO: Use GetButtonDown instead of GetKeyDown
  void HandleInput()
  {
    switch (GameMaster.Instance.GetGameStatus())
    {

      case DrossConstants.GameState.Play:
      case DrossConstants.GameState.Menu:
        HandleMovementInput();
        HandleActionInput();
        break;
    }
  }

  private void SpawnBurrowHoleTile()
  {
    GridManager.Instance.ReplaceTileAtLocation(GetTileLocation(), burrowHoleTile);
  }

  private void ClimbAdjacentTile()
  {
    SetCurrentFloor(currentFloor + 1);
  }

  public void AddContextualAction(string name, Action action)
  {
    availableContextualActions.Add(new ContextualAction(name, action));
  }

  void PopulateContextualActions()
  {
    availableContextualActions.Clear();
    EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(GetTileLocation());
    // if (tile.isInteractable)
    // {
    //   AddContextualAction(tile.GetInteractableText(this), UseTile);
    // }
    if (interactables.Count > 0)
    {

      interactables.RemoveAll(interactableObject => interactableObject == null);
      foreach (GameObject interactableObject in interactables)
      {
        Interactable interactable = interactableObject.GetComponent<Interactable>();
        if (!interactable.isInteractable) { continue; }
        AddContextualAction(
          interactable.interactableText,
          () => interactableObject.SendMessage("PlayerActivate", this, SendMessageOptions.DontRequireReceiver)
        );
      }
    }
    // if (activeMovementAbilities.Contains(CharacterMovementAbility.StickyFeet) && GridManager.Instance.CanClimbAdjacentTile(GetTileLocation()))
    // {
    //   AddContextualAction("climb", ClimbAdjacentTile);
    // }
    if (GridManager.Instance.AdjacentTileIsValid(GetTileLocation(), TilemapDirection.Above) && GridManager.Instance.CanAscendThroughTileAbove(GetTileLocation(), this))
    {
      Debug.Log("adding ascend action");
      AddContextualAction("ascend", AscendOneFloor);
    }
    if (GridManager.Instance.AdjacentTileIsValid(GetTileLocation(), TilemapDirection.Below) && GridManager.Instance.CanDescendThroughCurrentTile(GetTileLocation(), this))
    {
      AddContextualAction("descend", DescendOneFloor);
    }
    if (selectedContextualActionIdx > 0 && selectedContextualActionIdx >= availableContextualActions.Count)
    {
      selectedContextualActionIdx = 0;
    }
  }

  // public void UseSelectedSkill()
  // {
  //   if (characterSkills[selectedSkillIdx] != null)
  //   {
  //     HandleSkillInput(characterSkills[selectedSkillIdx]);
  //   }
  // }
  public void UseSelectedAttackSkill()
  {
    if (characterAttackSkills[selectedAttackSkillIdx] != null)
    {
      HandleSkillInput(characterAttackSkills[selectedAttackSkillIdx]);
    }
  }
  public void PreviousSelectedSkill()
  {
    if (characterSkills.Count > 0)
    {
      selectedSkillIdx = selectedSkillIdx - 1;
      if (selectedSkillIdx < 0)
      {
        selectedSkillIdx = characterSkills.Count - 1;
      }
    }
    else
    {
      selectedSkillIdx = 0;
    }
  }

  public CharacterSkillData GetSkillForActionInput(string actionInput)
  {
    return characterSkills[GetTraitSlotForActionInput(actionInput)];
  }
  public static TraitSlot GetTraitSlotForActionInput(string actionInput)
  {
    switch (actionInput)
    {
      case "Use Skill 1":
        return TraitSlot.Head;
      case "Use Skill 2":
        return TraitSlot.Abdomen;
      case "Use Skill 3":
        return TraitSlot.Legs;
      case "Use Skill 4":
        return TraitSlot.Wings;
      case "Use Skill 5":
        return TraitSlot.Thorax;
      default:
        Debug.LogError("received invalid input " + actionInput);
        return TraitSlot.Thorax;
    }
  }

  public void AdvanceSelectedSkill()
  {
    if (characterSkills.Count > 0)
    {
      selectedSkillIdx = selectedSkillIdx + 1;
      if (selectedSkillIdx >= characterSkills.Count)
      {
        selectedSkillIdx = 0;
      }
    }
    else
    {
      selectedSkillIdx = 0;
    }
  }

  public void PreviousSelectedAttack()
  {
    if (characterAttackSkills.Count > 0)
    {
      selectedAttackSkillIdx = selectedAttackSkillIdx - 1;
      if (selectedAttackSkillIdx < 0)
      {
        selectedAttackSkillIdx = characterAttackSkills.Count - 1;
      }
    }
    else
    {
      selectedAttackSkillIdx = 0;
    }
  }

  public void AdvanceSelectedAttack()
  {
    if (characterAttackSkills.Count > 0)
    {
      selectedAttackSkillIdx = selectedAttackSkillIdx + 1;
      if (selectedAttackSkillIdx >= characterAttackSkills.Count)
      {
        selectedAttackSkillIdx = 0;
      }
    }
    else
    {
      selectedAttackSkillIdx = 0;
    }
  }
  public void UseSelectedSpell()
  {
    if (characterSpells.Count > 0 && characterSpells[selectedSpellIdx] != null)
    {
      HandleSkillInput(characterSpells[selectedSpellIdx]);
    }
  }

  public void AdvanceSelectedSpell()
  {
    if (characterSpells.Count > 0)
    {
      selectedSpellIdx = selectedSpellIdx + 1;
      if (selectedSpellIdx >= characterSpells.Count)
      {
        selectedSpellIdx = 0;
      }
      Debug.Log("selected spell: " + characterSpells[selectedSpellIdx]);
    }
    else
    {
      selectedSpellIdx = 0;
    }
  }

  public void AdvanceSelectedContextualAction()
  {
    if (availableContextualActions.Count > 0)
    {
      selectedContextualActionIdx = selectedContextualActionIdx + 1;
      if (selectedContextualActionIdx >= availableContextualActions.Count)
      {
        selectedContextualActionIdx = 0;
      }
    }
    else
    {
      selectedContextualActionIdx = 0;
    }

  }
  public ContextualAction GetSelectedContextualAction()
  {
    return availableContextualActions[selectedContextualActionIdx];
  }
  // Handle movement inputs.
  // TODO: Use GetButtonDown instead of GetKeyDown
  void HandleMovementInput()
  {
    Vector3 newPos = transform.position;
    movementInput.x = rewiredPlayer.GetAxis("Move Horizontal");
    movementInput.y = rewiredPlayer.GetAxis("Move Vertical");
    // if (Input.GetKey("w"))
    // {
    //   movementInput.y = 1;
    // }
    // else if (Input.GetKey("s"))
    // {
    //   movementInput.y = -1;
    // }
    // else
    // {
    //   movementInput.y = 0;
    // }
    // if (Input.GetKey("d"))
    // {
    //   movementInput.x = 1;
    // }
    // else if (Input.GetKey("a"))
    // {
    //   movementInput.x = -1;
    // }
    // else
    // {
    //   movementInput.x = 0;
    // }
  }

  public static string[] skillActionNames = new string[] { "Use Skill 1", "Use Skill 2", "Use Skill 3", "Use Skill 4", "Use Skill 5" };

  void HandleActionInput()
  {
    switch (GameMaster.Instance.GetGameStatus())
    {
      // holding down block should cause us to block.
      // taking _any other action_ or releasing the key should cancel the block.
      case (DrossConstants.GameState.Play):
        // Debug.Log("handling player input");
        foreach (string skillActionName in skillActionNames)
        {
          if (rewiredPlayer.GetButtonDown(skillActionName))
          {
            HandleSkillInput(GetSkillForActionInput(skillActionName));
            return;
          }
          if (rewiredPlayer.GetButtonUp(skillActionName))
          {
            if (pressingSkill == GetSkillForActionInput(skillActionName))
            {
              pressingSkill = null;
            }
          }
        }
        if (CanMove())
        {
          if (Input.GetButtonDown("Ascend"))
          {
            if (GridManager.Instance.CanAscendThroughTileAbove(GetTileLocation(), this))
            {
              AscendOneFloor();
              return;
            }
          }
          else if (rewiredPlayer.GetButtonDown("Interact"))
          {
            Debug.Log("Interact?");
            if (availableContextualActions.Count > 0)
            {
              GetSelectedContextualAction().actionToCall();
              return;
            }
          }
        }
        else if (Input.GetButtonDown("AdvanceSelectedAction"))
        {
          AdvanceSelectedContextualAction();
          return;
        }
        break;
      case (DrossConstants.GameState.Menu):
        break;
    }

  }

  public void AddInteractable(Interactable interactable)
  {
    if (!interactables.Contains(interactable.gameObject))
    {
      interactables.Add(interactable.gameObject);
    }
  }

  public void RemoveInteractable(Interactable interactable)
  {
    interactable.OnRemove();
    interactables.Remove(interactable.gameObject);
  }

  public void AddToInventory(PickupItem item)
  {
    if (item.itemType == InventoryItemType.Currency)
    {
      trophyGrubCount.Value += item.quantity;
      Debug.Log("trophyGrubCount: " + trophyGrubCount.Value);
      Debug.Log("item quantity: " + item.quantity);
      changedTrophyGrubCountEvent.Raise();
    }
    else
    {
      inventory.AddToInventory(item);
    }
  }

  public override void SetCurrentFloor(FloorLayer newFloorLayer)
  {
    // change currentFloorLayer scriptableObject variable
    // raise floorLayerChanged event
    currentFloorLayer.Value = (int)newFloorLayer;
    changedFloorLayerEvent.Raise();
    base.SetCurrentFloor(newFloorLayer);
  }

  protected override void HandleTile()
  {

    TileLocation currentLoc = CalculateCurrentTileLocation();

    EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(currentLoc);
    if (tile == null)
    {
      {
        GameMaster.Instance.playerObliterated = true;
        Die();
        return;
      }
    }
    if (tile != currentTile)
    {
      currentTile = tile;
      GridManager.Instance.PlayerChangedTile(currentLoc, GetSightRange(), defaultCharacterData.GetDarkVisionAttributeData().GetDarkVisionInfos(this));
    }
    base.HandleTile();
  }
  public void RemoveFromInventory(string itemId, int quantity)
  {
    inventory.RemoveFromInventory(itemId, quantity);
  }

  public void EquipConsumableToSlot(string itemToEquip, int slot)
  {
    inventory.EquipConsumableToSlot(itemToEquip, slot);
  }

  public void EquipTrait(Trait itemTrait, TraitSlot slot)
  {
    pupa[slot] = itemTrait;
  }

  override public void AssignTraitsForNextLife(TraitSlotToUpcomingTraitDictionary nextLifeTraits)
  {
    foreach (TraitSlot slot in nextLifeTraits.Keys)
    {
      if (nextLifeTraits[slot] != null && nextLifeTraits[slot].trait != null)
      {
        traits[slot] = nextLifeTraits[slot].trait;
      }
    }
    LymphTypeToIntDictionary lymphTypeCounts = inventory.GetLymphTypeCounts(nextLifeTraits);
  }

  public override void PlayDamageSounds()
  {
    base.PlayDamageSounds();
    AkSoundEngine.PostEvent("TriggerClergySmallEnemyHit", GameMaster.Instance.gameObject);
  }

  public override void DoCameraShake(float damageAfterResistances, float knockbackDistance)
  {

    float duration = .06f * knockbackDistance;
    float magnitude = .04f * knockbackDistance;
    GameMaster.Instance.DoCameraShake(duration, magnitude);
    Debug.Log("camera shake?" + duration + ", " + magnitude);
  }
  override public void Die()
  {
    GameMaster.Instance.KillPlayer(pupa);
    base.Die();
  }

}
