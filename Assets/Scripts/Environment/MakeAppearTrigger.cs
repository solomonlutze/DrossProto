using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeAppearTrigger : MonoBehaviour {

    public GameObject targetObject;
    public bool targetIsActiveAtStart;

	void Start () {
        if (!targetIsActiveAtStart) {
            targetObject.SetActive(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if (collider.gameObject.tag == "Player") {
			targetObject.SetActive(!targetIsActiveAtStart);
            Destroy(gameObject);
		}
	}
}
