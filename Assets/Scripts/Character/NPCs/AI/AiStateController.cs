using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class AiStateController : Character
{
  [Header("AI State")]
  public AiState currentState;
  private bool aiActive;
  public AiState remainState;
  public bool waitingToAttack = false;
  public bool attacking
  {
    get
    {
      return waitingToAttack || usingSkill;
    }
  }
  [HideInInspector] public float timeSpentInState;

  [Header("AI Attributes")]
  public bool DEBUGStopRecalculatingPath = false;
  public float detectionRange = 5f;
  public float attackAngleInDegrees = 45f;
  public float maxTargetDistanceFromSpawnPoint = 15;
  public float minDistanceFromPathNode = .15f;

  [Tooltip("minDistanceToAttackBuffer * attack range = min distance at which we back up")]
  public float minDistanceToAttackBuffer = .10f;

  [Tooltip("attack range - (preferredAttackRangeBuffer * attack range) = max distance at which we will attack")]
  public float preferredAttackRangeBuffer = .10f;

  public float minDistanceFromTarget;
  public float blockTimer = 0;

  [HideInInspector] public Vector2 spawnLocation;
  public AttackType selectedAttackType;
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
    spawnLocation = transform.position;
    Init();
  }
  protected override void Update()
  {
    base.Update();
    timeSpentInState += Time.deltaTime;
  }

  protected override void FixedUpdate()
  {
    base.FixedUpdate();
    if (!aiActive) { return; }
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
      currentState.OnExit(this);
      currentState = nextState;
      //TODO: should we fall back if onEntry fails?
      currentState.OnEntry(this);
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
      && pathToTarget.Count > 1
      && (
        pathToTarget[1] != null
        && pathToTarget[1].loc.floorLayer == tile.GetTargetFloorLayer(currentFloor)
      )
    )
    {
      UseTile();
    }
  }

  public void StartValidateAndSetWanderDestination(Vector3 pos, FloorLayer fl, MoveAiAction initiatingAction)
  {
    if (isCalculatingPath) { return; }
    StartCoroutine(ValidateAndSetWanderDestination(pos, fl, initiatingAction));
  }

  public IEnumerator ValidateAndSetWanderDestination(Vector3 pos, FloorLayer fl, MoveAiAction initiatingAction)
  {
    TileLocation targetLocation = new TileLocation(pos, fl);
    yield return StartCoroutine(PathfindingSystem.Instance.CalculatePathToTarget(transform.TransformPoint(circleCollider.offset), targetLocation, this, initiatingAction));
    if (pathToTarget != null)
    {
      SetWanderDestination(targetLocation.cellCenterPosition, fl);
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
    // Debug.Log("unsetting override destination!");
    overrideDestination = null;
  }
  public void DelayThenSetOverrideDestination(Vector3 pos, FloorLayer fl)
  {
    StartCoroutine(DelayThenSetOverrideDestinationCoroutine(pos, fl));
  }

  public IEnumerator DelayThenSetOverrideDestinationCoroutine(Vector3 pos, FloorLayer fl)
  {
    yield return new WaitForSeconds(Random.Range(.1f, 6f));
    SetOverrideDestination(pos, fl);
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
    // if (newPath != null && newPath.Count > 1)
    // {

    //   // Debug.Log("new path first node: " + newPath[0].loc.tilemapCoordinates);
    //   // Debug.Log("current loc: " + GetTileLocation().tilemapCoordinates);
    //   // GridManager.Instance.DEBUGHighlightTile(newPath[0].loc);
    //   // GridManager.Instance.DEBUGHighlightTile(GetTileLocation(), Color.blue);
    // }
    // if (pathToTarget != null && pathToTarget.Count > 0 // if we already have a path to the target...
    // && newPath != null && newPath.Count > 1 && newPath[0].loc == GetTileLocation())
    // {
    //   Debug.Log("new path starts with current tile!");
    // }
    // if (pathToTarget != null && pathToTarget.Count > 0 // if we already have a path to the target...
    // && newPath != null && newPath.Count > 1 && newPath[0].loc == GetTileLocation()// and our current tile is the first tile in our new path...
    // && pathToTarget[0].loc == newPath[1].loc // and our current path's first tile is the SECOND tile in our new path...
    // )
    // {
    //   Debug.Log("We already did the first tile in this path! Removing it!");
    //   newPath.RemoveAt(0);
    // }


    // we should drop the first tile from our new path
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

  public bool IsCalculatingPath()
  {
    return isCalculatingPath;
  }

  public void StartCalculatingPath(TileLocation targetLocation, MoveAiAction initiatingAction, WorldObject potentialObjectOfInterest = null)
  {
    if (isCalculatingPath || DEBUGStopRecalculatingPath) { return; }
    // Debug.Log("startCalculatingPath");
    StartCoroutine(PathfindingSystem.Instance.CalculatePathToTarget(transform.TransformPoint(circleCollider.offset), targetLocation, this, initiatingAction, potentialObjectOfInterest));
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

  public bool ObjectOfInterestWithinRangeOfSpawnPoint()
  {
    return
      objectOfInterest != null
      && maxTargetDistanceFromSpawnPoint * maxTargetDistanceFromSpawnPoint > ((Vector2)objectOfInterest.transform.position - spawnLocation).sqrMagnitude;
  }

  public float GetMinPreferredAttackRange()
  {
    return GetAttackRange() * minDistanceToAttackBuffer;
  }
  public float GetMaxPreferredAttackRange(AttackType attackType)
  {
    float attackRange = GetSkillDataForAttackType(attackType).GetEffectiveRange();
    return attackRange - attackRange * preferredAttackRangeBuffer;
  }

  public float GetMaxPreferredAttackRange()
  {
    float attackRange = GetAttackRange();
    return attackRange - attackRange * preferredAttackRangeBuffer;
  }
  public void WaitThenAttack()
  {
    waitingToAttack = true;
    StartCoroutine(WaitThenAttackCoroutine());
  }
  public IEnumerator WaitThenAttackCoroutine()
  {
    blocking = selectedAttackType == AttackType.Blocking; // block if using block attack, force unblock otherwise
    yield return new WaitForSeconds(Random.Range(0.4f, 1.1f));
    Debug.Log("using skill " + GetSelectedCharacterSkill());
    if (selectedAttackType == AttackType.Critical)
    {
      StartCoroutine(UseCritAttack());
    }
    else
    {
      UseSkill(GetSelectedCharacterSkill()); // todo: fix this????
    }
    waitingToAttack = false;
  }

  protected override void TakeDamage(IDamageSource damageSource)
  {
    if (damageSource.forcesItemDrop)
    {
      SpawnDroppedItems();
    }
    base.TakeDamage(damageSource);
  }

  public override CharacterSkillData GetSelectedCharacterSkill()
  {
    return GetSkillDataForAttackType(selectedAttackType);
  }

  private void SpawnDroppedItems()
  {
    if (alreadyDroppedItems) { return; }
    alreadyDroppedItems = true;
    foreach (PickupItem item in itemDrops)
    {
      TraitPickupItem traitItem = (TraitPickupItem)item;
      if (traitItem != null)
      {
        foreach (TraitSlot slot in traits.Keys)
        {
          if (traitItem.traits.ContainsKey(slot))
          { // it definitely should
            traitItem.traits[slot] = traits[slot];
          }
        }
      }
      GameObject instantiatedItem = Instantiate(item.gameObject, transform.position, transform.rotation);
      WorldObject.ChangeLayersRecursively(instantiatedItem.transform, GetFloorLayer());
      // instantiatedItem.GetComponent<SpriteRenderer>().sortingLayerName = LayerMask.LayerToName(gameObject.layer);
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