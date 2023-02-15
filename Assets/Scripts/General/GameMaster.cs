using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using Rewired;
using ScriptableObjectArchitecture;
using System.Diagnostics;

public class GameMaster : Singleton<GameMaster>
{
  public bool DebugEnabled = false;
  public bool DEBUG_dontDropItems = false;
  public float DEBUG_damageMultiplier = 2.0f;
  public int rewiredPlayerId = 0;

  private Rewired.Player rewiredPlayer;
  public IntVariable trophyGrubCount;
  public DrossConstants.GameState startingGameStatus;
  public DamageTypeToElementalBuildupConstantDictionary elementalBuildupConstants;
  public CanvasHandler canvasHandler;
  public ParticleSystemMaster particleSystemMaster;
  public InputGlyphHelper inputGlyphHelper;
  public GameObject playerPrefab;
  public Egg eggPrefab;
  public DialogueRunner dialogueRunner;
  public PlayerHUD playerHud;
  public VariableStorage dialogueVariableStorage;
  private PlayerController playerController;

  private PathfindingSystem pathfinding;
  public DrossConstants.GameState gameStatus;

  // Saved when player dies so their next life can be preserved
  public TraitSlotToTraitDictionary cachedPupa;
  public List<TraitSlotToTraitDictionary> collectedTraitItems;
  public int foodRequiredForEgg;
  public int maxFood;
  public List<FoodInfo> collectedFood;
  public List<LaidEggInfo> laidEggs;
  public int selectedEgg = 0;
  public GameObject[] spawnPoints;

  public BaseVariable[] variablesToClearOnRespawn;
  public GameEvent[] eventsToRaiseOnRespawn;
  public List<GameObject> objectsToDestroyOnRespawn;
  public GameObject nextSpawnPoint;
  private int previousSpawnPoint = 0;

  public LymphTypeToSpriteDictionary lymphTypeToSpriteMapping;
  public CombatJuiceData combatJuiceConstants;
  public SettingsData settingsData;
  public float cycleLength;
  public Stopwatch timeSinceStartup;
  public Camera mainCamera;
  public Camera camera2D; // god save me
  public bool isPaused = false;
  public bool playerObliterated;
  bool musicPlaying = false;

  // Use this for initialization
  void Start()
  {
    rewiredPlayer = ReInput.players.GetPlayer(rewiredPlayerId);
    timeSinceStartup = new Stopwatch();
    trophyGrubCount.Value = 0;
    Time.fixedDeltaTime = 1 / 60f;
    fixedDeltaTime = Time.fixedDeltaTime;
    objectsToDestroyOnRespawn = new List<GameObject>();
    collectedTraitItems = new List<TraitSlotToTraitDictionary>();
    collectedFood = new List<FoodInfo>();
    laidEggs = new List<LaidEggInfo>();
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
    StartCoroutine(Respawn(data.loadout));
  }

  void BeginGame()
  {
    SetGameStatus(DrossConstants.GameState.Play);
    StartCoroutine(Respawn());
  }

