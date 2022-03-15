using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TileParticleSystem : MonoBehaviour
{
  EnvironmentTileInfo owningTileInfo; // Assumes a single env tile for now, can rework to handle multiple tiles if needed
  FloorLayer floorLayer;
  public VisualEffect hazardConstantSystem;
  public VisualEffect hazardWarmupSystem;
  bool warmupPlaying = false;
  public VisualEffect hazardActiveSystem;
  bool activePlaying = false;

  // wall objects are built 1 of 2 ways:
  // as part of the terrain painting process,
  // or during a total rebuild
  public void Init(TileLocation location, EnvironmentTileInfo tileInfo)
  {
    owningTileInfo = tileInfo;
    floorLayer = location.floorLayer;
    string sortingLayer = floorLayer.ToString();
    // probably make particle system use correct layer for visuals
    WorldObject.ChangeLayersRecursively(transform, floorLayer);
  }

  void Update()
  {
    if (owningTileInfo == null) { return; }
    if (hazardWarmupSystem)
    {
      if (owningTileInfo.IsEnvironmentalDamageSourceWarmup)
      {
        if (!warmupPlaying)
        {
          warmupPlaying = true;
          hazardWarmupSystem.Play();
        }
      }
      else
      {
        warmupPlaying = false;
        hazardWarmupSystem.Stop();
      }
    }
    if (hazardActiveSystem)
    {
      if (owningTileInfo.IsEnvironmentalDamageSourceActive)
      {
        if (!activePlaying)
        {
          activePlaying = true;
          hazardActiveSystem.Play();
        }
      }
      else
      {
        activePlaying = false;
        hazardActiveSystem.Stop();
      }
    }
  }
}
