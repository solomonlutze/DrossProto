using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnsOnPlayerRespawn : ActivateOnPlayerRespawn
{
    // Update is called once per frame
    public GameObject objectToSpawn;
    public bool spawnOnlyOnce;
    private GameObject spawnedObj;

    void Start() {
      SpriteRenderer sr = GetComponent<SpriteRenderer>();
      if (sr) {
        sr.sprite = null;
      }
    }

    public override void Activate() {
      if (!spawnedObj) {
        spawnedObj = Instantiate(objectToSpawn, transform.position, transform.rotation);
        WorldObject.ChangeLayersRecursively(spawnedObj.transform, LayerMask.LayerToName(gameObject.layer));
        if (spawnOnlyOnce) {
          Destroy(gameObject);
        }
      }
    }


    private void OnValidate() {
      SpriteRenderer sr = GetComponent<SpriteRenderer>();
      if (objectToSpawn) {
        SpriteRenderer newSr = objectToSpawn.GetComponentInChildren<SpriteRenderer>();
        if (sr && newSr) {
          sr.sprite = objectToSpawn.GetComponentInChildren<SpriteRenderer>().sprite;
        }

      }
      WorldObject.ChangeLayersRecursively(transform, LayerMask.LayerToName(gameObject.layer));
    }
}
