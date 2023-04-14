using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class SpawnsOnPlayerRespawn : ActivateOnPlayerRespawn
{
  // Update is called once per frame
  public GameObject objectToSpawn;
  public bool spawnOnlyOnce;
  private GameObject spawnedObj;
  private GameObject prevObjectToSpawn;

  public SpriteRenderer spawnerSpritePrefab;

  void Start()
  {
    List<SpriteRenderer> srs = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
    foreach (SpriteRenderer sr in srs)
    {
      sr.enabled = false;
    }
  }

  public override void Activate()
  {
    if (spawnedObj != null)
    {
      Destroy(spawnedObj); // I just have a terrible feeling about this but I don't know why
    }
    spawnedObj = Instantiate(objectToSpawn, transform.position, transform.rotation);
    spawnedObj.transform.position = transform.position;
    Debug.Log("spawner going " + transform.position + ", " + WorldObject.GetFloorLayerOfGameObject(gameObject), gameObject);
    GameMaster.Instance.RegisterObjectToDestroyOnRespawn(spawnedObj);
    WorldObject wObj = spawnedObj.GetComponent<WorldObject>();
    if (wObj != null)
    {
      wObj.currentFloor = WorldObject.GetFloorLayerOfGameObject(gameObject);
    }
    WorldObject.ChangeLayersRecursively(spawnedObj.transform, wObj.currentFloor);
    Debug.Log("spawned object info " + spawnedObj.transform.position + ", " + WorldObject.GetFloorLayerOfGameObject(spawnedObj), spawnedObj);
    spawnedObj.SendMessage("Init", SendMessageOptions.DontRequireReceiver);
    if (spawnOnlyOnce)
    {
      Destroy(gameObject);
    }
  }

#if UNITY_EDITOR
  private void OnValidate()
  {
    UnityEditor.EditorApplication.delayCall += _OnValidate;
  }

  private void _OnValidate()
  {
    if (this == null || this.gameObject == null || this.gameObject.transform == null) { return; }
    if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) { return; }
    if (objectToSpawn && objectToSpawn != prevObjectToSpawn)
    {
      gameObject.name = objectToSpawn.name + "_Spawner";
      SpriteRenderer[] newSrs = objectToSpawn.GetComponentsInChildren<SpriteRenderer>();
      if (newSrs.Length > 0)
      {
        List<SpriteRenderer> srs = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        while (srs.Count < newSrs.Length)
        {
          SpriteRenderer sprite = Instantiate(spawnerSpritePrefab, this.gameObject.transform);
          sprite.name = gameObject.name;
          srs.Add(sprite);
        }
        for (int i = 0; i < newSrs.Length; i++)
        {
          srs[i].sprite = newSrs[i].sprite;
        }
      }
      prevObjectToSpawn = objectToSpawn;
    }
    WorldObject.ChangeLayersRecursively(transform, LayerMask.LayerToName(gameObject.layer));
  }
#endif
}
