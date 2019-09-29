using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SilkThread : TraitSpawnedObject
{
  // Start is called before the first frame update
  public float strandLength = 5.0f;
  public Hitbox hitboxPrefab;
  public HitboxData hitboxData;

  ContactFilter2D contactFilter;
  void Start()
  {
    SpriteRenderer r = GetComponent<SpriteRenderer>();
    if (r != null)
    {
      r.sortingLayerName = LayerMask.LayerToName(gameObject.layer);
    }
    Collider2D col = GetComponent<Collider2D>();
    if (col != null)
    {
      contactFilter.useTriggers = false;
      contactFilter.useLayerMask = true;
      contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
      // Debug.Break();
      RaycastHit2D[] res = new RaycastHit2D[5]; //what's the right number! who knows! why casts like dis!
      int hits = Physics2D.Raycast(transform.position, transform.right, contactFilter, res, strandLength);

      for (int i = 0; i < hits; i++)
      {
        Debug.LogWarning("raycast hit object with tag " + res[i].collider.tag);
        Debug.LogWarning("raycast hit " + res[i].collider.gameObject);
        Debug.LogWarning("tag equals player? " + (res[i].collider.tag == "Player").ToString());
        if (res[i].collider.tag == "Player")
        {
          Debug.LogWarning("raycast hit with " + res[i].collider.gameObject + "is player, so continuing");
          continue;
        }
        if (res[i].collider != null)
        {
          Debug.LogWarning("hit gameobject " + res[i].collider.gameObject);
          strandLength = Mathf.Min(strandLength, res[i].distance);
        }
      }
      Debug.Log("number of hits: " + hits);
      Debug.Log("strand length: " + strandLength);
      transform.localScale = new Vector3(
          strandLength,
          transform.localScale.y,
          transform.localScale.z
      );
      transform.position += transform.right * ((strandLength / 2) - 1f);
      GenerateHitbox();
      // int hits = col.Cast(transform.right, contactFilter, res, transform.localScale.x);

      //     }
      //     Debug.Log("setting x size to "+res[i].distance+"after collision with " +res[i].collider.gameObject);
      //     transform.localScale = new Vector3(
      //         res[i].distance,
      //         transform.localScale.y,
      //         transform.localScale.z
      //     );
      // }
    }
  }

  void GenerateHitbox()
  {
    Hitbox hb = Instantiate(hitboxPrefab, transform.position, transform.rotation) as Hitbox;
    hb.gameObject.layer = gameObject.layer;
    hb.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = gameObject.layer.ToString();
    if (hitboxData == null) { Debug.LogError("no attack info defined for " + gameObject.name); }
    // hb.Init(this.transform, owner, hitboxInfo, hitboxInfo.damageInfo, null);
  }

  // Update is called once per frame
  void Update()
  {
  }

  void OnTriggerEnter2D(Collider2D col)
  {
    Character character = col.gameObject.GetComponent<Character>();
    if (character != null)
    {
      character.sticking = true;
    }
  }

  void OnTriggerExit2D(Collider2D collider)
  {
    Debug.Log("silk thread trigger exit");
    Character character = collider.gameObject.GetComponent<Character>();
    if (character != null)
    {
      Debug.Log("setting sticking to false");
      character.sticking = false;
    }
  }
}
