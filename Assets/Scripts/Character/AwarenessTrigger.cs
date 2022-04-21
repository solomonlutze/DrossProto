using UnityEngine;
using System.Collections.Generic;
public class AwarenessTrigger : MonoBehaviour
{

  Character owner;
  public List<Character> nearbyCharacters;
  public List<CharacterDirectionIndicator> characterDirectionIndicators;
  public CharacterDirectionIndicator indicatorPrefab;

  public void Init(Character c, float radius = 1)
  {
    owner = c;
    transform.localScale = new Vector3(radius, radius, 1);
  }

  void OnTriggerEnter2D(Collider2D c)
  {
    Character character = c.GetComponentInParent<Character>();
    if (owner != null && character != null && character != owner && !nearbyCharacters.Contains(character))
    {
      nearbyCharacters.Add(character);
    }
  }

  void OnTriggerExit2D(Collider2D c)
  {
    Character character = c.GetComponent<Character>();
    int idx = nearbyCharacters.IndexOf(character);
    if (character != null && idx >= 0)
    {
      nearbyCharacters.RemoveAt(idx);
    }
  }

  public Character NearestCharacter(float withAngle = 360f)
  {
    return NearestCharacter(transform.position, withAngle);
  }

  void ValidateNearbyCharacters()
  {
    List<Character> newNearbyCharacters = new List<Character>();
    foreach (Character nearbyCharacter in nearbyCharacters)
    {
      if (nearbyCharacter != null) { newNearbyCharacters.Add(nearbyCharacter); }
    }
    nearbyCharacters = newNearbyCharacters;
  }

  public Character NearestCharacter(Vector3 toPosition, float withinAngle = 360f)
  {
    Character nearestCharacter = null;
    float shortestDistance = 10000f;
    ValidateNearbyCharacters();
    foreach (Character nearbyCharacter in nearbyCharacters)
    {
      if (nearbyCharacter.gameObject.layer != owner.gameObject.layer) { continue; }
      float distance = (toPosition - nearbyCharacter.transform.position).sqrMagnitude;
      if (distance < shortestDistance && Utils.GetAngleToDirection(transform.rotation, nearbyCharacter.transform.position) < withinAngle)
      {
        shortestDistance = distance;
        nearestCharacter = nearbyCharacter;
      }
    }
    return nearestCharacter;
  }
}