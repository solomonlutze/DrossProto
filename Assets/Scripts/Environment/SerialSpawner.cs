using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnInfoSet
{
  public List<SpawnInfo> spawnInfos;
  public float delayBefore;

}

[System.Serializable]
public class SpawnInfo
{
  public AiStateController characterToSpawn;
  public float startingHeatlhPercent = 100;

}

[SelectionBase]
public class SerialSpawner : ActivateOnPlayerRespawn
{
  // Update is called once per frame
  int currentSpawn;
  public List<SpawnInfoSet> spawns;
  private List<GameObject> spawnedObjects;
  bool waitingToSpawn;
  bool displayingVictoryText;
  void Start()
  {
    Activate();
  }

  public void Update()
  {
    if (currentSpawn < spawns.Count && !waitingToSpawn)
    {
      for (int i = 0; i < spawnedObjects.Count; i++)
      {
        if (spawnedObjects[i] != null)
        {
          return;
        }
      }
      spawnedObjects = new List<GameObject>();
      currentSpawn++;
      if (currentSpawn < spawns.Count)
      {
        StartCoroutine(WaitThenSpawnNext());

      }
      else
      {
        Debug.Log("all enemies dead? victory?");
      }
    }
  }

  public IEnumerator WaitThenSpawnNext()
  {
    waitingToSpawn = true;
    yield return new WaitForSeconds(spawns[currentSpawn].delayBefore);
    SpawnNext();
    waitingToSpawn = false;
  }

  public void SpawnNext()
  {
    foreach (SpawnInfo spawn in spawns[currentSpawn].spawnInfos)
    {
      Character spawnedObj = Instantiate(spawn.characterToSpawn, transform.position, transform.rotation);
      spawnedObj.vitals[CharacterVital.CurrentHealth] = spawn.startingHeatlhPercent;
      GameMaster.Instance.RegisterObjectToDestroyOnRespawn(spawnedObj.gameObject);
      WorldObject wObj = spawnedObj.GetComponent<WorldObject>();
      if (wObj != null)
      {
        wObj.currentFloor = WorldObject.GetFloorLayerOfGameObject(gameObject);
      }
      WorldObject.ChangeLayersRecursively(spawnedObj.transform, wObj.currentFloor);
      spawnedObjects.Add(spawnedObj.gameObject);
    }
  }

  public override void Activate()
  {
    displayingVictoryText = false;
    spawnedObjects = new List<GameObject>();
    currentSpawn = 0;
    StartCoroutine(WaitThenSpawnNext());
  }

}
