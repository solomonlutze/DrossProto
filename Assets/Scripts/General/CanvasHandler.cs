using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// WIP - NOT IN USE YET.
// Ported over from another project.

// Controls all canvases. All "public" functions on canvases should be exposed here.
// Any component that needs to interact with a canvas should interact with CanvasHandler.
public class CanvasHandler : MonoBehaviour {

	public InventoryScreen inventoryScreen;	private List<GameObject> canvasList = new List<GameObject>(); 

	// Use this for initialization
	void Start () {
		canvasList.Add(inventoryScreen.gameObject);
		SetAllCanvasesInactive();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) { 
			Debug.Log("Quit!"); Application.Quit();
		} else if (Input.GetKeyDown(KeyCode.I)) {
			if (inventoryScreen.gameObject.activeSelf) {
				SetAllCanvasesInactive();
				GameMaster.Instance.SetGameStatus(Constants.GameState.Play);
			} else {
				DisplayInventory();
			}
		}
		
	}
	
	// TODO: this should def actually advance dialogue
	// TODO: Should this go in DialogueCanvas?
	public void AdvanceDialogue() {
		SetAllCanvasesInactive();
		GameMaster.Instance.SetGameStatus(Constants.GameState.Play);
	}

	public void SetAllCanvasesInactive() {
		foreach (GameObject canvas in canvasList)
		{	
			canvas.SetActive(false);
		}
	}

	// Dialogue canvas functions

	public void DisplayInventory() {
		SetAllCanvasesInactive();
		// BulletinBoardCanvas.GetComponent<BulletinBoardCanvas>().SetBulletinBoardText();
		inventoryScreen.Enable();
		inventoryScreen.gameObject.SetActive(true);
		GameMaster.Instance.SetGameStatus(Constants.GameState.Menu);
	}
}
