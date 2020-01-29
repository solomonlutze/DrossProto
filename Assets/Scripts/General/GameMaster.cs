using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// Not in use yet; will be used to handle things like input state, etc.
public class GameMaster : Singleton<GameMaster>
{

    public CanvasHandler canvasHandler;
    public GameObject playerPrefab;
    public DialogueRunner dialogueRunner;
    private PlayerController playerController;
    private PathfindingSystem pathfinding;
    private Constants.GameState gameStatus;

    // Saved when player dies so their next life can be preserved
    private TraitSlotToTraitDictionary cachedPupa;
    public GameObject[] spawnPoints;
    public GameObject nextSpawnPoint;
    private int previousSpawnPoint = 0;

    public LymphTypeToSpriteDictionary lymphTypeToSpriteMapping;
    public LymphTypeToLymphTypeSkillsDictionary lymphTypeToSkillsMapping;

    // Use this for initialization
    void Start()
    {
        pathfinding = GetComponent<PathfindingSystem>();
        Respawn(true);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        switch (gameStatus)
        {
            case Constants.GameState.Dead:
                HandleDeadInput();
                break;
            default:
                if (Input.GetKeyDown("=") && playerController != null)
                {
                    playerController.Die();
                }
                break;
        }
    }

    private void HandleDeadInput()
    {
        if (Input.GetButtonDown("Respawn"))
        {
            Respawn(false);
        }
    }

    private void Respawn(bool initialSpawn = false)
    {
        Debug.Log("GridManager Instance: " + GridManager.Instance.name);
        GridManager.Instance.DestroyTilesOnPlayerRespawn();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerController.currentFloor = (FloorLayer)Enum.Parse(typeof(FloorLayer), LayerMask.LayerToName(player.layer));
        }
        else
        {
            GameObject spawnPoint = ChooseSpawnPoint();
            FloorLayer fl = FloorLayer.F1;
            if (spawnPoint != null)
            {
                fl = spawnPoint.GetComponent<SpawnPoint>().GetTileLocation().floorLayer;
            }
            playerController = Instantiate(playerPrefab, spawnPoint.transform.position, Quaternion.identity).GetComponent<PlayerController>();
            playerController.currentFloor = fl;
        }
        playerController.SetCurrentFloor(playerController.currentFloor);
        playerController.Init(initialSpawn, cachedPupa);
        DoActivateOnPlayerRespawn();
        SetGameStatus(Constants.GameState.Play);
    }

    private void DoActivateOnPlayerRespawn()
    {
        GameObject[] objectsToActivate = GameObject.FindGameObjectsWithTag("ActivateOnPlayerRespawn");
        foreach (GameObject obj in objectsToActivate)
        {
            obj.SendMessage("Activate", SendMessageOptions.RequireReceiver);
        }
    }
    private GameObject ChooseSpawnPoint()
    {
        return nextSpawnPoint ?? null;
        // if (spawnPoints.Length > 0) {
        // 	previousSpawnPoint = (int) Mathf.Repeat(previousSpawnPoint+1, spawnPoints.Length);
        // 	return spawnPoints[previousSpawnPoint];
        // }
        // return null;
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }

    public void SetGameStatus(Constants.GameState newStatus)
    {
        gameStatus = newStatus;
    }

    public void KillPlayer(TraitSlotToTraitDictionary pupa)
    {
        cachedPupa = pupa;
        playerController = null;
        SetGameStatus(Constants.GameState.Dead);
    }

    public Constants.GameState GetGameStatus()
    {
        return gameStatus;
    }

    public void StartDialogue(string startNode)
    {
        if (dialogueRunner.isDialogueRunning) { return; }
        dialogueRunner.StartDialogue(startNode);
    }


    public void StopDialogue()
    {
        if (!dialogueRunner.isDialogueRunning) { return; }
        StartCoroutine(dialogueRunner.Interrupt());
    }
}