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

	private ActiveTraitInstance activeTrait1;
	private ActiveTraitInstance activeTrait2;
	private TileLocation lastSafeTileLocation;

	public string lastActivatedTrait = null;
	public List<ContextualAction> availableContextualActions;
	private int selectedContextualActionIdx;
	// Use this for initialization
	override protected void Start () {
		base.Start();
	}

	public void Init(bool initialSpawn, UpcomingLifeTraits previousLarva, UpcomingLifeTraits previousPupa) {
		base.Init();
		availableContextualActions = new List<ContextualAction>();
		interactables = new List<GameObject>();
		inventory = GetComponent<Inventory>();
		inventory.owner = this;
    if (initialSpawn) {
      AssignTraitsForFirstLife();
    } else {
  		inventory.AdvanceUpcomingLifeTraits(previousLarva, previousPupa);
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
		activeTrait1.CancelActiveEffects();
		activeTrait2.CancelActiveEffects();
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
			if (!tile.CharacterCanCrossTile(movementAbilities)) {
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
			Debug.Log("collided with "+tile);
			if (tile.tileTags.Contains(TileTag.Ground) && movementAbilities.Contains(CharacterMovementAbility.Burrow)) {
				tile.DestroyTile();
			}
		}
	}

	private void ClimbAdjacentTile() {
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
		if (movementAbilities.Contains(CharacterMovementAbility.StickyFeet) && GridManager.Instance.CanClimbAdjacentTile(transform.position, currentFloor)) {
			AddContextualAction("climb", ClimbAdjacentTile);
		}
		if (interactables.Count > 0) {
			foreach (GameObject interactableObject in interactables) {
				Interactable interactable = interactableObject.GetComponent<Interactable>();
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
				if (Input.GetKeyDown("space")) {
						Attack();
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
				else if (Input.GetButtonDown("ActiveTrait1")) {
					if (activeTrait1 != null) {
						activeTrait1.OnActivateTraitPressed();
					}
				}
				else if (Input.GetButtonDown("ActiveTrait2")) {
					if (activeTrait2 != null) {
						activeTrait2.OnActivateTraitPressed();
					}
				}
				if (Input.GetButton("ActiveTrait1")) {
					if (activeTrait1 != null) {
						activeTrait1.WhileActiveTraitPressed();
					}
				}
				if (Input.GetButton("ActiveTrait2")) {
					if (activeTrait2 != null) {
						activeTrait2.WhileActiveTraitPressed();
					}
				}
				if (Input.GetButtonUp("ActiveTrait1")) {
					if (activeTrait1 != null) {
						activeTrait1.OnActivateTraitReleased();
					}
				}
				if (Input.GetButtonUp("ActiveTrait2")) {
					if (activeTrait2 != null) {
						activeTrait2.OnActivateTraitReleased();
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

	public void EquipWeapon(InventoryEntry weaponToEquip) {
		if (weapon != null) {
			inventory.MarkItemUnequipped(equippedWeaponId);
			Destroy(weapon.gameObject);
		}
		inventory.MarkItemEquipped(weaponToEquip.itemId);
		InitializeWeapon(weaponToEquip.itemId, true);
	}

	public void EquipConsumableToSlot(string itemToEquip, int slot) {
		inventory.EquipConsumableToSlot(itemToEquip, slot);
	}

	public void EquipTrait(InventoryEntry itemToEquip, int slot, UpcomingLifeTraits traitsToEquipTo, TraitType type) {
		inventory.EquipTraitToUpcomingLifeTrait(itemToEquip, slot, traitsToEquipTo, type);
	}

  private void AssignTraitsForFirstLife() {
    TraitSlotToTraitDictionary et = initialEquippedTraits.EquippedTraits();
    foreach(TraitSlot traitSlot in et.Keys) {
      PassiveTrait trait = et[traitSlot];
      if (et[traitSlot] != null) {
        Debug.Log("added "+trait);
        trait.OnTraitAdded(this);
      }
      equippedTraits[traitSlot] = trait;
    }

	ActiveTraitInstance activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
			ActiveTrait activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+initialActiveTrait1) as ActiveTrait;
			if (activeTrait != null && activeTraitData != null) {
				activeTrait1 = activeTrait;
				activeTrait1.Init(this, activeTraitData);
			} else {
				Debug.LogError("couldn't load ActiveTraitInstance or ActiveTraitData for "+initialActiveTrait1);
			}
			activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
			activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+initialActiveTrait2) as ActiveTrait;
			if (activeTrait != null && activeTraitData != null) {
				activeTrait2 = activeTrait;
				activeTrait2.Init(this, activeTraitData);
			}
  }

	override public void AssignTraitsForNextLife(UpcomingLifeTraits nextLifeTraits) {
		string[] initialTraitNames = initialEquippedTraits.AllPopulatedTraitNames;
    PassiveTrait passiveTrait;
    foreach (UpcomingLifeTrait trait in nextLifeTraits.passiveTraits) {
      if (trait != null) {
        passiveTrait = Resources.Load("Data/TraitData/PassiveTraits/"+trait.traitName) as PassiveTrait;
        if (passiveTrait != null) { passiveTrait.OnTraitAdded(this); }
      }
		}
    ActiveTraitInstance activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    ActiveTrait activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+nextLifeTraits.activeTraits[0].traitName) as ActiveTrait;
    if (activeTrait != null) {
      activeTrait1 = activeTrait;
      activeTrait1.Init(this, activeTraitData);
    }
    activeTrait = gameObject.AddComponent(Type.GetType("ActiveTraitInstance")) as ActiveTraitInstance;
    activeTraitData = Resources.Load("Data/TraitData/ActiveTraits/"+nextLifeTraits.activeTraits[1].traitName) as ActiveTrait;
    if (activeTrait != null) {
      activeTrait2 = activeTrait;
      activeTrait2.Init(this, activeTraitData);
    }
	}

	override public void Die(){
		GameMaster.Instance.KillPlayer(inventory.GetUpcomingLarva(), inventory.GetUpcomingPupa());
		base.Die();
	}

}
