using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TESTTileTriggerObj : MonoBehaviour {

	
    void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("OnTriggerEnter from trigger object!!");
    }
}
