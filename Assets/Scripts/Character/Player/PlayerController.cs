using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public struct ContextualAction {
	public string actionName;
	public Action actionToCall;

	public ContextualAction(string n, Action a) {
		actionName = n;
		actionToCall = a;
	}
}

public class PlayerController : Character {

	private List<GameObject> interactables;
	private Inventory inventory;

	private PassiveTrait passiveTrait1;
	private PassiveTrait passiveTrait2;

	public ActiveTraitInstance skill1;
	public ActiveTraitInstance skill2;
	private TileLocation lastSafeTileLocation;

	public string lastActivatedTrait = null;
	public List<ContextualAction> availableContextualActions;
	private int selectedContextualActionIdx;
	// Use this for initialization
	override protected void Start () {
		base.Start();
	}

	public void Init(bool initialSpawn, TraitSlotToUpcomingTraitDictionary previousPupa) {
		base.Init();
		availableContextualActions = new List<ContextualAction>();
		interactables = new List<GameObject>();
		inventory = GetComponent<Inventory>();
		inventory.owner = this;
    if (!initialSpawn) {
      Debug.Log("init - cachedPupa head trait: "+previousPupa[TraitSlot.Head].trait);
    }
    if (initialSpawn) {
      AssignTraitsForFirstLife();
    } else {
  		inventory.AdvanceUpcomingLifeTraits(previousPupa);
    }
	}
	// Player specific non-physics biz.

	// TODO: setting orientTowards can prolly be its own function
	override protected void Update () {
		orientTowards = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		base.Update();
		PopulateContextualActions();
		HandleInput();
	}

	// TODO: implement other game states e.g. dialogue
	// TODO: Use GetButtonDown instead of GetKeyDown
	void HandleInput() {
		switch (GameMaster.Instance.GetGameStatus()) {

		case Constants.GameState.Play:
		case Constants.GameState.Menu:
			HandleMovementInput();
			HandleActionInput();
			break;
		}
	}

	protected void RespawnPlayerAtLatSafeLocation() {
		skill1.CancelActiveEffects();
		skill2.CancelActiveEffects();
		transform.position =
			new Vector3(lastSafeTileLocation.position.x + .5f, lastSafeTileLocation.position.y + .5f, 0);
		if (currentFloor != lastSafeTileLocation.floorLayer) {
			SetCurrentFloor(lastSafeTileLocation.floorLayer);
		}
		CalculateAndApplyStun(.5f, true);
		po.HardSetVelocityToZero();
	}

	protected override void HandleTile() {
		EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
		base.HandleTile();
		if (tile == null) { return; }
		if (tile.CanRespawnPlayer()) {
			if (!tile.CharacterCanCrossTile(activeMovementAbilities)) {
				RespawnPlayerAtLatSafeLocation();
			}
		} else {
			lastSafeTileLocation = currentTileLocation;
		}
	}

	public override void HandleTileCollision(EnvironmentTileInfo tile) {
		if (tile.GetColliderType() == Tile.ColliderType.None) {
			return;
		}
		else {
			if (tile.objectTileTags.Contains(TileTag.Ground) && activeMovementAbilities.Contains(CharacterMovementAbility.Burrow)) {
				tile.DestroyTile();
        tile.DestroyTile();
			}
		}
	}

	private void ClimbAdjacentTile() {
    transform.position = new Vector3 (
      currentTileLocation.position.x + .5f,
      currentTileLocation.position.y + .5f,
      0f
    );
		SetCurrentFloor(currentFloor + 1);
	}

	private void AddContextualAction(string name, Action action) {
		availableContextualActions.Add(new ContextualAction(name, action));
	}

