using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

  private List<GameObject> interactables;
  private Inventory inventory;

  private PassiveTrait passiveTrait1;
  private PassiveTrait passiveTrait2;
  public Transform cameraFollowTarget;
  private TileLocation lastSafeTileLocation;
  public SpawnPoint spawnPoint;

  public string lastActivatedTrait = null;
  public List<ContextualAction> availableContextualActions;
  private int selectedContextualActionIdx = 0;
  private int selectedSkillIdx = 0;


  [Header("Trait Info", order = 1)]
  public TraitSlotToTraitDictionary pupa;

  [Header("Trait-Related Prefabs")]
  public EnvironmentTile burrowHoleTile;
  // Use this for initialization
  override protected void Start()
  {
    base.Start();
  }

  public void Init(bool initialSpawn, TraitSlotToTraitDictionary previousPupa)
  {
    if (!initialSpawn)
    {
      traits = previousPupa;
      pupa = new TraitSlotToTraitDictionary(previousPupa);
    }
    Debug.Log("Init??");
    characterVisuals.SetCharacterVisuals(traits);
    base.Init();
    availableContextualActions = new List<ContextualAction>();
    interactables = new List<GameObject>();
    inventory = GetComponent<Inventory>();
    inventory.owner = this;
    // if (initialSpawn)
    // {
    //     AssignTraitsForFirstLife();
    // }
    // else
    // {
    //     // inventory.AdvanceUpcomingLifeTraits(previousPupa);
    // }
  }
  // Player specific non-physics biz.

  // TODO: setting orientTowards can prolly be its own function
  override protected void Update()
  {

    // Ray ray;
    // RaycastHit hit;
    // ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    // if (Physics2d.Raycast(ray, out hit, Mathf.Infinity))
    // {
    //     Debug.Log("hit: " + hit.point);
    // }
    // Ray ray = GameMaster.Instance.mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    // Debug.DrawRay(ray.origin, ray.direction * 50, Color.yellow);
    // Debug.Log("Mouse position: " + Input.mousePosition);
    orientTowards = GameMaster.Instance.camera2D.ScreenToWorldPoint(Input.mousePosition);
    // Debug.Log("orientTowards: " + orientTowards);
    // Debug.Log("transform position: " + transform.position);
    // Debug.Log("target direction: " + (orientTowards - transform.position));
    // Debug.DrawLine(orientTowards, transform.position, Color.cyan, .1f);
    base.Update();
    PopulateContextualActions();
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

  protected void RespawnPlayerAtLastSafeLocation()
  {
    // skill1.CancelActiveEffects();
    // skill2.CancelActiveEffects();
    transform.position =
      new Vector3(lastSafeTileLocation.position.x + .5f, lastSafeTileLocation.position.y + .5f, 0);
    if (currentFloor != lastSafeTileLocation.floorLayer)
    {
      SetCurrentFloor(lastSafeTileLocation.floorLayer);
    }
    CalculateAndApplyStun(.5f, true);
    po.HardSetVelocityToZero();
  }

  protected override void HandleTile()
  {
    EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
    base.HandleTile();
    if (tile == null) { return; }
    if (tile.CanRespawnPlayer())
    {
      if (!tile.CharacterCanCrossTile(this))
      {
        RespawnPlayerAtLastSafeLocation();
      }
    }
    else
    {
      lastSafeTileLocation = currentTileLocation;
    }
  }

  public override void HandleTileCollision(EnvironmentTileInfo tile)
  {
    if (tile.GetColliderType() == Tile.ColliderType.None)
    {
      return;
    }
    else
    {
      // Debug.Log("collided with " + tile);
      if (tile.CharacterCanBurrowThroughObjectTile(this))
      {
        tile.DestroyObjectTile();
      }
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
        Debug.Log("interactableObject: " + interactableObject);
        if (!interactable.isInteractable) { continue; }
        AddContextualAction(
          interactable.interactableText,
          () => interactableObject.SendMessage("PlayerActivate", this, SendMessageOptions.DontRequireReceiver)
        );
      }
    }
    if (activeMovementAbilities.Contains(CharacterMovementAbility.StickyFeet) && GridManager.Instance.CanClimbAdjacentTile(GetTileLocation()))
    {
      AddContextualAction("climb", ClimbAdjacentTile);
    }
    if (GridManager.Instance.AdjacentTileIsValid(GetTileLocation(), TilemapDirection.Above) && GridManager.Instance.CanAscendThroughTileAbove(GetTileLocation(), this))
    {
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
    Debug.Log("using skill " + characterSkills[selectedSkillIdx]);
    Debug.Log("is attack: " + (AttackSkillData)characterSkills[selectedSkillIdx] != null);
    if ((AttackSkillData)characterSkills[selectedSkillIdx] != null) // TODO: handle this more generically!!
    {
      StartCoroutine(DoAttack((AttackSkillData)characterSkills[selectedSkillIdx]));
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
      Debug.Log("selected skill: " + characterSkills[selectedSkillIdx]);
    }
    else
    {
      selectedSkillIdx = 0;
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
    if (Input.GetKey("w"))
    {
      movementInput.y = 1;
    }
    else if (Input.GetKey("s"))
    {
      movementInput.y = -1;
    }
    else
    {
      movementInput.y = 0;
    }
    if (Input.GetKey("d"))
    {
      movementInput.x = 1;
    }
    else if (Input.GetKey("a"))
    {
      movementInput.x = -1;
    }
    else
    {
      movementInput.x = 0;
    }
  }
  // Handle player action inputs. Currently only attack does anything useful.
  // TODO: Use GetButtonDown instead of GetKeyDown
  void HandleActionInput()
  {
    switch (GameMaster.Instance.GetGameStatus())
    {
      case (Constants.GameState.Play):
        if (Input.GetButtonDown("Attack"))
        {
          UseSelectedSkill();
          // Attack();
        }
        else if (Input.GetButtonDown("Ascend"))
        {
          if (flying && GetCanFlyUp() && GridManager.Instance.AdjacentTileIsValidAndEmpty(GetTileLocation(), TilemapDirection.Above))
          {
            FlyUp();
          }
          else if (!flying)
          {
            Fly();
          }
        }
        else if (Input.GetButtonDown("Descend"))
        {
          if (flying)
          {
            FlyDown();
          }
          else
          {
            // DescendOneFloor(); // maybe descend??
          }
        }
        else if (Input.GetButtonDown("Activate"))
        {
          if (availableContextualActions.Count > 0)
          {
            GetSelectedContextualAction().actionToCall();
          }
          else if (inventory.lastPickedUpItems.Count > 0)
          {
            inventory.ClearPickedUpItem();
          }
        }
        else if (Input.GetButtonDown("AdvanceSkill"))
        {
          AdvanceSelectedSkill();
        }
        else if (Input.GetButtonDown("AdvanceSelectedAction"))
        {
          AdvanceSelectedContextualAction();
        }
        else if (Input.GetButtonDown("Molt"))
        {
          Molt();
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
        if (Input.GetKeyDown(KeyCode.P))
        {
          SetCurrentFloor(currentFloor == FloorLayer.F1 ? FloorLayer.F2 : FloorLayer.F1);
        }
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
    inventory.AddToInventory(item);
  }

  public override void SetCurrentFloor(FloorLayer newFloorLayer)
  {
    // Debug.Log("should be changing to floor " + newFloorLayer);
    // GridManager.Instance.OnLayerChange(newFloorLayer);
    base.SetCurrentFloor(newFloorLayer);
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

  public override Weapon GetSelectedWeaponInstance()
  {
    if (characterSkills.Count > selectedSkillIdx)
    {
      string skillName = GetSkillNameFromIndex(selectedSkillIdx);
      return weaponInstances[skillName];
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
