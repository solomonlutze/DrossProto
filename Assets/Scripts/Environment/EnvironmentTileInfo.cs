using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnvironmentTileInfo {
    public TileLocation tileLocation;
    public EnvironmentTile groundTileType;
    public EnvironmentTile objectTileType;
    public List<TileTag> tileTags;
    public bool isInteractable = false;
    public bool dealsDamage = false;
    public DamageObject environmentalDamage;
    public void Init(TileLocation location, EnvironmentTile groundTile, EnvironmentTile objectTile) {
        tileLocation = location;
        groundTileType = groundTile;
        objectTileType = objectTile;
        tileTags = new List<TileTag>();
        dealsDamage = false;
        environmentalDamage = null;
        if (groundTileType != null) {
            isInteractable |= groundTileType.IsInteractable();
            tileTags.AddRange(groundTileType.tileTags);
            if (groundTileType.dealsDamage) {
                dealsDamage = true;
                environmentalDamage = groundTileType.environmentalDamage;
            }
        }
        if (objectTileType != null) {
            isInteractable |= objectTileType.IsInteractable();
            tileTags.AddRange(objectTileType.tileTags);
            if (objectTileType.dealsDamage) {
                dealsDamage = true;
                // TODO: Stack floor/object damage? Currently overwrites floor with object damage
                environmentalDamage = objectTileType.environmentalDamage;
            }
        }
    }

    // for now, groundTiles should never change floor layer, but, y'know
    public bool ChangesFloorLayer() {
        return (objectTileType && objectTileType.changesFloorLayer)
            || (groundTileType && groundTileType.changesFloorLayer);
    }
    public FloorLayer GetTargetFloorLayer() {
        if (objectTileType != null && objectTileType.changesFloorLayer) {
            return objectTileType.targetFloorLayer;
        } else if (groundTileType != null && groundTileType.changesFloorLayer) {
            return groundTileType.targetFloorLayer;
        } else {
            Debug.LogError("tried to get target floor layer from tileInfo that does not change floors?");
            return FloorLayer.B6;
        }
    }

    public float GetAccelerationMod() {
        return objectTileType ? objectTileType.accelerationMod :
            groundTileType ? groundTileType.accelerationMod :
            0;
    }

    // TODO: Right now, only ground tiles can respawn players. Generalizing this would be best.
    public bool CanRespawnPlayer() {
        return groundTileType != null && groundTileType.shouldRespawnPlayer;
    }

    public bool CharacterCanCrossTile(List<CharacterMovementAbility> characterMovementAbilities) {
        if (!CanRespawnPlayer()) {
            return true;
        }
        foreach(CharacterMovementAbility characterAbility in characterMovementAbilities) {
            if (groundTileType != null &&
                groundTileType.movementAbilitiesWhichBypassRespawn.Contains(characterAbility))
                {
                    return true;
                }
        }
        return false;
    }

    public void TakeDamage(DamageObject damageObj) {
        if (!objectTileType) { return; }
        if (damageObj.durabilityDamageLevel > objectTileType.tileDurability) {
           DestroyTile();
        }
    }

    // TODO: this should eventually be based on whether it _will_ respawn player/deal damage
    public bool IsClimbable() {
        return  objectTileType != null &&
                objectTileType.isClimbable &&
                !dealsDamage &&
                !CanRespawnPlayer();
    }

    // TODO: this should eventually be based on whether it _will_ respawn player/deal damage
    public bool CanBeStuckTo() {
        return !IsEmpty() && !CanRespawnPlayer() && !dealsDamage;
    }

	public bool IsEmpty() {
		return (groundTileType == null && objectTileType == null);
	}
    public void DestroyTile() {
        GridManager.Instance.ReplaceTileAtLocation(tileLocation, null);
    }

    public Tile.ColliderType GetColliderType() {
        if (!objectTileType) { return Tile.ColliderType.None; }
        else {
            return objectTileType.colliderType;
        }
    }

    public string GetInteractableText(PlayerController pc) {
        if (ChangesFloorLayer()) {
            if (GetTargetFloorLayer() > pc.currentFloor) {
                return "ascend";
            } else {
                return "descend";
            }
        }
        else {
            Debug.LogWarning("Tried to get interactable text for a non-interactable tile");
            return "";
        }
    }
}