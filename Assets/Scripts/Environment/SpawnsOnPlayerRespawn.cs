using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    WorldObject wObj = spawnedObj.GetComponent<WorldObject>();
    if (wObj != null)
    {
      wObj.currentFloor = WorldObject.GetFloorLayerOfGameObject(gameObject);
    }
    WorldObject.ChangeLayersRecursively(spawnedObj.transform, wObj.currentFloor);
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

    if (this == null || this.gameObject == null) { return; }
    if (objectToSpawn && objectToSpawn != prevObjectToSpawn)
    {
      prevObjectToSpawn = objectToSpawn;
      gameObject.name = objectToSpawn.name + "_Spawner";
      SpriteRenderer[] newSrs = objectToSpawn.GetComponentsInChildren<SpriteRenderer>();
      if (newSrs.Length > 0)
      {
        List<SpriteRenderer> srs = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());

        while (srs.Count < newSrs.Length)
        {
          srs.Add(Instantiate(spawnerSpritePrefab, transform));
        }
        for (int i = 0; i < newSrs.Length; i++)
        {

          srs[i].sprite = newSrs[i].sprite;
        }
      }
    }
    WorldObject.ChangeLayersRecursively(transform, LayerMask.LayerToName(gameObject.layer));
  }
#endif
}
