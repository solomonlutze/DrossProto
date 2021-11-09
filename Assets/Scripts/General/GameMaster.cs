using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;
using Rewired;
using ScriptableObjectArchitecture;
using UnityEngine.SceneManagement;

public class GameMaster : Singleton<GameMaster>
{
  public bool DebugEnabled = false;
  public bool DEBUG_dontDropItems = false;
  public int rewiredPlayerId = 0;
  private Rewired.Player rewiredPlayer;
  public IntVariable trophyGrubCount;
  public DrossConstants.GameState startingGameStatus;
  public CanvasHandler canvasHandler;
  public ParticleSystemMaster particleSystemMaster;
  public InputGlyphHelper inputGlyphHelper;
  public GameObject playerPrefab;
  public DialogueRunner dialogueRunner;
  public VariableStorage dialogueVariableStorage;
  private PlayerController playerController;
  private PathfindingSystem pathfinding;
  public DrossConstants.GameState gameStatus;

  // Saved when player dies so their next life can be preserved
  private TraitSlotToTraitDictionary cachedPupa;
  public GameObject[] spawnPoints;

  public BaseVariable[] variablesToClearOnRespawn;
  public GameEvent[] eventsToRaiseOnRespawn;
  public GameObject nextSpawnPoint;
  private int previousSpawnPoint = 0;

  public LymphTypeToSpriteDictionary lymphTypeToSpriteMapping;
  public Camera mainCamera;
  public Camera camera2D; // god save me
  public bool isPaused = false;
  public bool playerObliterated;
  bool musicPlaying = false;

  // Use this for initialization
  void Start()
  {
    rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerId);
    trophyGrubCount.Value = 0;
    Time.fixedDeltaTime = 1 / 60f;
    pathfinding = GetComponent<PathfindingSystem>();
    SetGameStatus(startingGameStatus);
    switch (GetGameStatus())
    {
      case DrossConstants.GameState.Play:
        BeginGame();
        break;
      case DrossConstants.GameState.ChooseBug:
        canvasHandler.DisplaySelectBugScreen();
        break;
    }
  }

  public void SelectBugPresetAndBegin(BugPresetData data)
  {
    canvasHandler.SetAllCanvasesInactive();
    SetGameStatus(DrossConstants.GameState.Play);
    Respawn(data.loadout);
  }

  void BeginGame()
  {
    SetGameStatus(DrossConstants.GameState.Play);
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
      case DrossConstants.GameState.Dead:
        HandleDeadInput();
        break;
      case DrossConstants.GameState.Play:
        if (rewiredPlayer.GetButtonDown("Restart") && playerController != null)
        {
          playerController.Die();
        }
        else if (rewiredPlayer.GetButtonDown("Pause"))
        {
          SetGamePaused();
        }
        break;
      case DrossConstants.GameState.Pause:
        if (rewiredPlayer.GetButtonDown("Pause"))
        {
          SetGameUnpaused();
        }
        break;
    }
  }

  private void HandleDeadInput()
  {
    if (rewiredPlayer.GetButtonDown("Respawn"))
    {
      Respawn(cachedPupa);
    }
  }

  public void SetGameMenu()
  {
    PauseGame();
    SetGameStatus(DrossConstants.GameState.Menu);
  }
  public void SetGamePaused()
  {
    PauseGame();
    canvasHandler.DisplayPauseMenu();
    SetGameStatus(DrossConstants.GameState.Pause);
  }

  public void SetGameUnpaused()
  {
    UnpauseGame();
    canvasHandler.ClosePauseMenu();
    SetGameStatus(DrossConstants.GameState.Play);
  }

  public void PauseGame()
  {
    Time.timeScale = 0;
  }

  public void UnpauseGame()
  {
    Time.timeScale = 1;
  }

  public void Die()
  {
    UnpauseGame();
    canvasHandler.ClosePauseMenu();
    playerController.Die();
  }

  public void Restart()
  {
    UnpauseGame();
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    SetGameStatus(DrossConstants.GameState.ChooseBug);
    canvasHandler.DisplaySelectBugScreen();
  }
  private void Respawn(TraitSlotToTraitDictionary overrideTraits = null)
  {
    ClearVariables(variablesToClearOnRespawn, eventsToRaiseOnRespawn);
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


    AkSoundEngine.PostEvent("StopClergyInstant", GameMaster.Instance.gameObject);
    if (playerController.currentTile.infoTileType != null)
    {
      foreach (MusicStem stem in Enum.GetValues(typeof(MusicStem)))
      {
        if (!playerController.currentTile.infoTileType.musicStems.Contains(stem))
        {
          AkSoundEngine.PostEvent(stem.ToString() + "_Mute", GameMaster.Instance.gameObject);
        }
      }
    }
    AkSoundEngine.PostEvent("PlayClergyLoop", GameMaster.Instance.gameObject);
    DoActivateOnPlayerRespawn();
    DoDestroyOnPlayerRespawn();
    SetGameStatus(DrossConstants.GameState.Play);
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

  public void SetGameStatus(DrossConstants.GameState newStatus)
  {
    gameStatus = newStatus;
  }

  public void KillPlayer(TraitSlotToTraitDictionary pupa)
  {
    cachedPupa = pupa;
    playerController = null;
    SetGameStatus(DrossConstants.GameState.Dead);
  }

  public DrossConstants.GameState GetGameStatus()
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

  public static void ClearVariables(BaseVariable[] variablesToClear, GameEvent[] eventsToRaise)
  {
    foreach (BaseVariable variable in variablesToClear)
    {
      switch (variable)
      {
        case StringVariable stringVariable:
          stringVariable.Value = "";
          break;
        case FloatVariable floatVariable:
          floatVariable.Value = 0.0f;
          break;
        case IntVariable intVariable:
          intVariable.Value = 0;
          break;
      }
    }
    foreach (GameEvent eventToRaise in eventsToRaise)
    {
      eventToRaise.Raise();
    }
  }
}