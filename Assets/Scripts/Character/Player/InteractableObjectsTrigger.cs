using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObjectsTrigger : MonoBehaviour {

	// Use this for initialization
	private PlayerController player;
	void Start () {
		player = transform.parent.GetComponent<PlayerController>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.GetComponentInChildren<Interactable>() != null) {
			player.AddInteractable(collider.gameObject);
		}
	}

	void OnTriggerExit2D(Collider2D collider) {
		if (collider.gameObject.GetComponentInChildren<Interactable>() != null) {
			player.RemoveInteractable(collider.gameObject);
		}
	}
}
