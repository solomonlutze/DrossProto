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
    timeSpentInState += Time.deltaTime;
    movementInput = Vector2.zero;
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

  public void StartValidateAndSetWanderDestination(Vector3 pos, FloorLayer fl)
  {
    if (isCalculatingPath) { return; }
    StartCoroutine(ValidateAndSetWanderDestination(pos, fl));
  }

  public IEnumerator ValidateAndSetWanderDestination(Vector3 pos, FloorLayer fl)
  {
    TileLocation targetLocation = new TileLocation(pos, fl);
    yield return StartCoroutine(PathfindingSystem.Instance.CalculatePathToTarget(transform.TransformPoint(circleCollider.offset), targetLocation, this));
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
    // Debug.Log("unsetting override destination!");
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
    StartCoroutine(PathfindingSystem.Instance.CalculatePathToTarget(transform.TransformPoint(circleCollider.offset), targetLocation, this, potentialObjectOfInterest));
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

  public float GetMinPreferredAttackRange()
  {
    return characterAttack.ai_preferredMinAttackRange;
  }

  public float GetMaxPreferredAttackRange()
  {
    return weaponInstance.effectiveRange - characterAttack.ai_preferredAttackRangeBuffer;
  }

  protected override void TakeDamage(IDamageSource damageSource)
  {
    if (damageSource.forcesItemDrop)
    {
      SpawnDroppedItems();
    }
    base.TakeDamage(damageSource);
  }

  private void SpawnDroppedItems()
  {
    Debug.Log("spawn dropped items!");
    if (alreadyDroppedItems) { return; }
    alreadyDroppedItems = true;
    foreach (PickupItem item in itemDrops)
    {
      Debug.Log("Dropping items!");
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
      Debug.Log("spawning " + item);
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