	void PopulateContextualActions() {
		availableContextualActions.Clear();
		EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
		if (tile.isInteractable) {
			AddContextualAction(tile.GetInteractableText(this), UseTile);
		}
		if (activeMovementAbilities.Contains(CharacterMovementAbility.StickyFeet) && GridManager.Instance.CanClimbAdjacentTile(transform.position, currentFloor)) {
			AddContextualAction("climb", ClimbAdjacentTile);
		}
		if (interactables.Count > 0) {
			foreach (GameObject interactableObject in interactables) {
				Interactable interactable = interactableObject.GetComponent<Interactable>();
        if (!interactable.isInteractable) { continue; }
				AddContextualAction(
					interactable.interactableText,
					() => interactableObject.SendMessage("PlayerActivate", this, SendMessageOptions.DontRequireReceiver)
				);
			}
		}
		if (selectedContextualActionIdx > 0 && selectedContextualActionIdx >= availableContextualActions.Count) {
			selectedContextualActionIdx = 0;
		}
	}

	public void AdvanceSelectedContextualAction() {
		if (availableContextualActions.Count > 0) {
			selectedContextualActionIdx = selectedContextualActionIdx + 1;
			if (selectedContextualActionIdx >= availableContextualActions.Count) {
				selectedContextualActionIdx = 0;
			}
		} else {
			selectedContextualActionIdx = 0;
		}
		Debug.Log("contextualActions: ");
		foreach(ContextualAction c in availableContextualActions) {
			Debug.Log(c.actionName);
		}
		Debug.Log("selectedContextualActionIdx = "+selectedContextualActionIdx);

	}
	public ContextualAction GetSelectedContextualAction() {
		return availableContextualActions[selectedContextualActionIdx];
	}
	// Handle movement inputs.
	// TODO: Use GetButtonDown instead of GetKeyDown
	void HandleMovementInput() {
		Vector3 newPos = transform.position;
		if (Input.GetKey("w")) {
			movementInput.y = 1;
		}
		else if (Input.GetKey("s")) {
			movementInput.y = -1;
		}
		else {
			movementInput.y = 0;
		}
		if (Input.GetKey("d")) {
			movementInput.x = 1;
		}
		else if (Input.GetKey("a")) {
			movementInput.x = -1;
		}
		else {
			movementInput.x = 0;
		}
	}
	// Handle player action inputs. Currently only attack does anything useful.
	// TODO: Use GetButtonDown instead of GetKeyDown
	void HandleActionInput() {
		switch (GameMaster.Instance.GetGameStatus()) {
			case (Constants.GameState.Play):
				if (Input.GetButtonDown("Attack")) {
          Attack();
        }
        else if (Input.GetButtonDown("Dash")) {
          Dash();
        }
				else if (Input.GetButtonDown("Activate")) {
					if (availableContextualActions.Count > 0) {
						GetSelectedContextualAction().actionToCall();
					} else if (inventory.lastPickedUpItems.Count > 0) {
						inventory.ClearPickedUpItem();
					}
				}
				else if (Input.GetButtonDown("AdvanceSelectedAction")) {
					AdvanceSelectedContextualAction();
				}
				else if (Input.GetButtonDown("Skill1")) {
					if (skill1 != null) {
						skill1.OnActivateTraitPressed();
					}
				}
				else if (Input.GetButtonDown("Skill2")) {
					if (skill2 != null) {
						skill2.OnActivateTraitPressed();
					}
				}
				if (Input.GetButton("Skill1")) {
					if (skill1 != null) {
						skill1.WhileActiveTraitPressed();
					}
				}
				if (Input.GetButton("Skill2")) {
					if (skill2 != null) {
						skill2.WhileActiveTraitPressed();
					}
				}
				if (Input.GetButtonUp("Skill1")) {
					if (skill1 != null) {
						skill1.OnActivateTraitReleased();
					}
				}
				if (Input.GetButtonUp("Skill2")) {
					if (skill2 != null) {
						skill2.OnActivateTraitReleased();
					}
				}
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetCurrentFloor(currentFloor == FloorLayer.F1 ? FloorLayer.F2 : FloorLayer.F1);
        }
        break;
			case (Constants.GameState.Menu):
				break;
		}

	}

    public void AddInteractable(Interactable interactable) {
		if (!interactables.Contains(interactable.gameObject)) {
			interactables.Add(interactable.gameObject);
		}
	}

	public void RemoveInteractable(Interactable interactable) {
		interactable.OnRemove();
		interactables.Remove(interactable.gameObject);
	}

	public void AddToInventory(PickupItem item) {
		inventory.AddToInventory(item);
	}

	public override void SetCurrentFloor(FloorLayer newFloorLayer) {
		Debug.Log("should be changing to floor "+newFloorLayer);
		// GridManager.Instance.OnLayerChange(newFloorLayer);
		base.SetCurrentFloor(newFloorLayer);
	}

	public void RemoveFromInventory(string itemId, int quantity) {
		inventory.RemoveFromInventory(itemId, quantity);
	}

	public void EquipConsumableToSlot(string itemToEquip, int slot) {
		inventory.EquipConsumableToSlot(itemToEquip, slot);
	}

  private void AssignTraitsForFirstLife() {
    Debug.Log("assigning traits for first life~");
    TraitSlotToTraitDictionary et = initialEquippedTraits.EquippedTraits();
    foreach(TraitSlot traitSlot in et.Keys) {
      Trait trait = et[traitSlot];
      if (et[traitSlot] != null) {
        Debug.Log("added "+trait);
        trait.OnTraitAdded(this);
      }
      equippedTraits[traitSlot] = trait;
    }
	  ActiveTraitInstance activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    ActiveTrait activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+initialskill1) as ActiveTrait;
    if (activeTrait != null && activeTraitData != null) {
      skill1 = activeTrait;
      skill1.Init(this, activeTraitData);
    }
    activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+initialskill2) as ActiveTrait;
    if (activeTrait != null && activeTraitData != null) {
      skill2 = activeTrait;
      skill2.Init(this, activeTraitData);
    }
  }

  public void EquipTrait(InventoryEntry itemToEquip, TraitSlot slot) {
    inventory.EquipTraitToUpcomingLifeTrait(itemToEquip, slot);
  }

	override public void AssignTraitsForNextLife(TraitSlotToUpcomingTraitDictionary nextLifeTraits) {
    foreach (TraitSlot slot in nextLifeTraits.Keys) {
      if (nextLifeTraits[slot] != null && nextLifeTraits[slot].trait != null) {
        equippedTraits[slot] = nextLifeTraits[slot].trait;
        nextLifeTraits[slot].trait.OnTraitAdded(this);
      }
		}
    LymphTypeToIntDictionary lymphTypeCounts = inventory.GetLymphTypeCounts(nextLifeTraits);
    bool primarySkillInited = false;
    foreach(LymphType type in lymphTypeCounts.Keys) {
      if (type == LymphType.None) { continue; }
      if (lymphTypeCounts[type] >= 2) {
        if (!primarySkillInited) {
          if (skill1 == null) {
            skill1 = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
          }
          skill1.Init(this, GameMaster.Instance.lymphTypeToSkillsMapping[type].GetPrimarySkill());
          primarySkillInited = true;
        } else {
          if (skill2 == null) {
            skill2 = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
          }
          skill2.Init(this, GameMaster.Instance.lymphTypeToSkillsMapping[type].GetPrimarySkill());
        }
      }
      if (lymphTypeCounts[type] >= 4) {
        if (skill2 == null) {
          skill2 = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
        }
        skill2.Init(this, GameMaster.Instance.lymphTypeToSkillsMapping[type].GetSecondarySkill());
      }
    }
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

	override public void Die(){
    foreach (TraitSlot slot in equippedTraits.Keys) {
      if (equippedTraits[slot] != null && equippedTraits[slot]) {
        Debug.Log("Removing trait in"+slot);
        equippedTraits[slot].OnTraitRemoved(this);
      }
		}
    Debug.Log("all traits removed; killing player");
		GameMaster.Instance.KillPlayer(inventory.GetUpcomingPupa());
    Debug.Log("destroying gameobject");
		base.Die();
	}

}
