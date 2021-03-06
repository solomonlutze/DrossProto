﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// Not in use yet; will be used to handle things like input state, etc.
public class GameMaster : Singleton<GameMaster>
{

  public Constants.GameState startingGameStatus;
  public CanvasHandler canvasHandler;
  public ParticleSystemMaster particleSystemMaster;
  public GameObject playerPrefab;
  public DialogueRunner dialogueRunner;
  public VariableStorage dialogueVariableStorage;
  private PlayerController playerController;
  private PathfindingSystem pathfinding;
  public Constants.GameState gameStatus;

  // Saved when player dies so their next life can be preserved
  private TraitSlotToTraitDictionary cachedPupa;
  public GameObject[] spawnPoints;
  public GameObject nextSpawnPoint;
  private int previousSpawnPoint = 0;

  public LymphTypeToSpriteDictionary lymphTypeToSpriteMapping;
  public Camera mainCamera;
  public Camera camera2D; // god save me
  public bool isPaused = false;
  public bool playerObliterated;

  // Use this for initialization
  void Start()
  {
    // Debug.unityLogger.logEnabled = false;
    Time.fixedDeltaTime = 1 / 60f;
    pathfinding = GetComponent<PathfindingSystem>();
    SetGameStatus(startingGameStatus);
    Shader.EnableKeyword("EDITOR");
    switch (GetGameStatus())
    {
      case Constants.GameState.Play:
        BeginGame();
        break;
      case Constants.GameState.ChooseBug:
        canvasHandler.DisplaySelectBugScreen();
        break;
    }
  }

  public void SelectBugPresetAndBegin(BugPresetData data)
  {
    canvasHandler.SetAllCanvasesInactive();
    SetGameStatus(Constants.GameState.Play);
    Respawn(data.loadout);
  }

  void BeginGame()
  {
    SetGameStatus(Constants.GameState.Play);
    Respawn();
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
      case Constants.GameState.Play:
        if (Input.GetKeyDown("=") && playerController != null)
        {
          playerController.Die();
          SetGameStatus(Constants.GameState.ChooseBug);
          canvasHandler.DisplaySelectBugScreen();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
          SetGamePaused();
        }
        break;
      case Constants.GameState.Pause:
        if (Input.GetKeyDown(KeyCode.Escape))
        {
          SetGameUnpaused();
        }
        break;
    }
  }

  private void HandleDeadInput()
  {
    if (Input.GetButtonDown("Respawn"))
    {
      Respawn(cachedPupa);
    }
  }

  public void SetGameMenu()
  {
    PauseGame();
    SetGameStatus(Constants.GameState.Menu);
  }
  public void SetGamePaused()
  {
    PauseGame();
    SetGameStatus(Constants.GameState.Pause);
  }

  public void SetGameUnpaused()
  {
    UnpauseGame();
    SetGameStatus(Constants.GameState.Play);
  }

  public void PauseGame()
  {
    Time.timeScale = 0;
  }

  public void UnpauseGame()
  {
    Time.timeScale = 1;
  }

  private void Respawn(TraitSlotToTraitDictionary overrideTraits = null)
  {
    GridManager.Instance.DestroyTilesOnPlayerRespawn();
    GridManager.Instance.RestoreTilesOnPlayerRespawn();
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
    playerController.Init(overrideTraits);
    DoActivateOnPlayerRespawn();
    DoDestroyOnPlayerRespawn();
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

  private void DoDestroyOnPlayerRespawn()
  {
    GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag("DestroyOnPlayerRespawn");
    foreach (GameObject obj in objectsToDestroy)
    {
      Destroy(obj);
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