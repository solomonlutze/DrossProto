﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : Character {

	// Contains info about game state, etc.
	public GridManager gridManager;
	private List<GameObject> interactables;
	private Inventory inventory;

	private PassiveTrait passiveTrait1;
	private PassiveTrait passiveTrait2;

	private ActiveTraitInstance activeTrait1;
	private ActiveTraitInstance activeTrait2;
	private TileLocation lastSafeTileLocation;

	public string lastActivatedTrait = null;
	// Use this for initialization
	override protected void Start () {
		base.Start();
	}

	public void Init(UpcomingLifeTraits previousLarva, UpcomingLifeTraits previousPupa) {
		interactables = new List<GameObject>();
		inventory = GetComponent<Inventory>();
		inventory.owner = this;
		inventory.AdvanceUpcomingLifeTraits(previousLarva, previousPupa);
	}
	// Player specific non-physics biz.

	// TODO: setting orientTowards can prolly be its own function
	override protected void Update () {
		orientTowards = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		base.Update();
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

	protected override void HandleTile() {
		EnvironmentTile tile = GameMaster.Instance.GetTileAtLocation(CalculateCurrentTileLocation().position, currentFloor);
		base.HandleTile();
		if (tile.ShouldRespawnPlayer()) {
			if (!(tile.tileTags.Contains(TileTag.Water) && movementAbilities.Contains(CharacterMovementAbility.WaterStride))) {
				transform.position = lastSafeTileLocation.position;
				if (currentFloor != lastSafeTileLocation.floorLayer) {
					SetCurrentFloor(lastSafeTileLocation.floorLayer);
				}
			}
		} else {
			lastSafeTileLocation = currentTileLocation;
		}
	}

	public override void HandleTileCollision(EnvironmentTile tile, Vector3 loc, Constants.FloorLayer floor) {
		if (tile.colliderType == Tile.ColliderType.None) {
			return;
		}
		else {
			Debug.Log("collided with "+tile);
			if (tile.tileTags.Contains(TileTag.Ground) && movementAbilities.Contains(CharacterMovementAbility.Burrow)) {
				tile.Destroy(loc, floor);
			}
		}
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
				else if (Input.GetKeyDown("e")) {
					//TODO: Sort these by distance or some other priority
					EnvironmentTile tile = GameMaster.Instance.GetTileAtLocation(CalculateCurrentTileLocation().position, currentFloor);
					if (tile.IsInteractable()) {
						UseTile();
					}
					else if (interactables.Count > 0) {
						interactables[0].SendMessage("PlayerActivate", this, SendMessageOptions.DontRequireReceiver);
					} else if (inventory.lastPickedUpItems.Count > 0) {
						inventory.ClearPickedUpItem();
					}

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
                    SetCurrentFloor(currentFloor == Constants.FloorLayer.F1 ? Constants.FloorLayer.F2 : Constants.FloorLayer.F1);
                }
                break;
			case (Constants.GameState.Menu):
				break;
		}

	}

    public void AddInteractable(GameObject interactable) {
		interactables.Add(interactable);
	}

	public void RemoveInteractable(GameObject interactable) {
		interactables.Remove(interactable);
	}

	public void AddToInventory(PickupItem item) {
		inventory.AddToInventory(item);
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

	override public void AssignTraitsForNextLife(UpcomingLifeTraits nextLifeTraits) {
		Debug.Log("upcoming life traits: "+nextLifeTraits);
		if (initialPassiveTrait1 != null || initialPassiveTrait2 != null) {
			PassiveTrait passiveTrait = Resources.Load("Data/TraitData/PassiveTraits/"+initialPassiveTrait1) as PassiveTrait;
			if (passiveTrait != null) { passiveTrait.OnTraitAdded(this); }
			passiveTrait = Resources.Load("Data/TraitData/PassiveTraits/"+initialPassiveTrait2) as PassiveTrait;
			if (passiveTrait != null) { passiveTrait.OnTraitAdded(this); }
			initialPassiveTrait1 = null;
			initialPassiveTrait2 = null;
		} else {
			PassiveTrait passiveTrait;
			foreach (UpcomingLifeTrait trait in nextLifeTraits.passiveTraits) {
				if (trait != null) {
					passiveTrait = Resources.Load("Data/TraitData/PassiveTraits/"+initialPassiveTrait1) as PassiveTrait;
					if (passiveTrait != null) { passiveTrait.OnTraitAdded(this); }
				}
			}
		}
		if (initialActiveTrait1 != null || initialActiveTrait2 != null) {
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
		} else {
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

	}
	// for each of your UpcomingLifeTraits, give your character the relevant abilities
	// override public void AssignTraitsForNextLife(UpcomingLifeTraits nextLifeTraits) {
	// 	Debug.Log("upcoming life traits: "+nextLifeTraits);
	// 	if (initialPassiveTrait1 != null || initialPassiveTrait2 != null) {
	// 			PassiveTraitMono passiveTrait = gameObject.AddComponent(Type.GetType(initialPassiveTrait1)) as PassiveTraitMono;
	// 			if (passiveTrait != null) { passiveTrait.OnTraitAdded(this); }
	// 			passiveTrait = gameObject.AddComponent(Type.GetType(initialPassiveTrait2)) as PassiveTraitMono;
	// 			if (passiveTrait != null) { passiveTrait.OnTraitAdded(this); }
	// 			initialPassiveTrait1 = null;
	// 			initialPassiveTrait2 = null;
	// 	} else {
	// 		foreach (UpcomingLifeTrait trait in nextLifeTraits.passiveTraits) {
	// 			if (trait != null) {
	// 				PassiveTraitMono passiveTrait = gameObject.AddComponent(Type.GetType(trait.traitName)) as PassiveTraitMono;
	// 				passiveTrait.OnTraitAdded(this);
	// 			}
	// 		}
	// 	}
	// 	if (initialActiveTrait1 != null || initialActiveTrait2 != null) {
	// 			ActiveTraitMono activeTrait = gameObject.AddComponent(Type.GetType(initialActiveTrait1)) as ActiveTraitMono;
	// 			if (activeTrait != null) {
	// 				activeTrait1Mono = activeTrait;
	// 			}
	// 			activeTrait = gameObject.AddComponent(Type.GetType(initialActiveTrait2)) as ActiveTraitMono;
	// 			if (activeTrait != null) {
	// 				activeTrait2Mono = activeTrait;
	// 			}
	// 			initialActiveTrait1 = null;
	// 			initialActiveTrait2 = null;
	// 	} else {
	// 		ActiveTraitMono activeTrait;
	// 		activeTrait = gameObject.AddComponent(Type.GetType(nextLifeTraits.activeTraits[0].traitName)) as ActiveTraitMono;
	// 		if (activeTrait != null) {
	// 			activeTrait1Mono = activeTrait;
	// 		}
	// 		activeTrait = gameObject.AddComponent(Type.GetType(nextLifeTraits.activeTraits[1].traitName)) as ActiveTraitMono;
	// 		if (activeTrait != null) {
	// 			activeTrait2Mono = activeTrait;
	// 		}
	// 	}
	// }

	override public void Die(){
		GameMaster.Instance.KillPlayer(inventory.GetUpcomingLarva(), inventory.GetUpcomingPupa());
		base.Die();
	}

}
