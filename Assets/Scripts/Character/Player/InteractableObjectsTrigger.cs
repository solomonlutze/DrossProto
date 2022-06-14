using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives on player's InteractableTrigger to collect interactable objects as they appear
public class InteractableObjectsTrigger : MonoBehaviour
{

  // Use this for initialization
  private PlayerController player;
  void Start()
  {
    player = transform.parent.GetComponent<PlayerController>();
  }

  // Update is called once per frame
  void Update()
  {

  }

  void OnTriggerEnter2D(Collider2D collider)
  {
    if (collider.gameObject.GetComponentInChildren<Interactable>() != null && WorldObject.HaveApproximatelySameElevation(collider.gameObject, gameObject))
    {
      player.AddInteractable(collider.gameObject.GetComponentInChildren<Interactable>());
    }
  }

  void OnTriggerExit2D(Collider2D collider)
  {
    if (collider.gameObject.GetComponentInChildren<Interactable>() != null)
    {
      player.RemoveInteractable(collider.gameObject.GetComponentInChildren<Interactable>());
    }
  }
}
