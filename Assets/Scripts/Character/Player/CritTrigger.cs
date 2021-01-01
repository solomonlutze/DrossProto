
using UnityEngine;
using System.Collections.Generic;

public class CritTrigger : MonoBehaviour
{

  public Character owner;
  public List<Character> enemiesInRange;

  void Update()
  {
    Character critTarget = null;
    float currentMinDistSquared = 0;
    for (int i = enemiesInRange.Count - 1; i >= 0; i--)
    {
      if (enemiesInRange[i].carapaceBroken)
      {
        if (critTarget == null
          || currentMinDistSquared > (owner.gameObject.transform.position - enemiesInRange[i].gameObject.transform.position).sqrMagnitude
        )
        {
          critTarget = enemiesInRange[i];
          currentMinDistSquared = (owner.gameObject.transform.position - critTarget.gameObject.transform.position).sqrMagnitude;
        }
      }
    }
    owner.critTarget = critTarget;
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    Debug.Log("on trigger enter " + other);
    Character c = other.gameObject.GetComponentInParent<Character>();
    if (c != null && c.characterType != owner.characterType && !enemiesInRange.Contains(c))
    {
      enemiesInRange.Add(other.gameObject.GetComponent<Character>());
    }
  }

  void OnTriggerExit2D(Collider2D other)
  {
    Character c = other.gameObject.GetComponent<Character>();
    if (c != null && enemiesInRange.Contains(c))
    {
      enemiesInRange.Remove(c);
    }
  }
}