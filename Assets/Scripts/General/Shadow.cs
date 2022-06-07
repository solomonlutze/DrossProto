using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : WorldObject
{
  public WorldObject owner;
  public AnimationCurve sizeByDistance;
  public float minSize = 0f;
  public float maxSize = 2f;
  public AnimationCurve opacityByDistance;
  public float maxOpacity = 1f;
  public float minOpacity = 0f;
  public SpriteRenderer sprite;

  void Start()
  {
    if (owner == null)
    {
      owner = GetComponentInParent<WorldObject>();
    }
  }

  //TODO: normalize all this by floor distance
  void Update()
  {
    if (owner == null || sprite == null)
    {
      Destroy(gameObject);
    }
    transform.position = new Vector3(transform.position.x, transform.position.y, GetCurrentGroundPosition());
    float distance = Vector3.Distance(transform.position, owner.transform.position);
    float scale = sizeByDistance.Evaluate(distance);
    transform.localScale = new Vector3(scale, scale, 1);
    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, opacityByDistance.Evaluate(distance));
  }
}
