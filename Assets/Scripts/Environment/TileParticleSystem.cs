using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TileParticleSystem : MonoBehaviour
{
  EnvironmentTileInfo owningTileInfo; // Assumes a single env tile for now, can rework to handle multiple tiles if needed
  FloorLayer floorLayer;
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
    Debug.Log("particle system update");
    if (hazardWarmupSystem)
    {
      Debug.Log("checking hazard warmup");
      if (owningTileInfo.IsEnvironmentalDamageSourceWarmup)
      {
        if (!warmupPlaying)
        {
          Debug.Log("hazardWarmup play");
          warmupPlaying = true;
          hazardWarmupSystem.Play();
        }
      }
      else
      {
        Debug.Log("hazardWarmup Stop");
        warmupPlaying = false;
        hazardWarmupSystem.Stop();
      }
    }
    if (hazardActiveSystem)
    {
      Debug.Log("checking hazard active");
      if (owningTileInfo.IsEnvironmentalDamageSourceActive)
      {
        if (!activePlaying)
        {
          Debug.Log("hazardActive play");
          activePlaying = true;
          hazardActiveSystem.Play();
        }
      }
      else
      {
        activePlaying = false;
        Debug.Log("hazardActive stop");
        hazardActiveSystem.Stop();
      }
    }
  }
}
