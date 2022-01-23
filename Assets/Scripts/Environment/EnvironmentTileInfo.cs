using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class IlluminationInfo
{
  public LightRangeInfo sourceLightInfo;
  public float illuminationLevel;
  public Color visibleColor;
  public Color opaqueColor;

  public IlluminationInfo(float i, Color c)
  {
    illuminationLevel = i;
    visibleColor = c * i;
    visibleColor.a = (1 - i);
    opaqueColor = c * illuminationLevel;
    opaqueColor.a = 1;
  }

}

public class IlluminatedByInfo
{
  public LightSourceInfo illuminationSource;
  public int distanceFromSource;

  public LightRangeInfo sourceRangeInfo
  {
    get
    {
      return illuminationSource.lightRangeInfos[distanceFromSource];
    }
  }
  public bool sunlit
  {
    get
    {
      return illuminationSource != null && illuminationSource.isSunlight && distanceFromSource == 0;
    }
  }

  public IlluminatedByInfo(LightSourceInfo source, int distance)
  {
    illuminationSource = source;
    distanceFromSource = distance;
  }
}

[System.Serializable]
public class LightRangeInfo
{
  public float defaultIntensity;
  public float currentIntensity;
}

[System.Serializable]
public class LightSourceInfo
{

  [Tooltip("illumination level of neighboring tiles. index = neighbor distance (0 is self)")]
  public LightRangeInfo[] lightRangeInfos;
  public Color illuminationColor;
  public LightPattern lightPattern;
  [Range(0, 1)]
  public float patternVariation;
  public int smoothing;
  public Queue<float> smoothingQueue;
  public float smoothingSum = 0;
  public bool isSunlight;
}

public class EnvironmentTileInfo
{
  public TileLocation tileLocation;
  public EnvironmentTile groundTileType;
  public EnvironmentTile objectTileType;
  public InfoTile infoTileType;
  public List<TileTag> groundTileTags
  {
    get
    {
      return groundTileType.tileTags;
    }
  }
  public List<TileTag> objectTileTags
  {
    get
    {
      return objectTileType.tileTags;
    }
  }

  public List<EnvironmentalDamage> environmentalDamageSources;
  public bool dealsDamage
  {
    get
    {
      return (groundTileType != null && groundTileType.dealsDamage)
    || (objectTileType != null && objectTileType.dealsDamage);
    }
  }

  public bool IsEnvironmentalDamageSourceWarmup
  {
    get
    {
      foreach (EnvironmentalDamage damage in environmentalDamageSources)
      {
        if (damage.IsEnvironmentalDamageSourceWarmup())
        {
          return true;
        }
      }
      return false;
    }
  }

  public bool IsEnvironmentalDamageSourceActive
  {
    get
    {
      foreach (EnvironmentalDamage damage in environmentalDamageSources)
      {
        if (damage.IsEnvironmentalDamageSourceActive())
        {
          return true;
        }
      }
      return false;
    }
  }
  public EnvironmentTileData tileData;

  public IlluminationInfo illuminationInfo;

  public void Init(TileLocation location, EnvironmentTile groundTile, EnvironmentTile objectTile, EnvironmentTile waterTile, InfoTile infoTile, EnvironmentTileData etd)
  {
    tileLocation = location;
    groundTileType = groundTile;
    objectTileType = objectTile;
    if (waterTile != null)
    {
      groundTileType = waterTile;
    }
    infoTileType = infoTile;
    tileData = etd;
    environmentalDamageSources = new List<EnvironmentalDamage>();
    foreach (EnvironmentTile t in new EnvironmentTile[] { groundTile, objectTile })
      if (t != null && t.dealsDamage)
      {
        EnvironmentalDamage d = new EnvironmentalDamage();
        d.Init(t);
        environmentalDamageSources.Add(d);
      }
  }

  // for now, groundTiles should never change floor layer, but, y'know
  public bool ChangesFloorLayer()
  {
    return AscendsOrDescends() != AscendingDescendingState.None;
  }

  public AscendingDescendingState AscendsOrDescends()
  {
    if (objectTileType && objectTileType.changesFloorLayer != AscendingDescendingState.None)
    {
      return objectTileType.changesFloorLayer;
    }
    if (groundTileType && groundTileType.changesFloorLayer != AscendingDescendingState.None)
    {
      return groundTileType.changesFloorLayer;
    }
    return AscendingDescendingState.None;
  }

