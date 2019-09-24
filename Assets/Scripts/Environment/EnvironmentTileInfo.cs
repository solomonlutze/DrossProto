using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnvironmentTileInfo
{
  public TileLocation tileLocation;
  public EnvironmentTile groundTileType;
  public EnvironmentTile objectTileType;
  public List<TileTag> groundTileTags;
  public List<TileTag> objectTileTags;
  public bool isInteractable = false;
  public bool dealsDamage = false;
  public bool corroded = false;
  public DamageInfo environmentalDamage;
  public void Init(TileLocation location, EnvironmentTile groundTile, EnvironmentTile objectTile)
  {
    tileLocation = location;
    groundTileType = groundTile;
    objectTileType = objectTile;
    groundTileTags = new List<TileTag>();
    objectTileTags = new List<TileTag>();
    dealsDamage = false;
    environmentalDamage = null;
    if (groundTileType != null)
    {
      isInteractable |= groundTileType.IsInteractable();
      groundTileTags.AddRange(groundTileType.tileTags);
      if (groundTileType.dealsDamage)
      {
        dealsDamage = true;
        environmentalDamage = groundTileType.environmentalDamage;
      }
    }
    if (objectTileType != null)
    {
      isInteractable |= objectTileType.IsInteractable();
      objectTileTags.AddRange(objectTileType.tileTags);
      if (objectTileType.dealsDamage)
      {
        dealsDamage = true;
        // TODO: Stack floor/object damage? Currently overwrites floor with object damage
        environmentalDamage = objectTileType.environmentalDamage;
      }
    }
  }

  // for now, groundTiles should never change floor layer, but, y'know
  public bool ChangesFloorLayer()
  {
    return (objectTileType && objectTileType.changesFloorLayer)
        || (groundTileType && groundTileType.changesFloorLayer);
  }
  public FloorLayer GetTargetFloorLayer(FloorLayer currentFloor)
  {
    int currentFloorAsInt = (int)currentFloor;
    int targetFloorLayerAsInt = currentFloorAsInt;
    if (objectTileType != null && objectTileType.changesFloorLayer)
    {
      targetFloorLayerAsInt = objectTileType.changesFloorLayerByAmount + currentFloorAsInt;
    }
    else if (groundTileType != null && groundTileType.changesFloorLayer)
    {
      targetFloorLayerAsInt = groundTileType.changesFloorLayerByAmount + currentFloorAsInt;
    }
    else
    {
      Debug.LogError("tried to get target floor layer from tileInfo that does not change floors?");
      return FloorLayer.B6;
    }
    if (targetFloorLayerAsInt < 0 || targetFloorLayerAsInt > (int)FloorLayer.F6)
    {
      Debug.LogError("tried to change to a floor that doesn't exist?");
      return FloorLayer.B6;
    }
    return (FloorLayer)targetFloorLayerAsInt;
  }

  public float GetAccelerationMod()
  {
    return objectTileType ? objectTileType.accelerationMod :
        groundTileType ? groundTileType.accelerationMod :
        0;
  }

  // TODO: Right now, only ground tiles can respawn players. Generalizing this would be best.
  public bool CanRespawnPlayer()
  {
    return groundTileType != null && groundTileType.shouldRespawnPlayer;
  }

  public bool CharacterCanCrossTile(List<CharacterMovementAbility> characterMovementAbilities)
  {
    if (!CanRespawnPlayer())
    {
      return true;
    }
    foreach (CharacterMovementAbility characterAbility in characterMovementAbilities)
    {
      if (groundTileType != null &&
          groundTileType.movementAbilitiesWhichBypassRespawn.Contains(characterAbility))
      {
        return true;
      }
    }
    return false;
  }

  public void TakeDamage(DamageInfo damageObj)
  {
    if (!objectTileType) { return; }
    if (damageObj.corrosive && objectTileType.corrodable)
    {
      corroded = true;
      GridManager.Instance.MarkTileToDestroyOnPlayerRespawn(this, objectTileType.replacedByWhenCorroded);
    }
    if (damageObj.durabilityDamageLevel >= objectTileType.tileDurability)
    {
      DestroyTile();
    }

  }

  // TODO: this should eventually be based on whether it _will_ respawn player/deal damage
  public bool IsClimbable()
  {
    return objectTileType != null &&
            objectTileType.isClimbable &&
            !dealsDamage &&
            !CanRespawnPlayer();
  }

  // TODO: this should eventually be based on whether it _will_ respawn player/deal damage
  public bool CanBeStuckTo()
  {
    return !IsEmpty() && !CanRespawnPlayer() && !dealsDamage;
  }

  public bool IsEmpty()
  {
    return (groundTileType == null && objectTileType == null);
  }
  public void DestroyTile()
  {
    GridManager.Instance.ReplaceTileAtLocation(tileLocation, null);
  }

  public Tile.ColliderType GetColliderType()
  {
    if (!objectTileType) { return Tile.ColliderType.None; }
    else
    {
      return objectTileType.colliderType;
    }
  }

  public string GetInteractableText(PlayerController pc)
  {
    if (ChangesFloorLayer())
    {
      if (GetTargetFloorLayer(pc.currentFloor) > pc.currentFloor)
      {
        return "ascend";
      }
      else
      {
        return "descend";
      }
    }
    else
    {
      Debug.LogWarning("Tried to get interactable text for a non-interactable tile");
      return "";
    }
  }

}