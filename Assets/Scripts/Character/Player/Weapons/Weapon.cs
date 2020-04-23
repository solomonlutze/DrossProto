
using UnityEngine;

public class Weapon : MonoBehaviour
{
  public Hitbox[] hitboxes;

  public float range;
  public float effectiveRange;
  public int sweepRadiusInDegrees;

  public void Awake()
  {
    hitboxes = GetComponentsInChildren<Hitbox>();
  }

  public void Init(CharacterAttackData attack)
  {
    Bounds b = gameObject.GetComponentInChildren<Collider2D>().bounds;
    Collider2D[] cols = gameObject.GetComponentsInChildren<Collider2D>();
    foreach (Collider2D col in cols)
    {
      b.Encapsulate(col.bounds);
    }
    range = b.size.x + attack.range;
    effectiveRange = range + attack.distance;
    sweepRadiusInDegrees = attack.sweepRadiusInDegrees;
    // Debug.Log("weapon's extents are" + b.extents);
    // Debug.Log("weapon's bounds are " + b.extents.x);
    // Debug.Log("weapon's position.x is" + transform.position.x);
    // Debug.Log("weapon's range is " + range);
  }
}