  public FloorLayer GetTargetFloorLayer(FloorLayer currentFloor)
  {
    int currentFloorAsInt = (int)currentFloor;
    int targetFloorLayerAsInt = currentFloorAsInt + (int)AscendsOrDescends();
    if (targetFloorLayerAsInt == currentFloorAsInt)
    {
      Debug.LogError("tried to get target floor layer from tileInfo that does not change floors?");
      return currentFloor;
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

  public bool HasTileTag(TileTag tag)
  {
    return (groundTileTags.Contains(tag) || objectTileTags.Contains(tag));
  }

  public List<CharacterMovementAbility> GetMovementAbilitiesWhichBypassRespawn()
  {
    return groundTileType.movementAbilitiesWhichBypassRespawn;
  }
  public bool CharacterCanCrossTile(Character character)
  {
    if (!CanRespawnPlayer())
    {
      return true;
    }
    if (character.IsMidair())
    {
      return true;
    }
    foreach (CharacterMovementAbility movementAbility in GetMovementAbilitiesWhichBypassRespawn())
    {
      if (character.HasMovementAbility(movementAbility))
      {
        return true;
      }
    }
    // foreach (CharacterAttribute attribute in groundTileType.attributesWhichBypassRespawn.Keys)
    // {
    //   if (character.GetAttribute(attribute) > 0
    //       && character.GetAttribute(attribute) >= groundTileType.attributesWhichBypassRespawn[attribute]
    //   )
    //   {
    //     return true;
    //   }
    // }
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

  public float GroundHeight()
  {
    return tileData.groundHeight;
  }

  public float CeilingHeight()
  {
    return tileData.ceilingHeight;
  }

  public TileParticleSystem GetTileParticleSystem()
  {
    if (objectTileType != null && objectTileType.tileParticleSystem != null)
    {
      return objectTileType.tileParticleSystem;
    }
    if (groundTileType != null && groundTileType.tileParticleSystem != null)
    {
      return groundTileType.tileParticleSystem;
    }
    return null;
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


  public bool HasSolidObject()
  {
    return (objectTileType != null && objectTileType.colliderType != Tile.ColliderType.None);
  }

  public bool IsEmpty()
  {
    return (groundTileType == null && objectTileType == null);
  }

  // public bool IsSunlit()
  // {
  //   for (int i = 0; i < illuminatedBySources.Count; i++)
  //   {
  //     if (illuminatedBySources[i].sunlit)
  //     {
  //       return true;
  //     }
  //   }
  //   return false;
  // }

  // public bool IsEmptyAndSunlit()
  // {
  //   return IsEmpty() && IsSunlit();
  // }

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
    return groundTileType ? groundTileType.EmitFootstepParticles(character) : 0;
  }

  public string GetInteractableText(PlayerController pc)
  {
    if (ChangesFloorLayer())
    {
      if (AscendsOrDescends() == AscendingDescendingState.Ascending)
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
      // case TilemapDirection.Up:
      //   return cornerInterestObjects[TilemapCorner.UpperLeft] == null && cornerInterestObjects[TilemapCorner.UpperRight] == null
      //     && borderTile.cornerInterestObjects[TilemapCorner.LowerLeft] == null && borderTile.cornerInterestObjects[TilemapCorner.LowerRight] == null;
      // case TilemapDirection.Left:
      //   return cornerInterestObjects[TilemapCorner.LowerLeft] == null && cornerInterestObjects[TilemapCorner.UpperLeft] == null
      //     && borderTile.cornerInterestObjects[TilemapCorner.LowerRight] == null && borderTile.cornerInterestObjects[TilemapCorner.UpperRight] == null;
      // case TilemapDirection.Right:
      //   return cornerInterestObjects[TilemapCorner.UpperRight] == null && cornerInterestObjects[TilemapCorner.LowerRight] == null
      //     && borderTile.cornerInterestObjects[TilemapCorner.UpperLeft] == null && borderTile.cornerInterestObjects[TilemapCorner.LowerLeft] == null;
      // case TilemapDirection.Down:
      //   return cornerInterestObjects[TilemapCorner.LowerLeft] == null && cornerInterestObjects[TilemapCorner.LowerRight] == null
      //     && borderTile.cornerInterestObjects[TilemapCorner.UpperLeft] == null && borderTile.cornerInterestObjects[TilemapCorner.UpperRight] == null;
      default:
        return false;
    }
  }

  // public void AddIlluminatedBySource(LightSourceInfo source, int distance)
  // {
  //   illuminatedBySources.Add(new IlluminatedByInfo(source, distance));
  //   RecalculateIllumination();
  // }


  // public void RecalculateIllumination()
  // {
  //   Color finalColor = Color.black;
  //   float totalIntensity = 0;
  //   float maxIntensity = .001f;
  //   for (int i = 0; i < illuminatedBySources.Count; i++)
  //   {
  //     totalIntensity += illuminatedBySources[i].sourceRangeInfo.currentIntensity;
  //     maxIntensity = Mathf.Max(maxIntensity, illuminatedBySources[i].sourceRangeInfo.currentIntensity);
  //   }
  //   for (int i = 0; i < illuminatedBySources.Count; i++)
  //   {
  //     finalColor += (illuminatedBySources[i].illuminationSource.illuminationColor * (illuminatedBySources[i].sourceRangeInfo.currentIntensity) / totalIntensity);
  //   }
  //   illuminationInfo = new IlluminationInfo(maxIntensity, finalColor);
  //   if (wallObject != null)
  //   {
  //     // wallObject.ChangeColor(finalColor);
  //   }
  // }

  // doing illumination this way could mean we change tile colors multiple times a frame in cases of overlapping lights :o
  // worth noting if perf gets shitty!!
  // public HashSet<EnvironmentTileInfo> IlluminateNeighbors()
  // {
  //   switch (lightSource.lightPattern)
  //   {
  //     case LightPattern.Flicker:
  //       if (lightSource.smoothingQueue == null)
  //       {
  //         Debug.Log("smoothing queue null?");
  //         return null;
  //       }
  //       while (lightSource.smoothingQueue.Count >= lightSource.smoothing)
  //       {
  //         lightSource.smoothingSum -= lightSource.smoothingQueue.Dequeue();
  //       }
  //       float newValue = UnityEngine.Random.Range(-lightSource.patternVariation, lightSource.patternVariation);
  //       lightSource.smoothingQueue.Enqueue(newValue);
  //       lightSource.smoothingSum += newValue;
  //       float intensityModifier = lightSource.smoothingSum / (float)lightSource.smoothingQueue.Count;
  //       for (int i = 0; i < lightSource.lightRangeInfos.Length; i++)
  //       {
  //         float temp = lightSource.lightRangeInfos[i].defaultIntensity + intensityModifier;
  //         lightSource.lightRangeInfos[i].currentIntensity = Mathf.Clamp(
  //           temp,
  //           0,
  //           1
  //         );

  //       }
  //       return illuminatedNeighbors;
  //     case LightPattern.Constant:
  //     default:
  //       return null;
  //   }
  // }
}