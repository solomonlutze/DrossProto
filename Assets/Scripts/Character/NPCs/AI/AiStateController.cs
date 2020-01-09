using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AiStateController : Character
{
  [Header("AI State")]
  public AiState currentState;
  private bool aiActive;
  public AiState remainState;
  [HideInInspector] public float timeSpentInState;

  [Header("AI Attributes")]
  public float detectionRange = 5f;
  public float attackRange
  {
    get
    {
      return characterAttack.range + GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.Range);
    }
  }
  public float attackAngleInDegrees = 45f;
  [HideInInspector] public float minDistanceFromPathNode;

  public float minDistanceFromTarget;

  [HideInInspector] public Vector2 spawnLocation;

  /*
  * Travel variables
   */
  // [HideInInspector]
  public WorldObject overrideDestination;
  // [HideInInspector]
  public WorldObject objectOfInterest;
  // [HideInInspector]
  public WorldObject wanderDestination;
  public bool lineToTargetIsClear;
  public List<Node> pathToTarget;
  bool isCalculatingPath;

  private WorldObject overrideDestinationObject;
  private WorldObject wanderDestinationObject;
  public WorldObject wanderDestinationPrefab;

  private bool alreadyDroppedItems;
  public PickupItem[] itemDrops;

  protected override void Awake()
  {
    base.Awake();
    aiActive = true;
    minDistanceFromPathNode = .05f;
    spawnLocation = transform.position;
    Init();
  }
  protected override void Update()
  {
    base.Update();
    if (!aiActive) { return; }
    timeSpentInState += Time.deltaTime;
    movementInput = Vector2.zero;
    currentState.UpdateState(this);
  }


  /*
  *STATE LOGIC
  */
  public void TransitionToState(AiState nextState)
  {
    if (nextState != remainState)
    {
      timeSpentInState = 0f;
      currentState = nextState;
    }
  }

  /*
  * CHARACTER LOGIC
  */
  // Move in a straight line towards target, if able.
  // Otherwise, seek pathfinding help


  protected override void HandleTile()
  {
    base.HandleTile();
    EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
    if (tile.ChangesFloorLayer()
      && pathToTarget != null
      && pathToTarget.Count >= 1
      && (
        pathToTarget[1] != null
        && pathToTarget[1].loc.floorLayer == tile.GetTargetFloorLayer(currentFloor)
      )
    )
    {
      UseTile();
    }
  }

  public void StartValidateAndSetWanderDestination(Vector3 pos, FloorLayer fl)
  {
    if (isCalculatingPath) { return; }
    StartCoroutine(ValidateAndSetWanderDestination(pos, fl));
  }

  public IEnumerator ValidateAndSetWanderDestination(Vector3 pos, FloorLayer fl)
  {
    TileLocation targetLocation = new TileLocation(pos, fl);
    yield return StartCoroutine(PathfindingSystem.Instance.CalculatePathToTarget(transform.TransformPoint(col.offset), targetLocation, this));
    if (pathToTarget != null)
    {
      SetWanderDestination(targetLocation.tileCenter, fl);
    }
    else
    {
      UnsetWanderDestination();
    }
  }

  public void UnsetWanderDestination()
  {
    wanderDestination = null;
  }

  public void SetWanderDestination(Vector3 pos, FloorLayer fl)
  {
    if (wanderDestinationObject == null)
    {
      wanderDestinationObject = GameObject.Instantiate(wanderDestinationPrefab);
    }
    wanderDestination = wanderDestinationObject;
    wanderDestination.transform.position = pos;
    WorldObject.ChangeLayersRecursively(wanderDestination.transform, fl);
  }

  public void UnsetOverrideDestination()
  {
    overrideDestination = null;
  }
  public void SetOverrideDestination(Vector3 pos, FloorLayer fl)
  {
    if (overrideDestinationObject == null)
    {
      overrideDestinationObject = GameObject.Instantiate(wanderDestinationPrefab);
    }
    overrideDestination = overrideDestinationObject;
    overrideDestination.transform.position = pos;
    WorldObject.ChangeLayersRecursively(overrideDestination.transform, fl);
  }

  public void SetPathToTarget(List<Node> newPath)
  {
    pathToTarget = newPath;
  }

  public void SetMoveInput(Vector2 newMoveInput)
  {
    movementInput = newMoveInput;
  }
  public Vector2 GetMovementInput()
  {
    return movementInput;
  }
  public void SetIsCalculatingPath(bool flag)
  {
    isCalculatingPath = flag;
  }
  public void StartCalculatingPath(TileLocation targetLocation, WorldObject potentialObjectOfInterest = null)
  {
    if (isCalculatingPath) { return; }
    StartCoroutine(PathfindingSystem.Instance.CalculatePathToTarget(transform.TransformPoint(col.offset), targetLocation, this, potentialObjectOfInterest));
  }

  //We can reach our target if:
  // - it exists, AND
  // - our direct line is free OR we have a path OR we are calculating a path

  public bool CanReachObjectOfInterest()
  {
    return
      objectOfInterest != null
      && (pathToTarget != null || lineToTargetIsClear || isCalculatingPath);

  }

  protected override void TakeDamage_OLD(Damage_OLD damage)
  {
    if (damage.ForcesItemDrop())
    {
      SpawnDroppedItems();
    }
    base.TakeDamage_OLD(damage);
  }

  private void SpawnDroppedItems()
  {
    if (alreadyDroppedItems) { return; }
    alreadyDroppedItems = true;
    foreach (PickupItem item in itemDrops)
    {
      GameObject instantiatedItem = Instantiate(item.gameObject, transform.position, transform.rotation);
      instantiatedItem.layer = gameObject.layer;
      instantiatedItem.GetComponent<SpriteRenderer>().sortingLayerName = LayerMask.LayerToName(gameObject.layer);
    }
  }

  public override void Die()
  {
    if (!alreadyDroppedItems)
    {
      SpawnDroppedItems();
    }
    base.Die();
  }
}