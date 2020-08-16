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
  public List<EnvironmentalDamage> environmentalDamageSources;

  public Dictionary<TilemapCorner, GameObject> cornerInterestObjects;
  // public DamageData_OLD environmentalDamage_OLD;
  public void Init(TileLocation location, EnvironmentTile groundTile, EnvironmentTile objectTile)
  {
    tileLocation = location;
    groundTileType = groundTile;
    objectTileType = objectTile;
    groundTileTags = new List<TileTag>();
    objectTileTags = new List<TileTag>();
    dealsDamage = false;
    environmentalDamageSources = new List<EnvironmentalDamage>();
    cornerInterestObjects = new Dictionary<TilemapCorner, GameObject>() {
      {TilemapCorner.UpperLeft, null},
      {TilemapCorner.LowerLeft, null},
      {TilemapCorner.UpperRight, null},
      {TilemapCorner.LowerRight, null},
    };
    if (groundTileType != null)
    {
      isInteractable |= groundTileType.IsInteractable();
      groundTileTags.AddRange(groundTileType.tileTags);
      if (groundTileType.dealsDamage)
      {
        dealsDamage = true;
        EnvironmentalDamage d = new EnvironmentalDamage();
        d.Init(groundTileType);
        environmentalDamageSources.Add(d);
      }
    }
    if (objectTileType != null)
    {
      isInteractable |= objectTileType.IsInteractable();
      objectTileTags.AddRange(objectTileType.tileTags);
      if (objectTileType.dealsDamage)
      {
        dealsDamage = true;
        EnvironmentalDamage d = new EnvironmentalDamage();
        d.Init(objectTileType);
        environmentalDamageSources.Add(d);
      }
    }
  }

  // for now, groundTiles should never change floor layer, but, y'know
  public bool ChangesFloorLayer()
  {
    return (objectTileType && objectTileType.changesFloorLayer_old)
        || (groundTileType && groundTileType.changesFloorLayer_old);
  }
  public FloorLayer GetTargetFloorLayer(FloorLayer currentFloor)
  {
    int currentFloorAsInt = (int)currentFloor;
    int targetFloorLayerAsInt = currentFloorAsInt;
    if (objectTileType != null && objectTileType.changesFloorLayer_old)
    {
      targetFloorLayerAsInt = objectTileType.changesFloorLayerByAmount + currentFloorAsInt;
    }
    else if (groundTileType != null && groundTileType.changesFloorLayer_old)
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

  public bool CharacterCanCrossTile(Character character)
  {
    if (!CanRespawnPlayer())
    {
      return true;
    }
    foreach (CharacterAttribute attribute in groundTileType.attributesWhichBypassRespawn.Keys)
    {
      if (character.GetAttribute(attribute) > 0
          && character.GetAttribute(attribute) >= groundTileType.attributesWhichBypassRespawn[attribute]
      )
      {
        return true;
      }
    }
    return false;
  }

  // ONLY for deciding if a tile can physically accommodate you.
  // NOT for deciding if you would want to be there (e.g. damaging, respawning, etc)
  public bool CharacterCanOccupyTile(Character c)
  {
    return objectTileType == null
    || GetColliderType() == Tile.ColliderType.None
    || CharacterCanBurrowThroughObjectTile(c);
  }

  // Used to determine if character can pass through otherwise-impassible block
  public bool CharacterCanBurrowThroughObjectTile(Character character)
  {
    if (objectTileType == null) { return false; }
    foreach (CharacterAttribute attribute in objectTileType.attributesWhichAllowBurrowing.Keys)
    {
      if (character.GetAttribute(attribute) > 0
          && character.GetAttribute(attribute) >= objectTileType.attributesWhichAllowBurrowing[attribute]
      )
      {
        return true;
      }
    }
    return false;
  }

  // Used to determine if character can ascend through floor tile above them,
  // or descend through floor tile they're currently on
  public bool CharacterCanPassThroughFloorTile(Character character)
  {
    if (groundTileType == null) { return false; }
    foreach (CharacterAttribute attribute in groundTileType.attributesWhichAllowPassingThrough.Keys)
    {
      if (character.GetAttribute(attribute) > 0
          && character.GetAttribute(attribute) >= groundTileType.attributesWhichAllowPassingThrough[attribute]
      )
      {
        return true;
      }
    }
    return false;
  }
  // public bool CharacterCanCrossTile_OLD(List<CharacterMovementAbility> characterMovementAbilities)
  // {
  //     if (!CanRespawnPlayer())
  //     {
  //         return true;
  //     }
  //     foreach ( characterAbility in characterMovementAbilities)
  //     {
  //         if (groundTileType != null &&
  //             groundTileType.movementAbilitiesWhichBypassRespawn.Contains(characterAbility))
  //         {
  //             return true;
  //         }
  //     }
  //     return false;
  // }

  public void TakeDamage(Damage_OLD damage)
  {
    if (!objectTileType) { return; }
    if (damage.IsCorrosive() && objectTileType.corrodable)
    {
      corroded = true;
      GridManager.Instance.MarkTileToDestroyOnPlayerRespawn(this, objectTileType.replacedByWhenCorroded);
    }
    if (damage.GetDurabilityDamageLevel() >= objectTileType.tileDurability)
    {
      DestroyTile();
    }

  }

  // TODO: this should eventually be based on whether it _will_ respawn player/deal damage
  public bool IsClimbable()
  {
    return objectTileType != null &&
            objectTileType.attributesWhichAllowClimbing.Keys.Count > 0 &&
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

  // DEPRECATED
  public void DestroyTile()
  {
    GridManager.Instance.ReplaceTileAtLocation(tileLocation, null);
  }

  public void DestroyObjectTile()
  {
    GridManager.Instance.DestroyObjectTileAtLocation(tileLocation);
  }

  public Tile.ColliderType GetColliderType()
  {
    if (!objectTileType) { return Tile.ColliderType.None; }
    else
    {
      return objectTileType.colliderType;
    }
  }

  public float HandleFootstep(Character character)
  {
    float objectResult = objectTileType ? objectTileType.EmitFootstepParticles(character) : 0;
    if (objectResult > 0)
    {
      return objectResult;
    }
    return groundTileType.EmitFootstepParticles(character);
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

  public int GetBorderInterestObjectPriority()
  {
    return groundTileType != null ? groundTileType.interestObjectPriority : 0;
  }

  public bool AcceptsInterestObjects()
  {
    return groundTileType != null && groundTileType.acceptsInterestObjects;
  }
  public GameObject GetBorderInterestObject()
  {
    return groundTileType != null && groundTileType.borderInterestObjects != null && groundTileType.borderInterestObjects.Length > 0 ?
      groundTileType.borderInterestObjects[UnityEngine.Random.Range(0, groundTileType.borderInterestObjects.Length)]
      : null;
  }

  public GameObject GetCornerInterestObject(EnvironmentTileInfo otherTile, EnvironmentTileInfo destinationTile)
  {
    if (objectTileType == otherTile.objectTileType && objectTileType != destinationTile.objectTileType && objectTileType != null && objectTileType.cornerInterestObjects.Length > 0)
    {
      return objectTileType.cornerInterestObjects[UnityEngine.Random.Range(0, objectTileType.cornerInterestObjects.Length)];
    }
    else if (groundTileType == otherTile.groundTileType && groundTileType != destinationTile.groundTileType && groundTileType != null && groundTileType.cornerInterestObjects.Length > 0)
    {
      return groundTileType.cornerInterestObjects[UnityEngine.Random.Range(0, groundTileType.cornerInterestObjects.Length)];
    }
    return null;
  }

  public bool IsBorderClear(TilemapDirection direction, EnvironmentTileInfo borderTile)
  {
    switch (direction)
    {
      case TilemapDirection.Up:
        return cornerInterestObjects[TilemapCorner.UpperLeft] == null && cornerInterestObjects[TilemapCorner.UpperRight] == null
          && borderTile.cornerInterestObjects[TilemapCorner.LowerLeft] == null && borderTile.cornerInterestObjects[TilemapCorner.LowerRight] == null;
      case TilemapDirection.Left:
        return cornerInterestObjects[TilemapCorner.LowerLeft] == null && cornerInterestObjects[TilemapCorner.UpperLeft] == null
          && borderTile.cornerInterestObjects[TilemapCorner.LowerRight] == null && borderTile.cornerInterestObjects[TilemapCorner.UpperRight] == null;
      case TilemapDirection.Right:
        return cornerInterestObjects[TilemapCorner.UpperRight] == null && cornerInterestObjects[TilemapCorner.LowerRight] == null
          && borderTile.cornerInterestObjects[TilemapCorner.UpperLeft] == null && borderTile.cornerInterestObjects[TilemapCorner.LowerLeft] == null;
      case TilemapDirection.Down:
        return cornerInterestObjects[TilemapCorner.LowerLeft] == null && cornerInterestObjects[TilemapCorner.LowerRight] == null
          && borderTile.cornerInterestObjects[TilemapCorner.UpperLeft] == null && borderTile.cornerInterestObjects[TilemapCorner.UpperRight] == null;
      default:
        return false;
    }
  }

}