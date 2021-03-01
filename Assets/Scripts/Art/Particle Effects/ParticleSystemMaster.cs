using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemMaster : MonoBehaviour
{

  public Dictionary<FloorLayer, Dictionary<EnvironmentTile, ParticleSystem>> particleSystemMap;
  public Dictionary<FloorLayer, Dictionary<ParticleSystem, ParticleSystem>> particleSystemDataToInstanceMap; // if we see a data that's already in here we reuse
  void Start()
  {
    EnvironmentTile[] tileData = Resources.LoadAll<EnvironmentTile>("Art/Environment/Tiles");
    Debug.Log("tiles loaded: " + tileData.Length);
    // EnvironmentTile[] tileData = dataObjects as EnvironmentTile[];
    // EnvironmentTile[] tileData = Resources.LoadAll("Art/Environment/Tiles") as EnvironmentTile[]
    // UnityEngine.ScriptableObject[] cast = dataObjects as ScriptableObject[];
    particleSystemMap = new Dictionary<FloorLayer, Dictionary<EnvironmentTile, ParticleSystem>>();
    particleSystemDataToInstanceMap = new Dictionary<FloorLayer, Dictionary<ParticleSystem, ParticleSystem>>();
    foreach (FloorLayer fl in GridManager.Instance.layerFloors.Keys)
    {
      particleSystemMap.Add(fl, new Dictionary<EnvironmentTile, ParticleSystem>());
      particleSystemDataToInstanceMap.Add(fl, new Dictionary<ParticleSystem, ParticleSystem>());
    }
    // TODO: This loop can be cleaned up to allow the same system to be reused for multiple tiletypes, if they should be the same.
    foreach (EnvironmentTile tile in tileData)
    {
      if (tile.footstepParticleSystemInfo.system)
      {
        AddParticleSystemToEveryFloor(tile.footstepParticleSystemInfo.system, tile);
      }
    }
  }

  public void AddParticleSystemToEveryFloor(ParticleSystem ps, EnvironmentTile tile)
  {
    foreach (KeyValuePair<FloorLayer, LayerFloor> lfEntry in GridManager.Instance.layerFloors)
    {
      bool systemIsNew = !particleSystemDataToInstanceMap[lfEntry.Key].ContainsKey(ps);
      ParticleSystem systemToAdd = systemIsNew ? Instantiate(ps, lfEntry.Value.transform) as ParticleSystem : particleSystemDataToInstanceMap[lfEntry.Key][ps];
      if (systemIsNew)
      {
        ParticleSystemRenderer newSystemRenderer = systemToAdd.GetComponent<ParticleSystemRenderer>();
        newSystemRenderer.sortingLayerName = lfEntry.Key.ToString();
        newSystemRenderer.sortingOrder = 3;
        systemToAdd.Stop();
        systemToAdd.gameObject.layer = LayerMask.NameToLayer(lfEntry.Key.ToString());
        systemToAdd.transform.position = new Vector3(0, 0, GridManager.GetZOffsetForGameObjectLayer(systemToAdd.gameObject.layer));
        particleSystemDataToInstanceMap[lfEntry.Key].Add(ps, systemToAdd);
      }
      particleSystemMap[lfEntry.Key].Add(tile, systemToAdd);
    }
  }

  public ParticleSystem GetParticleSystemForCharacterAndTile(Character character, EnvironmentTile tile)
  {
    Debug.Log("floor: " + character.currentFloor + ", keys: " + particleSystemMap[character.currentFloor].Keys);
    return particleSystemMap[character.currentFloor][tile];
  }
  public void EmitFootstep(Character c, EnvironmentTile tile, ParticleSystem.EmitParams emitParams, int count)
  {
    GetParticleSystemForCharacterAndTile(c, tile).Emit(emitParams, count);
  }
}
