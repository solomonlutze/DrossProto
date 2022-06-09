using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour
{
  public WorldObject owner;
  public AnimationCurve sizeByDistance;
  public float maxSize = 2f;
  public AnimationCurve opacityByDistance;
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
    transform.position = new Vector3(transform.position.x, transform.position.y, owner.GetCurrentGroundPosition());
    float distance = Vector3.Distance(transform.position, owner.transform.position) / GridConstants.Z_SPACING;
    float scale = sizeByDistance.Evaluate(distance) * maxSize;
    transform.localScale = new Vector3(scale, scale, 1);
    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, opacityByDistance.Evaluate(distance));
  }
}
