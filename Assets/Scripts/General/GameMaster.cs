using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

// Not in use yet; will be used to handle things like input state, etc.
public class GameMaster : Singleton<GameMaster> {

	public CanvasHandler canvasHandler;
	public GameObject playerPrefab;
	public DialogueRunner dialogueRunner;
	public GridManager gridManager;
	private PlayerController playerController;
	private PathfindingSystem pathfinding;
	private Constants.GameState gameStatus;

	// Saved when player dies so their next life can be preserved
	private UpcomingLifeTraits cachedLarva;
	private UpcomingLifeTraits cachedPupa;

	// Use this for initialization
	void Start () {
		pathfinding = GetComponent<PathfindingSystem>();
		Respawn();
	}

	// Update is called once per frame
	void Update () {
		HandleInput();
	}

	private void HandleInput() {
		switch (gameStatus){
			case Constants.GameState.Dead:
				HandleDeadInput();
				break;
			default:
				if (Input.GetKeyDown("=")) {
					playerController.Die();
				}
				break;
		}
	}
	// TODO: Buttons instead of keys!!!
	private void HandleDeadInput() {
		if (Input.GetButtonDown("Respawn")) {
			Respawn();
		}
	}

	private void Respawn() {
		Debug.Log("Respawning...");
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player != null) {
			Debug.Log("player isn't null?");
			playerController = player.GetComponent<PlayerController>();
		} else {
			GameObject spawnPoint = ChooseSpawnPoint();
			Vector3 spawnLocation = Vector3.zero;
			if (spawnPoint != null) {
				spawnLocation = spawnPoint.transform.position;
			}
			playerController = Instantiate(playerPrefab, spawnLocation, Quaternion.identity).GetComponent<PlayerController>();
		}
		playerController.Init(cachedLarva, cachedPupa);
		Debug.Log("Respawned!");
		SetGameStatus(Constants.GameState.Play);
	}

	private GameObject ChooseSpawnPoint() {
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint"); // TODO: spawn points should register with GM
		if (spawnPoints.Length > 0) {
			return spawnPoints[0];
		}
		return null;
	}

	public PlayerController GetPlayerController() {
		return playerController;
	}

	public void SetGameStatus(Constants.GameState newStatus) {
		Debug.Log("setGameStatus "+newStatus);
		gameStatus = newStatus;
	}

	public void KillPlayer(UpcomingLifeTraits larva, UpcomingLifeTraits pupa) {
		cachedLarva = larva;
		cachedPupa = pupa;
		playerController = null;
		SetGameStatus(Constants.GameState.Dead);
	}

	public Constants.GameState GetGameStatus() {
		return gameStatus;
	}

	public List<Node> FindPath(Vector3 originPosition, TileLocation targetLocation, CharacterAI ai) {
		return pathfinding.CalculatePathToTarget(originPosition, targetLocation, ai);
	}

    public bool IsPathClearOfHazards(Collider2D col, TileLocation target, CharacterAI ai) {
		return pathfinding.IsPathClearOfHazards(col, target, ai);
	}

	public void StartDialogue(string startNode) {
		if (dialogueRunner.isDialogueRunning) { return; }
		dialogueRunner.StartDialogue(startNode);
	}

	public Vector3Int GetCellFromWorldLocation(Vector3 loc) {
		return gridManager.GetCellFromWorldLocation(loc);
	}
	public EnvironmentTile GetTileAtLocation(Vector3 loc, Constants.FloorLayer floor) {
		return gridManager.GetTileAtLocation(loc, floor);
	}

	public void ReplaceAdjacentTile(Vector3 loc,  Constants.FloorLayer floor, EnvironmentTile replacementTile, TilemapDirection direction) {
		gridManager.ReplaceAdjacentTile(loc, floor, replacementTile,direction);
	}

	public void DestroyObjectTileAtLocation(Vector3 loc, Constants.FloorLayer floor) {
		gridManager.DestroyObjectTileAtLocation(loc, floor);
	}

	public void ReplaceTileAtLocation(Vector3 loc,  Constants.FloorLayer floor, EnvironmentTile replacementTile) {
		gridManager.ReplaceTileAtLocation(loc, floor, replacementTile);
	}
}
