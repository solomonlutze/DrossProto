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
      return waitingToAttack || UsingSkill();
    }
  }
  [HideInInspector] public float timeSpentInState;
  [HideInInspector] public float attackCooldownTimer;

  [Header("AI Attributes")]
  public bool DEBUGStopRecalculatingPath = false;
  public float attackAngleInDegrees = 45f;
  public AiSettings aiSettings;
  public float maxTargetDistanceFromSpawnPoint = 15;
  public float minDistanceFromPathNode = .15f;

  [Tooltip("minDistanceToAttackBuffer * attack range = min distance at which we back up")]
  public float minDistanceToAttackBuffer = .10f;

  [Tooltip("attack range - (preferredAttackRangeBuffer * attack range) = max distance at which we will attack")]
  public float preferredAttackRangeBuffer = .10f;

  [Tooltip("smallest, in degrees, an attack angle is allowed to be")]
  public float minimumAttackAngle = 5f;

  [Tooltip("attackAngle - (attackAngle * attackAngleBuffer) = max angle at which we will attack")]
  public float attackAngleBuffer = .1f;

  public float minDistanceFromTarget;
  public float blockTimer = 0;

  public float currentOpacity = 1;
  public bool shouldBeVisible = true;
  public float camouflageFadeTime = .25f; //maybe this should live on defaultCharacterData

  [HideInInspector] public Vector2 spawnLocation;
  // public AttackType selectedAttackType;
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
  public LayerRenderer layerRenderer;
  public bool DEBUG_AiLogging = false;

  protected override void Awake()
  {
    base.Awake();
    aiActive = true;
    spawnLocation = transform.position;
    Init();
  }
  protected override void Init()
  {
    base.Init();
    characterAttackSkills = new List<CharacterSkillData>();
    foreach (CharacterSkillData skill in characterSkills.Values)
    {
      if (skill.IsAttack())
      {
        characterAttackSkills.Add(skill);
      }
    }
  }
  protected override void Update()
  {
    base.Update();
    timeSpentInState += Time.deltaTime;
    attackCooldownTimer += Time.deltaTime;
  }

  protected override void FixedUpdate()
  {
    base.FixedUpdate();
    if (!aiActive) { return; }
    currentState.UpdateState(this);
  }


  public override void BeginSkill(CharacterSkillData skill)
  {
    if (characterAttackSkills.Contains(skill))
    {
      attackCooldownTimer = 0;
    }
    base.BeginSkill(skill);
    pressingSkill = null;
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


  protected override void HandleTile()
  {
    base.HandleTile();
    EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
    if (tile.HasSolidObject())
    {
      GridManager.Instance.DEBUGHighlightTile(tile.tileLocation, Color.magenta);
      // Debug.Log("bugstuck");
      // Debug.Break();
    }
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

  // WARNING: Tightly coupled with layerRenderer.
  // Visibility controlled by LayerRenderer if LayerRenderer wants to hide the character,
  // and by the AI otherwise.
  // The reasoning is that the character should only be visible if they're both:
  // - not camouflaged (controlled by AI), and
  // - not above the player (controlled by layerRenderer)

  // void HandleVisibility()
  // {
  //   CalculateTargetOpacity();
  //   if (!layerRenderer.shouldBeVisible)
  //   {
  //     if (GetCamouflageRange() > 0)
  //     {
  //       Debug.Log("layer renderer handling visibility for " + gameObject.name);
  //     }
  //     return; // LayerRenderer handles visability if it thinks the character should be invisible
  //   }
  //   else
  //   {
  //     // if (GetCamouflageRange() > 0)
  //     // {
  //     //   Debug.Log("currentOpacity: " + currentOpacity);
  //     // }
  //     if (!LayerRenderer.FinishedChangingOpacity(shouldBeVisible, currentOpacity))
  //     {
  //       currentOpacity += Time.deltaTime / camouflageFadeTime * (shouldBeVisible ? 1 : -1);
  //     }
  //     LayerRenderer.ChangeOpacityRecursively(transform, currentOpacity);
  //   }
  // }

  // void CalculateTargetOpacity()
  // {
  //   PlayerController player = GameMaster.Instance.GetPlayerController();
  //   if (player == null || GetCamouflageRange() <= 0)
  //   {
  //     shouldBeVisible = true;
  //     return;
  //   }
  //   float distanceFromPlayer = Vector2.SqrMagnitude(transform.position - player.transform.position);
  //   if (distanceFromPlayer > Mathf.Pow(GetCamouflageRange(), 2))
  //   {
  //     shouldBeVisible = false;
  //     return;
  //   }
  //   shouldBeVisible = true;
  // }

  public override void SetCurrentFloor(FloorLayer newFloorLayer)
  {
    base.SetCurrentFloor(newFloorLayer);
    // layerRenderer.floorLayer = newFloorLayer;
    layerRenderer.ChangeTargetOpacity();
  }

  public void StartValidateAndSetWanderDestination(Vector3 pos, FloorLayer fl, MoveAiAction initiatingAction)
  {
    if (isCalculatingPath) { return; }
    StartCoroutine(ValidateAndSetWanderDestination(pos, fl, initiatingAction));
  }

  public IEnumerator ValidateAndSetWanderDestination(Vector3 pos, FloorLayer fl, MoveAiAction initiatingAction)
  {
    pos.z = GridManager.GetZOffsetForGameObjectLayer(WorldObject.GetGameObjectLayerFromFloorLayer(fl));
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

  public bool LineToTargetIsClear(WorldObject target)
  {
    return
      PathfindingSystem.Instance.IsPathClearOfHazards(
        target.transform.position,
        target.GetFloorLayer(),
        this
      );
  }
  public bool ObjectOfInterestWithinRangeOfSpawnPoint()
  {
    return
      objectOfInterest != null
      && maxTargetDistanceFromSpawnPoint * maxTargetDistanceFromSpawnPoint > ((Vector2)objectOfInterest.transform.position - spawnLocation).sqrMagnitude;
  }

  // public float GetMinPreferredAttackRange()
  // {
  //   return GetAttackRange() * minDistanceToAttackBuffer;
  // }
  // public float GetMaxPreferredAttackRange(AttackType attackType)
  // {
  //   float attackRange = GetSkillDataForAttackType(attackType).GetEffectiveRange();
  //   return attackRange - attackRange * preferredAttackRangeBuffer;
  // }

  // public float GetMaxPreferredAttackRange()
  // {
  //   float attackRange = GetAttackRange();
  //   return attackRange - attackRange * preferredAttackRangeBuffer;
  // }

  // closer than this and _none_ of our attacks are a good idea,
  // and we should back up
  public float GetMinPreferredAttackRange()
  {
    float overallMin = 1000f;
    foreach (CharacterSkillData skillData in characterAttackSkills)
    {
      foreach (SkillRangeInfo range in skillData.CalculateRangeInfosForSkillEffectSet(this))
      {
        float min = range.minRange;
        min += (range.maxRange - min) * minDistanceToAttackBuffer;
        overallMin = Mathf.Min(min, overallMin);
      }
    }
    return overallMin;
  }


  public (float, float) GetMinAndMaxPreferredAttackRange()
  {
    float overallMin = 1000f;
    float overallMax = -1000f;
    foreach (CharacterSkillData skillData in characterAttackSkills)
    {
      foreach (SkillRangeInfo range in skillData.CalculateRangeInfosForSkillEffectSet(this))
      {
        float min = range.minRange;
        min += (range.maxRange - min) * minDistanceToAttackBuffer;
        overallMin = Mathf.Min(min, overallMin);
        float max = range.maxRange;
        overallMax = Mathf.Max(max, overallMax);
      }
    }
    return (overallMin, overallMax);
  }

  public bool TooCloseToTarget(WorldObject target)
  {
    float min = GetMinPreferredAttackRange();
    if ((transform.position - target.transform.position).sqrMagnitude < min * min
    )
    {
      Debug.Log("too close to target - min range " + min);
      return true;
    }
    return false;
  }

  public bool TooFarFromTarget(WorldObject target)
  {

    if ((transform.position - target.transform.position).sqrMagnitude > aiSettings.maxCombatDistance * aiSettings.maxCombatDistance
    )
    {
      Debug.Log("too far from target - max range " + aiSettings.maxCombatDistance);
      return true;
    }
    return false;
  }


  public bool WithinAttackRange(WorldObject target, SkillRangeInfo[] rangeInfos)
  {
    foreach (SkillRangeInfo range in rangeInfos)
    {
      float min = range.minRange;
      float max = range.maxRange;
      max -= (max - min) * preferredAttackRangeBuffer;
      min += (max - min) * minDistanceToAttackBuffer;
      if ((transform.position - target.transform.position).sqrMagnitude <= max * max
        && (transform.position - target.transform.position).sqrMagnitude >= min * min
      )
      {
        return true;
      }
    }
    return false;
  }

  public bool WithinAttackAngle(WorldObject target, SkillRangeInfo[] rangeInfos)
  {
    Vector3 targetDirection = target.transform.position - transform.position;
    return Mathf.Abs(GetAngleToDirection(targetDirection)) < aiSettings.minAttackAngle;

    foreach (SkillRangeInfo range in rangeInfos)
    {
      float min = range.minAngle;
      float max = range.maxAngle;
      if (Mathf.Abs(max - min) < minimumAttackAngle)
      {
        float d = minimumAttackAngle - (max - min);
        min -= d / 2;
        max += d / 2;
      }
      // —if abs(max - min) > 360, return true

      // —elif max > 180
      // —- true if angle between (min and 180) or (-180 and (max - 360)) 

      // —elif min < -180
      // —- true if angle between ((min + 360) and 180) or (-180 and max) 

      // —else true if angle between min and max
      // remember: angle to target always >= -180 and <= 180
      if (Mathf.Abs(max - min) > 360f)
      {
        return true; // full circle swings are always in angle
      }
      else if (max > 180f)
      {
        return GetAngleToDirection(targetDirection) >= min || max - 360 > GetAngleToDirection(targetDirection);
      }
      else if (min < -180f)
      {
        return GetAngleToDirection(targetDirection) >= min + 360 || max > GetAngleToDirection(targetDirection);
      }
      else if (GetAngleToDirection(targetDirection) >= min && GetAngleToDirection(targetDirection) <= max)
      {
        return true;
      }
    }
    return false;
  }

  public bool HasSufficientStaminaForSelectedAttack()
  {
    return true;
    // return (GetCharacterVital(CharacterVital.CurrentStamina) > GetSelectedCharacterSkill().staminaCost / 2);
  }
  public void WaitThenAttack(CharacterSkillData attack)
  {
    waitingToAttack = true;
    StartCoroutine(WaitThenAttackCoroutine(attack));
  }
  public IEnumerator WaitThenAttackCoroutine(CharacterSkillData attack)
  {
    yield return null;
    // yield return new WaitForSeconds(Random.Range(0.4f, 1.1f));
    // if (selectedAttackType == AttackType.Critical)
    // {
    //   StartCoroutine(UseCritAttack());
    // }
    // else
    {
      HandleSkillInput(attack);
    }
    waitingToAttack = false;
  }

  protected override void TakeDamage(IDamageSource damageSource)
  {
    base.TakeDamage(damageSource);
  }

  // public override CharacterSkillData GetSelectedCharacterSkill()
  // {
  //   return GetSkillDataForAttackType(selectedAttackType);
  // }

  private void SpawnDroppedItems()
  {
    if (GameMaster.Instance.DEBUG_dontDropItems) { return; }
    if (alreadyDroppedItems) { return; }
    Debug.Log("dropping item");
    alreadyDroppedItems = true;
    foreach (PickupItem item in itemDrops)
    {
      TraitPickupItem traitItem = (TraitPickupItem)item;
      Debug.Log("dropping item " + traitItem);
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
      Debug.Log("dropped item " + instantiatedItem);
      WorldObject.ChangeLayersRecursively(instantiatedItem.transform, GetFloorLayer());
      instantiatedItem.transform.position = new Vector3(instantiatedItem.transform.position.x, instantiatedItem.transform.position.y, GridManager.GetZOffsetForGameObjectLayer(instantiatedItem.layer));
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