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
      traits = overrideTraits;
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

      case Constants.GameState.Play:
      case Constants.GameState.Menu:
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
    if (tile.isInteractable)
    {
      AddContextualAction(tile.GetInteractableText(this), UseTile);
    }
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

  public void UseSelectedSkill()
  {
    if (characterSkills[selectedSkillIdx] != null)
    {
      HandleSkillInput(characterSkills[selectedSkillIdx]);
    }
  }
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

  void HandleActionInput()
  {
    switch (GameMaster.Instance.GetGameStatus())
    {
      // holding down block should cause us to block.
      // taking _any other action_ or releasing the key should cancel the block.
      case (Constants.GameState.Play):
        // Debug.Log("handling player input");
        bool shouldBlock = false;
        if ((CanBlock() && Input.GetButton("Block")) /*|| using block attack?*/)
        {
          shouldBlock = true;
        }
        // receivingSkillInput = Input.GetButton("Attack") || Input.GetButton; // NOT used to determine if attacks are happening! used to hold continuous/charge skills
        if (rewiredPlayer.GetButtonDown("Use Skill"))
        {
          UseSelectedSkill();
          // }
          // chargeAttackTime += Time.deltaTime; // incrementing this further is handled in HandleCooldowns
          // UseSkill(GetSkillEffectForAttackType(AttackType.Basic));
          return;
        }
        if (rewiredPlayer.GetButtonUp("Use Skill"))
        {
          if (pressingSkill != moltSkill)
          {
            pressingSkill = null;
          }
        }
        if (rewiredPlayer.GetButtonDown("Use Attack"))
        {
          UseSelectedAttackSkill();
          // }
          // chargeAttackTime += Time.deltaTime; // incrementing this further is handled in HandleCooldowns
          // UseSkill(GetSkillEffectForAttackType(AttackType.Basic));
          return;
        }
        if (rewiredPlayer.GetButtonUp("Use Attack"))
        {
          if (pressingSkill != moltSkill)
          {
            pressingSkill = null;
          }
        }
        if (rewiredPlayer.GetButtonDown("Molt"))
        {
          HandleSkillInput(moltSkill);
          // }
          // chargeAttackTime += Time.deltaTime; // incrementing this further is handled in HandleCooldowns
          // UseSkill(GetSkillEffectForAttackType(AttackType.Basic));
          return;
        }
        if (rewiredPlayer.GetButtonUp("Molt"))
        {
          if (pressingSkill == moltSkill)
          {
            pressingSkill = null;
          }
        }
        if (CanMove())
        {
          if (Input.GetButtonDown("Ascend"))
          {
            if (flying && GetCanFlyUp() && GridManager.Instance.AdjacentTileIsValidAndEmpty(GetTileLocation(), TilemapDirection.Above))
            {
              FlyUp();
              return;
            }
            else if (GridManager.Instance.CanAscendThroughTileAbove(GetTileLocation(), this))
            {
              AscendOneFloor();
              return;
            }
            // else if (!flying && GetCanFly())
            // {
            //   Fly();
            //   return;
            // }
          }
          else if (Input.GetButtonDown("Descend"))
          {
            Debug.Log("descend?");
            if (flying)
            {
              FlyDown();
              return;
            }
            else
            {
              // DescendOneFloor(); // maybe descend??
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
          // else if (Input.GetButtonDown("Molt"))
          // {
          //   StartCoroutine(Molt());
          //   return;
          // }
        }
        // if (CanAct())
        // {
        //   if (Input.GetButton("Block"))x
        //   {
        //     shouldBlock = true;
        //   }
        //   if (Input.GetButtonDown("Attack"))
        //   {
        //     if (shouldBlock)
        //     {
        //       UseSkill(GetSkillEffectForAttackType(AttackType.Blocking));
        //       return;
        //     }
        //     UseSelectedSkill();
        //     return;
        //     // Attack();
        //   }
        //   else if (Input.GetButtonDown("Spell"))
        //   {
        //     Debug.Log("spell?");
        //     UseSelectedSpell();
        //     return;
        //     // Attack();
        //   }
        //   else if (Input.GetButtonDown("Ascend"))
        //   {
        //     if (flying && GetCanFlyUp() && GridManager.Instance.AdjacentTileIsValidAndEmpty(GetTileLocation(), TilemapDirection.Above))
        //     {
        //       FlyUp();
        //       return;
        //     }
        //     else if (GridManager.Instance.CanAscendThroughTileAbove(GetTileLocation(), this))
        //     {
        //       AscendOneFloor();
        //       return;
        //     }
        //     else if (!flying)
        //     {
        //       Fly();
        //       return;
        //     }
        //   }
        //   else if (Input.GetButtonDown("Descend"))
        //   {
        //     Debug.Log("descend?");
        //     if (flying)
        //     {
        //       FlyDown();
        //       return;
        //     }
        //     else
        //     {
        //       // DescendOneFloor(); // maybe descend??
        //     }
        // }
        //     else if (inventory.lastPickedUpItems.Count > 0)
        //     {
        //       inventory.ClearPickedUpItem();
        //       return;
        //     }
        //   }
        //   else if (Input.GetButtonDown("Molt"))
        //   {
        //     return; // TODO: DELETE THIS
        //     Molt();
        //     return;
        //   }
        // }
        if (Input.GetButtonDown("Dash"))
        {
          // Debug.Log("pressing dash");
          if (CanDash())
          {
            // Debug.Log("doing dash");
            Dash();
          }
          return;
        }
        else if (rewiredPlayer.GetButtonDown("Next Skill"))
        {
          AdvanceSelectedSkill();
          return;
        }
        else if (rewiredPlayer.GetButtonDown("Previous Skill"))
        {
          PreviousSelectedSkill();
          return;
        }

        else if (rewiredPlayer.GetButtonDown("Next Attack"))
        {
          AdvanceSelectedAttack();
          return;
        }
        else if (rewiredPlayer.GetButtonDown("Previous Attack"))
        {
          PreviousSelectedAttack();
          return;
        }
        else if (Input.GetButtonDown("AdvanceSelectedAction"))
        {
          AdvanceSelectedContextualAction();
          return;
        }
        // else if (Input.GetButtonDown("Skill1"))
        // {
        //   if (skill1 != null)
        //   {
        //     skill1.OnActivateTraitPressed();
        //   }
        // }
        // else if (Input.GetButtonDown("Skill2"))
        // {
        //   if (skill2 != null)
        //   {
        //     skill2.OnActivateTraitPressed();
        //   }
        // }
        // if (Input.GetButton("Skill1"))
        // {
        //   if (skill1 != null)
        //   {
        //     skill1.WhileActiveTraitPressed();
        //   }
        // }
        // if (Input.GetButton("Skill2"))
        // {
        //   if (skill2 != null)
        //   {
        //     skill2.WhileActiveTraitPressed();
        //   }
        // }
        // if (Input.GetButtonUp("Skill1"))
        // {
        //   if (skill1 != null)
        //   {
        //     skill1.OnActivateTraitReleased();
        //   }
        // }
        // if (Input.GetButtonUp("Skill2"))
        // {
        //   if (skill2 != null)
        //   {
        //     skill2.OnActivateTraitReleased();
        //   }
        // }
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //   SetCurrentFloor(currentFloor == FloorLayer.F1 ? FloorLayer.F2 : FloorLayer.F1);
        // }
        SetBlocking(shouldBlock);
        break;
      case (Constants.GameState.Menu):
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
    Debug.Log("raised changeFloorLayer event");
    base.SetCurrentFloor(newFloorLayer);
  }

  protected override void HandleTile()
  {

    TileLocation currentLoc = CalculateCurrentTileLocation();
    // GridManager.Instance.DEBUGHighlightTile(currentLoc, Color.red);
    // GridManager.Instance.DEBUGHighlightTile(new TileLocation(Vector3.zero, currentFloor), Color.blue);
    // Debug.Log("current tile world position y" + GetTileLocation().worldPosition.y);
    // Debug.Log("current tile (cube coords int)" + GetTileLocation().CubeCoordsInt());
    // Debug.Log("current tile position y FROM current tile's cube coords" + TileLocation.FromCubicCoords(GetTileLocation().cubeCoords, currentFloor).worldPosition.y);

    // Debug.Log("odd-y: " + ((Mathf.RoundToInt(GetTileLocation().worldPosition.y) & 1)) + ", off by " + (TileLocation.FromCubicCoords(GetTileLocation().cubeCoords, currentFloor).worldPosition - GetTileLocation().worldPosition));
    // Debug.Log("y: " + GetTileLocation().worldPosition.y + ", off by " + (TileLocation.FromCubicCoords(GetTileLocation().cubeCoords, currentFloor).worldPosition - GetTileLocation().worldPosition));

    // PathfindingSystem.Instance.IsPathClearOfHazards(Vector3.zero, currentFloor, this);

    // PathfindingSystem.Instance.GetTilesAlongLine(new TileLocation(Vector3.zero, currentFloor), GetTileLocation(), true);
    // Debug.Log("current tile (offset coords)" + GetTileLocation().tilemapCoordinates);
    // Debug.Log("current tile (cube)" + GetTileLocation().cubeCoords);
    // Debug.Log("upper-left should be " + GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.UpperLeft).tilemapCoordinates);
    // Debug.Log("upper-right should be " + GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.UpperRight).tilemapCoordinates);
    // Debug.Log("lower-left should be " + GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.LowerLeft).tilemapCoordinates);
    // Debug.Log("lower-right should be " + GridManager.Instance.GetAdjacentTileLocation(GetTileLocation(), TilemapDirection.LowerRight).tilemapCoordinates);

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
      if (tile != null && currentTile != null)
      {
        // Debug.Log("changing tile from " + currentTile.tileLocation + " to " + tile.tileLocation);
      }
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

  // TODO: Delete or update this so it sets attributes
  private void AssignTraitsForFirstLife()
  {
    // Debug.Log("assigning traits for first life~");
    // TraitSlotToTraitDictionary et = initialEquippedTraits.EquippedTraits();
    // foreach (TraitSlot traitSlot in et.Keys)
    // {
    //     Trait trait = et[traitSlot];
    //     if (et[traitSlot] != null)
    //     {
    //         Debug.Log("added " + trait);
    //         // trait.OnTraitAdded(this);
    //     }
    //     traits[traitSlot] = trait;
    // }
    // ActiveTraitInstance activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    // ActiveTrait activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+initialskill1) as ActiveTrait;
    // if (activeTrait != null && activeTraitData != null) {
    //   skill1 = activeTrait;
    //   skill1.Init(this, activeTraitData);
    // }
    // activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    // activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+initialskill2) as ActiveTrait;
    // if (activeTrait != null && activeTraitData != null) {
    //   skill2 = activeTrait;
    //   skill2.Init(this, activeTraitData);
    // }
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
        // nextLifeTraits[slot].trait.OnTraitAdded(this);
      }
    }
    LymphTypeToIntDictionary lymphTypeCounts = inventory.GetLymphTypeCounts(nextLifeTraits);
    // bool primarySkillInited = false;
    // foreach (LymphType type in lymphTypeCounts.Keys)
    // {
    //   if (type == LymphType.None) { continue; }
    //   if (lymphTypeCounts[type] >= 2)
    //   {
    //     if (!primarySkillInited)
    //     {
    //       if (skill1 == null)
    //       {
    //         skill1 = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    //       }
    //       Debug.Log("attempting to init skill for type " + type);
    //       skill1.Init(this, GameMaster.Instance.lymphTypeToSkillsMapping[type].GetPrimarySkill());
    //       primarySkillInited = true;
    //     }
    //     else
    //     {
    //       if (skill2 == null)
    //       {
    //         skill2 = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    //       }
    //       skill2.Init(this, GameMaster.Instance.lymphTypeToSkillsMapping[type].GetPrimarySkill());
    //     }
    //   }
    //   if (lymphTypeCounts[type] >= 4)
    //   {
    //     if (skill2 == null)
    //     {
    //       skill2 = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    //     }
    //     skill2.Init(this, GameMaster.Instance.lymphTypeToSkillsMapping[type].GetSecondarySkill());
    //   }
    // }
    // iterate over each item in the dictionary.
    // if we have >2 of a thing,
    // ...and no assigned primary skill: its primary Skill becomes our primary skill.
    // ...and an assigned primary skill: its primary Skill becomes our secondary skill.
    // if we have >4 of that thing, its secondary Skill  becomes our secondary skill.
    // assuming lymphTypeCounts is always <=5 this should just work out

  }
  // ActiveTraitInstance activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
  // ActiveTrait activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+nextLifeTraits.activeTraits[0].traitName) as ActiveTrait;
  // if (activeTrait != null) {
  //   skill1 = activeTrait;
  //   skill1.Init(this, activeTraitData);
  // }
  // activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
  // activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+nextLifeTraits.activeTraits[1].traitName) as ActiveTrait;
  // if (activeTrait != null) {
  //   skill2 = activeTrait;
  //   skill2.Init(this, activeTraitData);
  // }
  // }

  public override CharacterSkillData GetSelectedCharacterSkill()
  {
    if (characterSkills.Count > selectedSkillIdx)
    {
      return characterSkills[selectedSkillIdx];
    }
    return null;
  }

  override public void Die()
  {
    foreach (TraitSlot slot in traits.Keys)
    {
      if (traits[slot] != null && traits[slot])
      {
        // Debug.Log("Removing trait in" + slot);
        // traits[slot].OnTraitRemoved(this);
      }
    }
    // Debug.Log("all traits removed; killing player");
    GameMaster.Instance.KillPlayer(pupa);
    // Debug.Log("destroying gameobject");
    base.Die();
  }

}
