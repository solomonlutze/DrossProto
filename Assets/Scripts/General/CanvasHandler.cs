using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// WIP - NOT IN USE YET.
// Ported over from another project.

// Controls all canvases. All "public" functions on canvases should be exposed here.
// Any component that needs to interact with a canvas should interact with CanvasHandler.
public class CanvasHandler : MonoBehaviour
{

    public InventoryScreen inventoryScreen;

    public AttributesView attributesView;
    private List<GameObject> canvasList = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        // canvasList.Add(inventoryScreen.gameObject);
        canvasList.Add(attributesView.gameObject);
        SetAllCanvasesInactive();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Quit!"); Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            if (attributesView.gameObject.activeSelf)
            {
                Debug.Log("I pressed; hiding stuff?");
                SetAllCanvasesInactive();
                GameMaster.Instance.SetGameStatus(Constants.GameState.Play);
            }
            else
            {
                Debug.Log("I pressed; displaying?");
                DisplayAttributes();
            }
        }

    }

    // TODO: this should def actually advance dialogue
    // TODO: Should this go in DialogueCanvas?
    public void AdvanceDialogue()
    {
        SetAllCanvasesInactive();
        GameMaster.Instance.SetGameStatus(Constants.GameState.Play);
    }

    public void SetAllCanvasesInactive()
    {
        foreach (GameObject canvas in canvasList)
        {
            canvas.SetActive(false);
        }
    }

    // Dialogue canvas functions

    public void DisplayAttributes()
    {
        SetAllCanvasesInactive();
        // BulletinBoardCanvas.GetComponent<BulletinBoardCanvas>().SetBulletinBoardText();
        Debug.Log("DisplayATtributes??");
        PlayerController player = GameMaster.Instance.GetPlayerController();
        if (player != null)
        {
            attributesView.gameObject.SetActive(true);
            attributesView.Init(player.attributes);
            attributesView.gameObject.SetActive(true);
            GameMaster.Instance.SetGameStatus(Constants.GameState.Menu);
        }
    }
}
