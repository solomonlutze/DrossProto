using UnityEngine;
using System.Collections.Generic;
public class AwarenessTrigger : MonoBehaviour
{

  Character owner;
  public List<Character> nearbyCharacters;
  public List<CharacterDirectionIndicator> characterDirectionIndicators;
  public CharacterDirectionIndicator indicatorPrefab;

  public void Init(Character c)
  {
    owner = c;
  }

  void OnTriggerEnter2D(Collider2D c)
  {
    Character character = c.GetComponent<Character>();
    if (character != null && character != owner && !nearbyCharacters.Contains(character))
    {
      Debug.Log("enter " + character.name);
      nearbyCharacters.Add(character);
      CharacterDirectionIndicator indicator = Instantiate(indicatorPrefab, owner.transform);
      indicator.Init(owner, character, this);
      characterDirectionIndicators.Add(indicator);
    }
  }

  void OnTriggerExit2D(Collider2D c)
  {
    Character character = c.GetComponent<Character>();
    int idx = nearbyCharacters.IndexOf(character);
    if (character != null && idx >= 0)
    {
      Debug.Log("exit " + character.name);
      Debug.Log("index: " + idx);
      nearbyCharacters.RemoveAt(idx);
      Destroy(characterDirectionIndicators[idx].gameObject);
      characterDirectionIndicators.RemoveAt(idx);
    }
  }
}