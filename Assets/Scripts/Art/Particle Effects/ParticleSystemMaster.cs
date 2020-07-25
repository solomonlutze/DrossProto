using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemMaster : MonoBehaviour
{

  public Dictionary<FloorLayer, Dictionary<EnvironmentTile, ParticleSystem>> particleSystemMap;
  void Start()
  {
    EnvironmentTile[] tileData = Resources.LoadAll<EnvironmentTile>("Art/Environment/Tiles/TileData");
    Debug.Log(tileData.Length);
    // EnvironmentTile[] tileData = dataObjects as EnvironmentTile[];
    // EnvironmentTile[] tileData = Resources.LoadAll("Art/Environment/Tiles") as EnvironmentTile[]
    // UnityEngine.ScriptableObject[] cast = dataObjects as ScriptableObject[];
    particleSystemMap = new Dictionary<FloorLayer, Dictionary<EnvironmentTile, ParticleSystem>>();
    foreach (FloorLayer fl in GridManager.Instance.layerFloors.Keys)
    {
      particleSystemMap.Add(fl, new Dictionary<EnvironmentTile, ParticleSystem>());
    }
    // TODO: This loop can be cleaned up to allow the same system to be reused for multiple tiletypes, if they should be the same.
    foreach (EnvironmentTile tile in tileData)
    {
      if (tile.footstepParticleSystem)
      {
        AddParticleSystemToEveryFloor(tile.footstepParticleSystem, tile);
      }
    }
  }

  public void AddParticleSystemToEveryFloor(ParticleSystem ps, EnvironmentTile tile)
  {
    foreach (KeyValuePair<FloorLayer, LayerFloor> lfEntry in GridManager.Instance.layerFloors)
    {
      ParticleSystem newSystem = Instantiate(ps, lfEntry.Value.transform) as ParticleSystem;
      ParticleSystemRenderer newSystemRenderer = newSystem.GetComponent<ParticleSystemRenderer>();
      newSystemRenderer.sortingLayerName = lfEntry.Key.ToString();
      newSystemRenderer.sortingOrder = 3;
      newSystem.Stop();
      newSystem.gameObject.layer = LayerMask.NameToLayer(lfEntry.Key.ToString());
      newSystem.transform.position = new Vector3(0, 0, GridManager.GetZOffsetForFloor(newSystem.gameObject.layer));
      particleSystemMap[lfEntry.Key].Add(tile, newSystem);
    }
  }

  public ParticleSystem GetParticleSystemForCharacterAndTile(Character character, EnvironmentTile tile)
  {
    return particleSystemMap[character.currentFloor][tile];
  }
  public void EmitFootstep(Character c, EnvironmentTile tile, ParticleSystem.EmitParams emitParams, int count)
  {
    Debug.Log("emitting footstep particle at " + emitParams.position);
    GetParticleSystemForCharacterAndTile(c, tile).Emit(emitParams, count);
    // testPS.GetParticles
  }
}
