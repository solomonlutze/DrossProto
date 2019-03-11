using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using TMPro; // Add the TextMesh Pro namespace to access the various functions.


// WIP - NOT IN USE YET.
// Ported over from another project.
public class DialogueCanvas : MonoBehaviour {

	// Use this for initialization
	protected CanvasHandler canvasHandler;
	void Start () {
		// GameObject canvasHandlerObject = GameObject.FindGameObjectWithTag("CanvasHandler");
		// if (canvasHandlerObject) {
		// 	canvasHandler = GetComponent<CanvasHandler>();
		// } else { Debug.LogError("DialogueCanvas found no CanvasHandler object."); }
	}
	
	
	// Update is called once per frame
	void Update () {
		
	}

	public void DisplayText(string text) {
		// GetComponentInChildren<TextMeshProUGUI>().text = text;
	}
}