  // Update is called once per frame
  void Update()
  {
    HandleInput();
    if (playerController && timeSinceStartup.ElapsedMilliseconds / 1000f > cycleLength)
    {
      playerController.Die();
    }
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

      case DrossConstants.GameState.EquipTraits:
        if (rewiredPlayer.GetButtonDown("Pause"))
        {
          ConfirmEquipAndRespawn();
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

  public void ConfirmEquipAndRespawn(TraitSlotToTraitDictionary overrideTraits = null)
  {
    canvasHandler.CloseMenus();
    collectedTraitItems.Clear();
    StartCoroutine(Respawn(overrideTraits == null ? cachedPupa : overrideTraits));
  }
  private void HandleDeadInput()
  {
    if (rewiredPlayer.GetButtonDown("UISubmit"))
    {
      selectedEgg = Mathf.FloorToInt(Mathf.Repeat(selectedEgg + 1, laidEggs.Count));
    }
    if (rewiredPlayer.GetButtonDown("Respawn"))
    {
      ConfirmEquipAndRespawn();
      // SetGameStatus(DrossConstants.GameState.EquipTraits);
      // canvasHandler.DisplayEquipTraitsView();
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
    timeSinceStartup.Stop();
  }

  public void UnpauseGame()
  {
    Time.timeScale = 1;
    Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
    timeSinceStartup.Start();
  }

  public void Die()
  {
    UnpauseGame();
    canvasHandler.ClosePauseMenu();
    playerController.Die();
  }

  public void Restart()
  {
    timeSinceStartup.Restart();
    UnpauseGame();
    if (playerController != null)
    {
      Destroy(playerController.gameObject);
    }
    SetGameStatus(DrossConstants.GameState.ChooseBug);
    canvasHandler.DisplaySelectBugScreen();
  }

  public void RegisterObjectToDestroyOnRespawn(GameObject gameObject)
  {
    objectsToDestroyOnRespawn.Add(gameObject);
  }
  private IEnumerator Respawn(TraitSlotToTraitDictionary overrideTraits = null)
  {
    timeSinceStartup.Restart();
    ClearVariables(variablesToClearOnRespawn, eventsToRaiseOnRespawn);
    GridManager.Instance.DestroyTilesOnPlayerRespawn();
    GridManager.Instance.RestoreTilesOnPlayerRespawn();
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (player != null)
    { // Should only be valid for a player placed in the scene
      playerController = player.GetComponent<PlayerController>();
      playerController.currentFloor = (FloorLayer)Enum.Parse(typeof(FloorLayer), LayerMask.LayerToName(player.layer));
    }
    else
    {
      TileLocation spawnPoint = ChooseSpawnPoint();
      FloorLayer fl = FloorLayer.F1;
      if (spawnPoint != null)
      {
        fl = spawnPoint.floorLayer;
      }
      playerController = Instantiate(playerPrefab, spawnPoint.cellCenterWorldPosition, Quaternion.identity).GetComponent<PlayerController>();
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
    PauseGame();
    while (GridManager.Instance.LoadingChunks())
    {
      UnityEngine.Debug.Log("Loading....");
      yield return null;
    }
    UnpauseGame();
    // AkSoundEngine.PostEvent("PlayClergyLoop", GameMaster.Instance.gameObject);
    DoActivateOnPlayerRespawn();
    DoDestroyOnPlayerRespawn();
    SetGameStatus(DrossConstants.GameState.Play);
  }

  public void DoCameraShake(float duration, float magnitude)
  {
    mainCamera.GetComponent<SmoothFollow>().DoCameraShake(duration, magnitude);
  }

  private float fixedDeltaTime;
  public void DoSlowdown(float baseSlowdownDuration)
  {
    if (baseSlowdownDuration <= 0) { return; }
    StartCoroutine(Slowdown(combatJuiceConstants.slowdownTimescale, baseSlowdownDuration * combatJuiceConstants.slowdownDurationMult));
  }

  public IEnumerator Slowdown(float mult, float duration)
  {
    Time.timeScale = 1 * mult;
    Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
    yield return WaitForRealSeconds(duration);
    if (!isPaused)
    {
      Time.timeScale = 1;
      Time.fixedDeltaTime = fixedDeltaTime * Time.timeScale;
    }
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
    for (int i = objectsToDestroyOnRespawn.Count - 1; i >= 0; i--)
    {
      Destroy(objectsToDestroyOnRespawn[i]);
    }
    objectsToDestroyOnRespawn.Clear();
  }

  private TileLocation ChooseSpawnPoint()
  {
    if (laidEggs.Count > 0)
    {
      TileLocation loc = laidEggs[selectedEgg].location;
      laidEggs.RemoveAt(selectedEgg);
      return loc;
    }
    return nextSpawnPoint.GetComponent<SpawnPoint>().GetTileLocation() ?? null;
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

  public void AddCollectedTraitItem(TraitSlotToTraitDictionary item)
  {
    collectedTraitItems.Add(item);
  }
  public void ClearCollectedTraitItems()
  {
    collectedTraitItems.Clear();
  }

  public void AddCollectedFoodItem(FoodInfo food)
  {
    collectedFood.Add(food);
    UnityEngine.Debug.Log("Collected food " + food + ", collected " + collectedFood.Count + "/" + maxFood);
  }
  public void ClearCollectedFoodItems()
  {
    collectedFood.Clear();
  }

  public void LayEgg()
  {
    Egg newEgg = Instantiate(eggPrefab, playerController.transform.position, Quaternion.identity);
    WorldObject.ChangeLayersRecursively(newEgg.transform, playerController.gameObject.layer);
    LaidEggInfo info = new LaidEggInfo(playerController.GetTileLocation(), playerController.traits);
    List<TraitSlot> shuffledSlots = Utils.ShuffleEnum<TraitSlot>();
    for (int i = 0; i < foodRequiredForEgg; i++)
    {
      UnityEngine.Debug.Log("food count: " + collectedFood.Count + ", food: " + collectedFood[0].foodType + ", trait: " + collectedFood[0].traits);
      if (collectedFood[0].foodType == FoodType.Lymph && collectedFood[0].traits != null)
      {
        UnityEngine.Debug.Log("assigning trait " + shuffledSlots[0]);
        info.slots[shuffledSlots[0]] = collectedFood[0].traits[shuffledSlots[0]];
        shuffledSlots.RemoveAt(0);
      }
      collectedFood.RemoveAt(0);
    }
    UnityEngine.Debug.Log("laid an egg!");
    laidEggs.Add(info);

  }
  public List<TraitSlotToTraitDictionary> GetCollectedTraitItems()
  {
    return collectedTraitItems;
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

  public void DisplayVictoryText()
  {
    playerHud.SetVictoryText();
  }

  public void ClearVictoryText()
  {
    playerHud.ClearVictoryText();
  }
  //continues running while paused, jsyk
  public static IEnumerator WaitForRealSeconds(float time)
  {
    while (time > 0)
    {
      time -= Mathf.Clamp(Time.unscaledDeltaTime, 0, .02f);
      yield return null;
    }
  }
}