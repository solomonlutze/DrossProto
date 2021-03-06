using UnityEngine;
public class CharacterDirectionIndicator : MonoBehaviour
{

  Character owner;
  public Character target;
  AwarenessTrigger owningTrigger;
  public float rotationSpeed = .3f;
  public float minVisibleDistance = 7f;
  public void Init(Character o, Character t, AwarenessTrigger a)
  {
    owner = o;
    target = t;
    owningTrigger = a;
    WorldObject.ChangeLayersRecursively(transform, o.currentFloor);
  }

  public void Update()
  {
    if (target != null)
    {
      if ((transform.position - target.transform.position).sqrMagnitude < minVisibleDistance)
      {
        GetComponentInChildren<SpriteRenderer>().enabled = false;
      }
      else
      {
        GetComponentInChildren<SpriteRenderer>().enabled = true;
      }
      Quaternion targetDirection = GetDirectionAngle(target.transform.position);
      // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetDirection, rotationSpeed * Time.deltaTime);
      transform.rotation = targetDirection;
    }
  }
  public Quaternion GetDirectionAngle(Vector3 targetPoint)
  {
    Vector2 target = targetPoint - transform.position;
    float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    return Quaternion.AngleAxis(angle, Vector3.forward);
  }
}