using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Tilemaps;

// TODO: Character should probably extend CustomPhysicsController, which should extend WorldObject
public class Character : WorldObject
{
  public static float BASE_SCRAMBLE_VELOCITY = 3.5f;
  public static float SCRAMBLE_MAX_ANGLE = 45;
  public static float REVERSE_DIRECTION_INPUT_DELAY = .6f;

  [Header("Stats and Vitals")]
  public CharacterType characterType;
  public CharacterVitalToFloatDictionary vitals;
  public StatToActiveStatModificationsDictionary statModifications;
  public DamageTypeToFloatDictionary damageTypeResistances;

  [Header("Attack Info")]
  public TraitSlotToCharacterSkillDataDictionary characterSkills;
  public Dictionary<string, TraitSlot> traitSlotsForSkills;
  public List<CharacterSkillData> characterAttackSkills;
  public CharacterSkillData hopSkill;
  public CharacterSkillData moltSkill;
  public List<CharacterSkillData> characterSpells;// possibly deprecated
  public CharacterAttackModifiers attackModifiers; // probably deprecated
  private bool dashAttackEnabled = true;
  public CharacterAttackModifiers dashAttackModifiers;
  private float attackChargingTime = 0;
  [Header("Trait Info", order = 1)]
  public TraitSlotToTraitDictionary traits;
  public CharacterAttributeToIntDictionary attributes;
  public List<TraitEffect> conditionallyActivatedTraitEffects;
  public List<TraitEffect> activeConditionallyActivatedTraitEffects;
  public List<CharacterMovementAbility> activeMovementAbilities;

  [Header("Child Components")]
  public Transform orientation;

  public Transform weaponPivotRoot;
  public Transform crosshair;
  // our physics object
  public CustomPhysicsController po;
  // animation object
  public Animator animator;
  public SpriteRenderer[] renderers;

  public CircleCollider2D circleCollider;
  public BoxCollider2D boxCollider; // used for calculating collisions w/ tiles while changing floor layer
  public PolygonCollider2D physicsCollider; // used for calculating collisions w/ actual objects
  public PolygonCollider2D touchingCollider; // used for eg. deciding if we're touching a wall
  public CharacterVisuals characterVisuals;
  // public AnimatorController animatorController;
  public VisualEffect chargingUpParticleSystem;
  public VisualEffect chargeLevelIncreaseParticleSystem;
  public VisualEffect fullyChargedParticleSystem;
  public VisualEffect bloodSplashParticleSystem;
  public GameObject attackReadyIndicatorObject;

  [Header("Game State Info")]
  public Vector3 previousPosition = Vector3.zero;
  public float distanceFromPreviousPosition = 0;
  public Color damageFlashColor = Color.red;
  public Character critTarget = null;
  public Character critVictimOf = null; // true while subject to crit attack
  public bool usingCrit = false;
  public float damageFlashSpeed = 1.0f;
  public CharacterSkillData activeSkill;
  public CharacterSkillData lastActiveSkill;
  public CharacterSkillData queuedSkill;
  public CharacterSkillData pressingSkill;
  public List<Weapon> activeWeaponObjects;

  public float timeSpentInSkillEffect = 0f;
  public int currentSkillEffectSetIndex = 0;
  public int currentSkillEffectIndex = 0;
  public int currentAttackSpawnIndex = 0;
  public bool molting = false;
  public float easedSkillForwardMovementProgressIncrement = 0.0f;
  public float easedSkillUpwardMovementProgressIncrement = 0.0f;
  public float knockbackAmount = 0.0f;
  public Vector2 knockbackHeading = Vector3.zero;
  public float knockbackProgress = 0.0f;
  public float easedKnockbackProgressIncrement = 0.0f;
  public float dashRecoveryTimer = 0.0f;
  protected bool stunned = false;
  public bool carapaceBroken = false;
  public bool animationPreventsMoving = false;
  public bool sticking = false;
  public Coroutine skillCoroutine;
  protected Coroutine flyCoroutine;
  public Vector2 movementInput;
  // point in space we would like to face
  public Vector3 orientTowards;
  protected TileLocation currentTileLocation;
  public EnvironmentTileInfo currentTile;
  protected TileLocation lastSafeTileLocation;
  protected float timeStandingStill = 0;
  protected float timeMoving = 0;
  protected Dictionary<string, GameObject> traitSpawnedGameObjects;
  public AscendingDescendingState ascendingDescendingState = AscendingDescendingState.None;
  public Dictionary<DamageType, ElementalDamageBuildup> elementalDamageBuildups;
  public Dictionary<TraitSlot, PartStatusInfo> partStatusInfos;
  public Dictionary<TraitSlot, bool> brokenParts;
  public bool ascending
  {
    get { return ascendingDescendingState == AscendingDescendingState.Ascending; }
  }
  public bool descending
  {
    get { return ascendingDescendingState == AscendingDescendingState.Descending; }
  }
  public float ascendDescendSpeed = 1f;
  protected List<string> sourceInvulnerabilities;
  public float footstepCooldown = 0.0f;
  public float maxFootstepCooldown = 0.2f;
  public float reverseDirectionRotationCooldown = 0.0f;

  public bool dashAttackQueued = false;
  public int chargeLevel = 0;

  [Header("Default Info")]
  public CharacterData defaultCharacterData;

  private Coroutine damageFlashCoroutine;

  public Color damagedColor;

  public CombatJuiceData combatJuiceConstants;
  [Header("Prefabs")]
  public MoltCasting moltCasingPrefab;
  public GameObject bloodSplatterPrefab;

  protected virtual void Awake()
  {
    orientation = transform.Find("Orientation");
    circleCollider = GetComponent<CircleCollider2D>();
    boxCollider = GetComponent<BoxCollider2D>();
    physicsCollider = GetComponent<PolygonCollider2D>();
    animator = characterVisuals.GetComponent<Animator>();
    if (orientation == null)
    {
      Debug.LogError("No object named 'Orientation' on Character object: " + gameObject.name);
      return;
    }
  }

  protected virtual void Start()
  {
    movementInput = Vector2.zero;
    orientTowards = Vector3.zero;
    if (po == null)
    {
      Debug.LogError("No physics controller component on Character object: " + gameObject.name);
    }
    SetCurrentFloor(currentFloor);
    transform.position = new Vector3(transform.position.x, transform.position.y, GridManager.GetZOffsetForGameObjectLayer(gameObject.layer));
  }

  protected virtual void Init()
  {
    characterVisuals.SetCharacterVisuals(traits);
    sourceInvulnerabilities = new List<string>();
    conditionallyActivatedTraitEffects = new List<TraitEffect>();
    elementalDamageBuildups = new Dictionary<DamageType, ElementalDamageBuildup>();
    partStatusInfos = Utils.InitializeEnumDictionary<TraitSlot, PartStatusInfo>();
    foreach (PartStatusInfo info in partStatusInfos.Values)
    {
      info.Init(this);
    }
    brokenParts = new Dictionary<TraitSlot, bool>();
    ascendingDescendingState = AscendingDescendingState.None;
    traitSpawnedGameObjects = new Dictionary<string, GameObject>();
    characterSkills = CalculateSkills(traits);
    traitSlotsForSkills = CalculateTraitSlotsForSkills();
    // attributes = CalculateAttributes(traits);
    AwarenessTrigger awareness = GetComponentInChildren<AwarenessTrigger>();

    float awarenessRange = GetAwarenessRange();
    if (awareness != null && awarenessRange > 0)
    {
      Debug.Log("initializing awareness");
      awareness.Init(this);
      awareness.transform.localScale = new Vector3(awarenessRange, 1, 1);
    }
    lastActiveSkill = characterSkills[TraitSlot.Head];
    lastSafeTileLocation = currentTileLocation;
    InitializeFromCharacterData();
    InitializeAnimationParameters();
  }

  public void RestoreBrokenParts()
  {
    brokenParts.Clear();
    InitializeVisuals();
  }
  public void InitializeVisuals()
  {
    characterVisuals.SetCharacterVisuals(traits);
    ChangeLayersRecursively(transform, currentFloor);
  }
  private void InitializeFromCharacterData()
  {
    if (defaultCharacterData != null)
    {
      CharacterData dataInstance = (CharacterData)ScriptableObject.Instantiate(defaultCharacterData);
      attackModifiers = dataInstance.attackModifiers;
      dashAttackModifiers = dataInstance.dashAttackModifiers;
      damageTypeResistances = dataInstance.damageTypeResistances;
      activeMovementAbilities.AddRange(dataInstance.movementAbilities);
    }
    vitals = new CharacterVitalToFloatDictionary();
    vitals[CharacterVital.CurrentMaxHealth] = defaultCharacterData.defaultStats[CharacterStat.MaxHealth];
    vitals[CharacterVital.CurrentHealth] = GetCurrentMaxHealth();
    vitals[CharacterVital.CurrentStamina] = GetMaxStamina();
    vitals[CharacterVital.CurrentCarapace] = GetMaxCarapace();
  }

  protected virtual void InitializeAttributes()
  {
    foreach (Trait trait in traits.Values)
    {
      if (trait == null) { continue; }
      foreach (CharacterAttribute attribute in trait.attributeModifiers.Keys)
      {
        attributes[attribute] += trait.attributeModifiers[attribute];
      }
    }
  }

  public static CharacterAttributeToIntDictionary CalculateAttributes(TraitSlotToTraitDictionary traits)
  {

    CharacterAttributeToIntDictionary ret = new CharacterAttributeToIntDictionary(true);
    return ret;
    foreach (Trait trait in traits.Values)
    {
      if (trait == null) { continue; }
      foreach (CharacterAttribute attribute in trait.attributeModifiers.Keys)
      {
        ret[attribute] += trait.attributeModifiers[attribute];
      }
    }
  }

  public static TraitSlotToCharacterSkillDataDictionary CalculateSkills(TraitSlotToTraitDictionary traits)
  {
    TraitSlotToCharacterSkillDataDictionary ret = new TraitSlotToCharacterSkillDataDictionary();
    foreach (TraitSlot traitKey in traits.Keys)
    {
      Trait trait = traits[traitKey];
      if (trait == null) { continue; }
      if (trait.skill != null)
      {
        ret[traitKey] = trait.skill;
      }
    }
    return ret;
  }

  public Dictionary<string, TraitSlot> CalculateTraitSlotsForSkills()
  {
    Dictionary<string, TraitSlot> ret = new Dictionary<string, TraitSlot>();
    foreach (TraitSlot slot in characterSkills.Keys)
    {
      ret[characterSkills[slot].id] = slot;
    }
    return ret;
  }
  void InitializeAnimationParameters()
  {
    animator.SetFloat("HeadAnimationType", (int)traits[TraitSlot.Head].bugSpecies_DEPRECATED);
    animator.SetFloat("ThoraxAnimationType", (int)traits[TraitSlot.Thorax].bugSpecies_DEPRECATED);
    animator.SetFloat("AbdomenAnimationType", (int)traits[TraitSlot.Abdomen].bugSpecies_DEPRECATED);
    animator.SetFloat("LegsAnimationType", (int)traits[TraitSlot.Legs].bugSpecies_DEPRECATED);
    animator.SetFloat("WingsAnimationType", (int)traits[TraitSlot.Wings].bugSpecies_DEPRECATED);
  }
  // non-physics biz
  protected virtual void Update()
  {
    distanceFromPreviousPosition = Vector3.Distance(transform.position, previousPosition);
    previousPosition = transform.position;
    HandleHealth();
    if (timeMoving > 0) // better way to do this??
    {
      HandleFacingDirection();
    }
    HandleTile();
    HandleSkills();
    HandleCooldowns();
    HandleConditionallyActivatedTraits();
    HandleVerticalMotion(); // it's DIFFERENT OK
  }

  // physics biz. phbyzics
  protected virtual void FixedUpdate()
  {
    HandleAscendOrDescend();
    HandleSkillMovement();
    HandleKnockbackCooldown();
    CalculateMovement();
  }

  public static AttackType GetAttackTypeForTraitSlot(TraitSlot slot)
  {
    switch (slot)
    {
      case TraitSlot.Head:
        return AttackType.Basic;
      case TraitSlot.Thorax:
        return AttackType.Critical;
      case TraitSlot.Abdomen:
        return AttackType.Charge;
      case TraitSlot.Legs:
        return AttackType.Dash;
      case TraitSlot.Wings:
        return AttackType.Blocking;
      default:
        return AttackType.Basic;
    }
  }

  public void HandleSkillInput(CharacterSkillData skill)
  {
    QueueSkill(skill);
    PressSkill(skill);
    if (!UsingSkill())
    {
      if (CanUseSkill(skill))
      {
        BeginSkill(skill);
      }
      queuedSkill = null;
    }
    else if (activeSkill.CanAdvanceSkillEffectSet(this) && queuedSkill == activeSkill)
    {
      if (CanUseSkill(skill, currentSkillEffectIndex + 1))
      {
        AdvanceSkillEffectSet();
      }
      queuedSkill = null;
    }
    else if (activeSkill.SkillIsCancelable(this))
    {
      if (CanUseSkill(skill))
      {
        InterruptSkill(skill);
      }
      queuedSkill = null;
    }
    // Otherwise: we ARE using a skill, and we aren't interrupting its effect. Leave queued skill alone.
  }

  // if (skill == activeSkill && activeSkill.SkillIsAdvanceable(this))
  //   {
  //     Debug.Log("advancing skill effect...");
  //     AdvanceSkillEffect();
  //   }
  //   if (skill != null && CanUseSkill(skill))
  //   {
  //     if (UsingSkill())
  //     {
  //       if (activeSkill.SkillIsInterruptable(this))
  //       {
  //         InterruptSkill(skill);
  //       }
  //       else
  //       {
  //         QueueSkill(skill);
  //       }
  //     }
  //     else
  //     {
  //       BeginSkill(skill);
  //     }
  //   }
  // }


  // TODO: Differentiate between advancing the skill effect itself
  // and advancing the skill effect set.
  // When the current effect expires, 
  // -if there's more effects in the set, advance to the next effect
  // -else, determine whether we should advance to the next set
  // the below method is probably closer to AdvanceSkillEffectSet
  public void AdvanceSkillEffectSet()
  {
    currentSkillEffectIndex = 0;
    // bool repeatSet = activeSkill.SkillEffectSetIsRepeatable(this) && (pressingSkill == activeSkill || queuedSkill == activeSkill);
    // if (repeatSet && ShouldUseSkillEffectSet(activeSkill, currentSkillEffectSetIndex) && CanUseSkill(activeSkill, currentSkillEffectSetIndex))
    // {
    //   BeginSkillEffect();
    //   queuedSkill = null;
    //   return;
    // }
    while (currentSkillEffectSetIndex < activeSkill.skillEffectSets.Length - 1)
    {
      currentSkillEffectSetIndex++;
      if (ShouldUseSkillEffectSet(activeSkill, currentSkillEffectSetIndex) && CanUseSkill(activeSkill, currentSkillEffectSetIndex))
      {
        BeginSkillEffect();
        queuedSkill = null;
        return;
      }
    };
    EndSkill();
  }

  public void AdvanceSkillEffect()
  {
    activeSkill.EndSkillEffect(this);
    bool repeatSkillEffect = activeSkill.SkillEffectIsRepeatable(this) && (pressingSkill == activeSkill || queuedSkill == activeSkill);
    if (repeatSkillEffect && ShouldUseSkillEffectSet(activeSkill, currentSkillEffectSetIndex) && CanUseSkill(activeSkill, currentSkillEffectSetIndex))
    {
      BeginSkillEffect();
      queuedSkill = null;
      return;
    }
    currentSkillEffectIndex++;
    while (currentSkillEffectIndex <= activeSkill.GetActiveSkillEffectSet(this).skillEffects.Length - 1)
    {
      if (!activeSkill.GetShouldExecute(this))
      {
        currentSkillEffectIndex++;
        continue;
      }
      if ((queuedSkill == activeSkill || pressingSkill == activeSkill) && activeSkill.CanAdvanceSkillEffectSet(this))
      {
        AdvanceSkillEffectSet();
        return;
      }
      BeginSkillEffect();
      return;
    };
    AdvanceSkillEffectSet();
  }

  public void BeginSkillEffect()
  {
    timeSpentInSkillEffect = 0;
    activeSkill.BeginSkillEffect(this);
  }

  public virtual void EndSkill()
  {
    bool repeatSkill = activeSkill && activeSkill.isRepeatable && (pressingSkill == activeSkill || queuedSkill == activeSkill) && CanUseSkill(activeSkill);
    if (UsingSkill())
    {
      activeSkill.EndSkillEffect(this);
      activeSkill.CleanUp(this);
    }
    currentSkillEffectSetIndex = 0;
    currentSkillEffectIndex = 0;
    timeSpentInSkillEffect = 0;
    chargeLevel = 0;
    if (repeatSkill)
    {
      BeginSkill(activeSkill);
    }
    else
    {
      activeSkill = null;
      pressingSkill = null;
    }
  }

  public bool UsingSkill()
  {
    return activeSkill != null;
  }
  public bool InCrit()
  {
    return usingCrit || critVictimOf != null;
  }

  public void SetIsCritVictimOf(Character c)
  {
    critVictimOf = c;
  }

  public IEnumerator UseCritAttack()
  {
    Character victim = critTarget;
    victim.SetIsCritVictimOf(this);
    usingCrit = true;
    orientation.rotation = GetDirectionAngle(critTarget.transform.position);
    victim.orientation.rotation = victim.GetDirectionAngle(transform.position);
    // HandleSkillInput(GetSkillDataForAttackType(AttackType.Critical));
    while (skillCoroutine != null)
    {
      yield return null;
    }
    if (victim != null)
    {
      victim.SetIsCritVictimOf(null);
    }
    usingCrit = false;
  }

  public virtual void BeginSkill(CharacterSkillData skill)
  {
    queuedSkill = null;
    activeSkill = skill;
    if (characterSkills.Values.Contains(skill) && !brokenParts.ContainsKey(traitSlotsForSkills[skill.id]))
    {
      lastActiveSkill = skill;
    }
    if (skill.SkipIfAirborne(this) && IsMidair())
    {
      AdvanceSkillEffectSet();
    }
    skill.BeginSkillEffect(this);
  }

  public void InterruptSkill(CharacterSkillData skill)
  {
    activeSkill.CleanUp(this);
    if (activeSkill.ReversesDirectionOnCancel(this))
    {
      Vector3 rot = orientation.rotation.eulerAngles;
      rot = new Vector3(rot.x, rot.y, rot.z + 180);
      orientation.rotation = Quaternion.Euler(rot);
      Input.ResetInputAxes();
      reverseDirectionRotationCooldown = REVERSE_DIRECTION_INPUT_DELAY;
      // orientation.rotation = Quaternion.AngleAxis(180, Vector3.forward);
    }
    currentSkillEffectIndex = 0;
    currentSkillEffectSetIndex = 0;
    timeSpentInSkillEffect = 0;

    BeginSkill(skill);
  }

  public void QueueSkill(CharacterSkillData skill)
  {
    queuedSkill = skill;
  }

  public void PressSkill(CharacterSkillData skill)
  {
    pressingSkill = skill;
  }

  public SkillRangeInfo[] GetAttackRangeForSkill(CharacterSkillData skillData)
  {
    return skillData.CalculateRangeInfosForSkillEffectSet(this, activeSkill == skillData ? currentSkillEffectIndex + 1 : 0);
  }


  public int GetAttackRadiusInDegrees(int skillIdxForAttack)
  {
    Debug.LogError("Something's trying to get attack radius in degrees. Time to implement it?");
    return 0;
  }

  public void ApplyAttackModifier(CharacterAttackModifiers mods, bool forDashAttack)
  {
    dashAttackEnabled |= forDashAttack;
    CharacterAttackModifiers modsToAdjust = forDashAttack ? dashAttackModifiers : attackModifiers;
    modsToAdjust.forcesLymphDrop |= mods.forcesLymphDrop;
    foreach (CharacterAttackValue val in mods.attackValueModifiers.Keys)
    {
      int existingVal = modsToAdjust.attackValueModifiers.ContainsKey(val) ? modsToAdjust.attackValueModifiers[val] : 0;
      modsToAdjust.attackValueModifiers[val] = existingVal + mods.attackValueModifiers[val];
    }
  }

  public void SetAnimationInput(Vector2 newAnimationInput)
  {
    po.SetAnimationInput(newAnimationInput);
  }

  public void SetAnimationPreventsMoving(bool newAnimationPreventsMoving)
  {
    animationPreventsMoving = newAnimationPreventsMoving;
  }

  // Point character towards a rotation target.
  void HandleFacingDirection()
  {
    if (
      (animationPreventsMoving || stunned || carapaceBroken || reverseDirectionRotationCooldown > 0)
      && !activeMovementAbilities.Contains(CharacterMovementAbility.Halteres)
    )
    {
      Debug.Log("can't turn!");
      return;
    }
    if (movementInput != Vector2.zero)
    {
      Quaternion targetDirection = GetTargetDirection();
      // orientation.rotation = Quaternion.Slerp(orientation.rotation, targetDirection, GetRotationSpeed() * Time.deltaTime);
      orientation.rotation = Quaternion.RotateTowards(orientation.rotation, targetDirection, GetRotationSpeed() * Time.deltaTime);
    }
    // Debug.Log("rotation: " + orientation.rotation.eulerAngles + ", targetDirection: " + targetDirection.eulerAngles);
  }

  // Rotate character smoothly towards a particular orientation around the Z axis.
  // Warning: I don't understand this math. If character rotation seems buggy, this is a
  // potential culprit.
  Quaternion GetTargetDirection()
  {
    return GetDirectionAngle(movementInput);
    // return GetDirectionAngle(orientTowards);
  }

  // Rotate character smoothly towards a particular orientation around the Z axis.
  // Warning: I don't understand this math. If character rotation seems buggy, this is a
  // potential culprit.
  public Quaternion GetDirectionAngle(Vector3 targetPoint)
  {
    Vector2 target = targetPoint;
    float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    return Quaternion.AngleAxis(angle, Vector3.forward);
  }

  // // used to calculate how far our facing direction is from our target facing direction.
  // // Useful for e.g. deciding if an enemy is facing a player enough for attacking to be a good idea.
  // public float GetAngleToTarget(Vector3 targetPoint)
  // {
  //   return Quaternion.Angle(GetTargetDirection(), orientation.rotation);
  // }

  public float GetAngleToDirection(Vector3 targetPoint)
  {
    return Quaternion.Angle(GetDirectionAngle(targetPoint), orientation.rotation);
  }

  // add input to our velocity, if necessary/possible.
  protected void CalculateMovement()
  {
    if (CanMove())
    {
      if (!UsingMovementSkill() && (movementInput == Vector2.zero))
      { // should be an approximate equals
        animator.SetBool("IsWalking", false);
        timeStandingStill += Time.deltaTime;
        timeMoving = 0;
      }
      else
      {
        animator.SetBool("IsWalking", true);
        timeMoving += Time.deltaTime;
        timeStandingStill = 0f;
      }
    }
    if (IsInKnockback())
    {
      po.SetMovementInput(knockbackHeading);
    }
    else if (CanMove())
    {
      po.SetMovementInput(new Vector2(movementInput.x, movementInput.y));
    }
    else
    {
      po.SetMovementInput(Vector2.zero);
    }
  }

  // Should mainly be used for ascending/descending.
  // Ideally, should only be used to avoid collision.
  protected void CenterCharacterOnCurrentTile()
  {
    transform.position = new Vector3(
      currentTileLocation.cellCenterWorldPosition.x,
      currentTileLocation.cellCenterWorldPosition.y,
      transform.position.z
    );
  }

  protected IEnumerator Molt()
  {
    if (GetMaxCarapaceLostPerMolt() < GetCurrentMaxCarapace())
    {
      molting = true;
      yield return new WaitForSeconds(GetStat(CharacterStat.MoltDuration));
      DoMolt();
      molting = false;
    }
  }

  protected void DoMolt()
  {
    AdjustCurrentMoltCount(1);
    AdjustCurrentCarapace(100);
  }

  public bool UsingMovementSkill()
  {
    return activeSkill && activeSkill.SkillMovesCharacter(this);
  }
  public bool UsingForwardMovementSkill()
  {
    return activeSkill && activeSkill.SkillMovesCharacterForward(this);
  }
  public bool UsingVerticalMovementSkill()
  {
    return activeSkill && activeSkill.SkillMovesCharacterVertically(this);
  }

  public bool HasMovementAbility(CharacterMovementAbility requiredAbility)
  {
    return activeSkill && activeSkill.SkillHasMovementAbility(this, requiredAbility);
  }
  public bool DashingPreventsDamage()
  {
    return UsingMovementSkill() && defaultCharacterData.GetDashAttributeData().GetDashingPreventsDamage(this);
  }
  public bool DashingPreventsFalling()
  {
    return UsingMovementSkill() && defaultCharacterData.GetDashAttributeData().GetDashingPreventsFalling(this);
  }

  public bool IsInKnockback()
  {
    return knockbackAmount > 0;
  }
  public bool IsUsingMovementSkillOrInKnockback()
  {
    return UsingMovementSkill() || IsInKnockback();
  }

  public void Dash()
  {
    // BeginDash();
  }
  private Vector3 dashStartPoint;
  // protected void BeginDash()
  // {
  //   dashing = true;
  //   // dashStartPoint = transform.position;
  //   AdjustCurrentStamina(-GetDashStaminaCost());
  // }

  // public float GetEasedSkillMovementProgress()
  // {
  //   if (UsingMovementSkill())
  //   {
  //     return Easing.Quadratic.Out(timeSpentInSkillEffect / activeSkill.GetActiveEffectDuration(this));
  //   }
  //   Debug.LogError("somebody's trying to get easedSkillMovementProgress when we aren't dashing");
  //   return 0; // try not to do this please
  // }

  public float CalculateCurveProgressIncrement(NormalizedCurve curve, bool usePhysicsTimestep, bool isContinuous = false)
  {
    float timestep = usePhysicsTimestep ? Time.fixedDeltaTime : Time.deltaTime;
    float duration = activeSkill.GetActiveEffectDuration(this);
    if (isContinuous)
    {
      if (activeSkill.GetActiveEffectMaxDuration(this) == 0)
      {
        return curve.magnitude.Resolve(this) * timestep;
      }
      duration = activeSkill.GetActiveEffectMaxDuration(this);
    }
    return curve.Evaluate(this, Mathf.Min(timeSpentInSkillEffect / duration, 1))
    - curve.Evaluate(this, Mathf.Max((timeSpentInSkillEffect - timestep) / duration, 0));
  }

  protected void BeginKnockback(Vector3 knockbackMagnitude)
  {
    // EndDash(); TODO: this should probably end some skills
    knockbackAmount = knockbackMagnitude.magnitude;
    knockbackHeading = knockbackMagnitude.normalized;
  }
  public float GetEasedKnockbackProgress()
  {
    return Easing.Quadratic.Out(knockbackProgress / GetKnockbackDuration());
  }

  public float GetKnockbackDuration()
  {
    return defaultCharacterData.knockbackRate * knockbackAmount;
  }

  public float GetEasedKnockbackProgressIncrement()
  {
    return easedKnockbackProgressIncrement;
  }

  public float GetEasedSkillForwardMovementProgressIncrement()
  {
    return easedSkillForwardMovementProgressIncrement;
  }
  public float GetEasedMovementProgressIncrement()
  {
    if (IsInKnockback())
    {
      return easedKnockbackProgressIncrement;
    }
    else if (UsingMovementSkill())
    {
      return easedSkillForwardMovementProgressIncrement;
    }
    else
    {
      Debug.LogError("Trying to get movement progress increment when neither dashing or knocked back - mods???");
      return 0;
    }
  }
  protected void EndKnockback()
  {
    knockbackAmount = 0;
    easedKnockbackProgressIncrement = 0;
    knockbackProgress = 0;
  }

  protected void AscendOneFloor()
  {
    StartAscentOrDescent(AscendingDescendingState.Ascending);
  }

  protected void DescendOneFloor()
  {
    StartAscentOrDescent(AscendingDescendingState.Descending);
  }

  public void StartAscentOrDescent(AscendingDescendingState ascendOrDescend)
  {
    CenterCharacterOnCurrentTile();
    if (ascendingDescendingState != AscendingDescendingState.None) { return; }
    ascendingDescendingState = ascendOrDescend;
    if (descending)
    {
      SetCurrentFloor(currentFloor - 1);
    }
  }

  public bool IsMidair()
  {
    return ascendingDescendingState != AscendingDescendingState.None || GetMinDistanceFromOverlappingFloorTiles() >= .0001;
  }

  // Note: increment should be positive for ascent, and negative for descent.
  // You know, what we expect!
  // this function will handle translating that into the fucked-up reality of our level layout situation
  void AdjustVerticalPosition(float increment)
  {
    if (increment == 0) { return; }

    if (GetNormalizedZOffsetFromCurrentFloorLayer(increment) < 0)
    { // attempting to go down a floor
      if (!CanPassThroughFloorLayer(currentFloor))
      {
        BecomeGrounded();
        return;
      }
      SetCurrentFloor(currentFloor - 1);
    }
    if (GetMinDistanceFromOverlappingFloorTiles(increment) < 0)
    {
      BecomeGrounded();
      return;
    }
    else if (GetNormalizedZOffsetFromCurrentFloorLayer(increment) > 1)
    {
      if (!CanPassThroughFloorLayer(currentFloor + 1))
      {
        EndSkill();
        return;
      }
      SetCurrentFloor(currentFloor + 1);
    }
    else if (ShouldCollideWithCeilingTile(increment))
    {
      EndSkill();
      return;
    }
    transform.position += new Vector3(0, 0, -increment);
  }

  public void BecomeGrounded()
  {
    transform.position = new Vector3(transform.position.x, transform.position.y, GridManager.GetZOffsetForGameObjectLayer(gameObject.layer) - GetMaxFloorHeight());
    if (activeSkill && activeSkill.EndOnGrounded(this))
    {
      AdvanceSkillEffect();
    }
  }
  // this returns numbers you'd expect - positive if above, negative if below.
  float GetZOffsetFromCurrentFloorLayer(float withIncrement = 0)
  {
    return GridManager.GetZOffsetForGameObjectLayer(gameObject.layer) - (transform.position.z - withIncrement);
  }

  // this returns numbers you'd expect - positive if above, negative if below.
  // note that increment should NOT be normalized here!
  protected float GetNormalizedZOffsetFromCurrentFloorLayer(float withIncrement = 0)
  {
    //eg layer offset is -50, position is -57.5
    // we are 3/4 of the way to the next floor, so normalized offset is .75
    // (position - layer offset) = -7.5
    // - (position - layerOffset) = 7.5
    // -(position - layerOffset) / z_spacing = .75
    return -((transform.position.z - GridManager.GetZOffsetForGameObjectLayer(gameObject.layer) - withIncrement) / GridConstants.Z_SPACING);
  }

  float GetMinDistanceFromOverlappingFloorTiles(float withIncrement = 0)
  {
    HashSet<EnvironmentTileInfo> overlappingTiles = GetOverlappingTiles(currentFloor);
    float minDistance = 1;
    foreach (EnvironmentTileInfo tile in overlappingTiles)
    {
      if (!tile.IsEmpty())
      {
        minDistance = Mathf.Min(GetDistanceFromFloorTile(tile.tileLocation, withIncrement), minDistance);
      }
    }
    return minDistance;
  }

  // NOTE: only works for current floor!!
  bool IsOverlappingWithCeilingTile()
  {
    foreach (WallObject wallObject in GetOverlappingWallObjects(currentFloor))
    {
      if (wallObject != null && wallObject.ceilingTile != null)
      {
        return true;
      }
    }
    return false;
  }

  bool ShouldCollideWithCeilingTile(float increment = 0)
  {
    foreach (EnvironmentTileInfo tile in GetOverlappingTiles(currentFloor))
    {
      if (tile != null && GridManager.Instance.CeilingHasCollisionWith(tile, transform.position.z - increment))
      {
        return true;
      }
    }
    return false;
  }

  public float GetDistanceFromFloorTile(TileLocation loc, float withIncrement = 0)
  {
    return GetZOffsetFromCurrentFloorLayer(withIncrement) - GridManager.Instance.GetFloorHeightForTileLocation(loc);
  }

  float GetDistanceFromCeilingTile(TileLocation loc, float withIncrement = 0)
  {
    return GridManager.Instance.GetCeilingHeightForTileLocation(loc) - GetZOffsetFromCurrentFloorLayer(withIncrement);
  }

  public void HandleAscendOrDescend()
  {

    float increment = (1 / ascendDescendSpeed * Time.deltaTime);
    if (ascending)
    {
      if (GetZOffsetFromCurrentFloorLayer(increment) > 1)
      {
        // we will have arrived!
        SetCurrentFloor(currentFloor + 1);
        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z));
        ascendingDescendingState = AscendingDescendingState.None;
        return;
      }
      AdjustVerticalPosition(increment);
    }
    else if (descending)
    {
      if (GetZOffsetFromCurrentFloorLayer(-increment) < 0)
      {
        // we will have arrived!
        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z));
        ascendingDescendingState = AscendingDescendingState.None;
        return;
      }
      AdjustVerticalPosition(-increment);
    }
  }

  // TODO: probably a lot
  // - DRY up checking for arrival at next floor and rounding position if so
  // - include skill motion
  // - make sure there's ceiling checks on everything
  // test this??
  public void HandleVerticalMotion()
  {
    float increment = 0;
    float animationValue = 0;
    if (ShouldFall())
    {
      animationValue = .3f;
      increment = -1 / ascendDescendSpeed * Time.deltaTime * GridConstants.Z_SPACING;
    }
    if (UsingSkill() && activeSkill.SkillMovesCharacterVertically(this))
    {
      // animationValue = CalculateVerticalMovementAnimationSpeed(activeSkill.GetMovement(this, SkillEffectCurveProperty.MoveUp), activeSkill.IsContinuous(this));
      easedSkillUpwardMovementProgressIncrement = GridConstants.Z_SPACING * (CalculateCurveProgressIncrement(activeSkill.GetMovement(this, SkillEffectMovementProperty.MoveUp), false, activeSkill.IsContinuous(this)));
      increment = easedSkillUpwardMovementProgressIncrement;
      animationValue = Mathf.Lerp(.5f, 1.5f, increment / Time.deltaTime);
    }
    // if (increment != 0)
    // {
    // if (increment < 0) // going down
    // {
    //   if (transform.position.z % 1 == 0)
    //   {
    //     SetCurrentFloor(currentFloor - 1);
    //   }
    // }
    // else
    // { // going up
    //   if (GetZOffsetFromCurrentFloor(increment) > 1)
    //   {
    //     SetCurrentFloor(currentFloor + 1);
    //   }
    // }

    animator.SetFloat("VerticalMovementSpeed", animationValue);
    AdjustVerticalPosition(increment);
    // }

    // // if we're above our current floor by > 1, change our floor
    // if (transform.position.z - GridManager.GetZOffsetForGameObjectLayer(gameObject.layer) < -1)
    // {

    //   if (AllTilesOnTargetFloorEmpty(currentFloor + 1)) // ceiling check
    //   {
    //     SetCurrentFloor(currentFloor + 1);
    //   }
    //   else
    //   {
    //     EndSkill();
    //   }
    // }
    // else if (transform.position.z - GridManager.GetZOffsetForGameObjectLayer(gameObject.layer) > 0)
    // {
    //   if (AllTilesOnTargetFloorEmpty(currentFloor)) // floor check
    //   {
    //     SetCurrentFloor(currentFloor - 1);
    //     transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z));
    //     ascendingDescendingState = AscendingDescendingState.None;
    //   }
    //   else
    //   {
    //     EndSkill();
    //   }
    // }
    // if ((transform.position.z % 1) > .9 && !UsingVerticalMovementSkill())
    // {
    //   transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Round(transform.position.z));
    //   ascendingDescendingState = AscendingDescendingState.None;
    // }
    // if (ascending)
    // {
    //   transform.position -= new Vector3(0, 0, 1 / ascendDescendSpeed * Time.deltaTime);
    // }
    // else if (descending)
    // {
    //   transform.position += new Vector3(0, 0, 1 / ascendDescendSpeed * Time.deltaTime);
    // }
  }
  // Can move UDLR on current floor
  protected virtual bool CanMove()
  {
    if (ascending
      || stunned
      || molting
      || carapaceBroken
      || IsInKnockback()
      || animationPreventsMoving
    )
    {
      return false;
    }
    return true;
  }
  // can block, cast a spell, or attack
  protected virtual bool CanAct()
  {
    if (
      ascending
      || stunned
      || molting
      || carapaceBroken
      || UsingMovementSkill()
      || IsInKnockback()
      || UsingSkill()
      || animationPreventsMoving // I guess?
    )
    {
      return false;
    }
    return true;
  }

  protected bool ShouldUseSkillEffectSet(CharacterSkillData skillData, int idx = 0)
  {
    return
      !(skillData.SkipIfAirborne(this) && IsMidair()) && (
        skillData.skillEffectSets[idx].alwaysExecute
        || activeSkill == pressingSkill
        || activeSkill == queuedSkill);
  }
  public virtual bool CanUseSkill(CharacterSkillData skillData, int effectSetIndex = 0)
  {
    if (
      (IsMidair() && !skillData.skillEffectSets[effectSetIndex].canUseInMidair && !(activeSkill && activeSkill.Scrambling(this) && skillData.canUseWhileScrambling))
      || stunned
      || molting
      || carapaceBroken
      || IsInKnockback()
      || dashAttackQueued
      || (!HasStaminaForSkill(skillData) && !partStatusInfos[traitSlotsForSkills[skillData.id]].IsBroken())
      || animationPreventsMoving // I guess?
    )
    {
      return false;
    }
    return true;
  }
  protected virtual bool CanBlock()
  {

    if (
      ascending
      || descending
      || stunned
      || molting
      || carapaceBroken
      || UsingSkill()
      || animationPreventsMoving // I guess?
    )
    {
      return false;
    }
    return true;
  }

  // DAMAGE FUNCTIONS
  //how damage works:
  // each damage source deals both real and stamina damage
  // stamina damage converted to real damage if the character is out of stamina
  // real damage distributed across parts if the part is broken
  protected virtual void TakeDamage(IDamageSource damageSource)
  {
    if (damageSource.IsOwnedBy(this)) { return; }
    if (damageSource.IsSameOwnerType(this)) { return; }
    if (!damageSource.GetCharacterWithinVerticalRange(this)) { return; }

    // Crit damage:
    // -If you're using a crit, you don't take damage
    // -If you aren't critVictimOf the damageSource owner, don't take damage
    if (usingCrit) { return; }
    if (DashingPreventsDamage())
    {
      return;
    }
    if (
      sourceInvulnerabilities.Contains(damageSource.sourceString)
      && !damageSource.ignoresInvulnerability)
    { return; }
    float damageAfterResistances = damageSource.CalculateDamageAfterResistances(this);
    float staminaDamageAfterResistances = damageSource.CalculateStaminaDamageAfterResistances(this);
    if ( // return if we take neither stamina nor regular damage and we're supposed to take either
      (damageAfterResistances <= 0 && staminaDamageAfterResistances <= 0) &&
      (damageSource.damageAmount > 0 || damageSource.staminaDamageAmount > 0)
    ) { return; }
    if (damageSource.movementAbilitiesWhichBypassDamage.Intersect(activeMovementAbilities).Any())
    {
      return;
    }
    characterVisuals.DamageFlash(damageFlashColor);
    if (damageAfterResistances > 0 && activeSkill != null && activeSkill.SkillIsInterruptable(this))
    {
      EndSkill();
    }
    InterruptAnimation();
    Vector3 knockback = damageSource.GetKnockbackForCharacter(this);
    if (damageSource.damageType != DamageType.Physical)
    {
      AdjustElementalDamageBuildup(damageSource.damageType, damageAfterResistances);
    }
    else if (damageAfterResistances > 0)
    {
      Dictionary<TraitSlot, float> cachedDamage = new Dictionary<TraitSlot, float>();
      foreach (TraitSlot slot in partStatusInfos.Keys)
      {
        cachedDamage[slot] = partStatusInfos[slot].currentDamage;
      }
      AdjustBodyPartHealthAndStamina(Mathf.Floor(damageAfterResistances), Mathf.Floor(staminaDamageAfterResistances), knockback, lastActiveOnly: true, isNonlethal: damageSource.isNonlethal);
      DoDamageFx(damageAfterResistances, cachedDamage, knockback, damageSource.applySlowdown);
    }
    StartCoroutine(ApplyInvulnerability(damageSource));
    if (knockback != Vector3.zero)
    {
      BeginKnockback(knockback);
    }
  }

  void DoDamageFx(float damageAfterResistances, Dictionary<TraitSlot, float> cachedDamage, Vector3 knockback, bool weaponAttached)
  {
    float knockbackDistance = knockback.magnitude;
    GameObject splatter = Instantiate(
      bloodSplatterPrefab,
      transform.position + knockback * knockbackDistance * combatJuiceConstants.splatterSpawnDistanceMult,
      transform.rotation
    );
    splatter.transform.position = new Vector3(splatter.transform.position.x, splatter.transform.position.y, GetCurrentGroundPosition());
    Color randomTraitColor = traits[RandomTraitSlot()].primaryColor;
    splatter.GetComponentInChildren<SpriteRenderer>().color = randomTraitColor;
    splatter.transform.localScale = new Vector3(
      combatJuiceConstants.splatterBaseLength + (knockbackDistance * combatJuiceConstants.splatterLengthMult),
      combatJuiceConstants.splatterBaseWidth + (knockbackDistance * combatJuiceConstants.splatterWidthMult),
      0
    );
    splatter.transform.rotation = GetDirectionAngle(knockback);
    bloodSplashParticleSystem.gameObject.transform.rotation = GetDirectionAngle(knockback);
    int splashParticleCountMin = Mathf.CeilToInt(combatJuiceConstants.bloodSplashParticleCountMin + damageAfterResistances / combatJuiceConstants.extraBloodSplashParticlePerDamage);
    bloodSplashParticleSystem.SetInt("CountMin", splashParticleCountMin);
    bloodSplashParticleSystem.SetInt("CountMax", Mathf.CeilToInt(splashParticleCountMin * combatJuiceConstants.bloodSplashParticleCountMaxMult));
    Gradient gradient = combatJuiceConstants.bloodSplashColorOverLife;
    GradientColorKey gck = new GradientColorKey(randomTraitColor, 0f);
    gradient.SetKeys(new GradientColorKey[] { gck }, gradient.alphaKeys);
    bloodSplashParticleSystem.SetGradient("ColorOverLife", gradient);
    float velocityMin = combatJuiceConstants.bloodSplashParticleVelocityMin + combatJuiceConstants.bloodSplashParticleVelocityKnockbackMult * knockbackDistance;
    Debug.Log("velocityMin " + velocityMin);
    bloodSplashParticleSystem.SetFloat("VelocityMin", velocityMin);
    bloodSplashParticleSystem.SetFloat("VelocityMax", velocityMin * combatJuiceConstants.bloodSplashParticleVelocityMaxMult);
    bloodSplashParticleSystem.Play();
    float baseSlowdownDuration = knockbackDistance;
    if (damageAfterResistances >= GetCharacterVital(CharacterVital.CurrentHealth))
    {
      bloodSplashParticleSystem.transform.parent = null; // so that it plays when we die
      bloodSplashParticleSystem.gameObject.AddComponent<DestroyOnPlayerRespawn>(); // so it doesn't stick around forever
      baseSlowdownDuration = combatJuiceConstants.deathSlowdownBaseDuration;
    }
    else if (!weaponAttached)
    {
      baseSlowdownDuration = 0;
    }
    GameMaster.Instance.DoSlowdown(baseSlowdownDuration);
    // BreakBodyParts(damageAfterResistances, cachedDamage, knockback);
    PlayDamageSounds();
    DoCameraShake(damageAfterResistances, knockbackDistance);

  }
  void BreakBodyParts(float damageAfterResistances, Dictionary<TraitSlot, float> cachedDamage, Vector2 knockback)
  {
    // if (GetUnbrokenBodyParts().Count == 0)
    // {
    //   characterVisuals.BreakOffRemainingBodyParts(knockback);
    //   return;
    // }
    foreach (TraitSlot slot in cachedDamage.Keys)
    { // TODO: fix all this
      // if (GetBodyPartHealthForSlot(slot) <= 0 && cachedDamage[slot] > 0)
      // {
      //   characterVisuals.BreakRandomBodyPartFromSlot(knockback, slot);
      //   if (cachedDamage[slot] > 50)
      //   {
      //     characterVisuals.BreakRandomBodyPartFromSlot(knockback, slot);
      //   }
      // }
      // else if (GetBodyPartHealthForSlot(slot) <= 50 && cachedDamage[slot] > 50)
      // {
      //   characterVisuals.BreakRandomBodyPartFromSlot(knockback, slot);
      // }
    }
  }
  void BreakRandomBodyPart(Vector2 knockback)
  {
    List<TraitSlot> unbrokenParts = GetUnbrokenBodyParts();
    if (unbrokenParts.Count == 0) { return; }
    TraitSlot partToBreak = unbrokenParts[(int)(UnityEngine.Random.value * unbrokenParts.Count)];
    partStatusInfos[partToBreak].currentDamage = 100;
    characterVisuals.BreakRandomBodyPartFromSlot(knockback, partToBreak);
  }

  void BreakNextBodyPart(Vector2 knockback)
  {
    TraitSlot slotToBreak = GameMaster.Instance.settingsData.bodyPartBreakOrder[brokenParts.Count()];
    brokenParts[slotToBreak] = true;
    characterVisuals.BreakRandomBodyPartFromSlot(knockback, slotToBreak, 2);
    Debug.Log("broke part " + slotToBreak);
  }
  public virtual void PlayDamageSounds()
  {
  }

  public virtual void DoCameraShake(float damageAfterResistances, float knockbackAmount)
  {

  }
  public int GetDamageTypeResistanceLevel(DamageType type) // we could also just return the 
  {
    return GetAttribute((CharacterAttribute)ProtectionAttributeData.DamageTypeToProtectionAttribute[type]);
  }

  public float GetDamageMultiplier(DamageType type) // we could also just return the 
  {
    if (activeSkill != null)
    {
      return activeSkill.GetDamageMultiplierForType(this, type);
    }
    return 1.0f;
  }

  public virtual void Die(Vector2 knockback = default(Vector2))
  {
    Debug.Log("dying??");
    characterVisuals.BreakOffRemainingBodyParts(knockback);
    Destroy(gameObject);
  }

  IEnumerator ApplyInvulnerability(IDamageSource damageSource)
  {
    float invulnerabilityDuration = damageSource.invulnerabilityWindow;
    if (invulnerabilityDuration <= 0) { yield break; }
    if (damageSource as EnvironmentalDamage != null)
    {
      invulnerabilityDuration *= GetHazardImmunityDurationMultiplier();
    }
    string src = damageSource.sourceString;
    sourceInvulnerabilities.Add(src);
    yield return new WaitForSeconds(invulnerabilityDuration);
    if (sourceInvulnerabilities.Contains(src))
    {
      sourceInvulnerabilities.Remove(src);
    }
  }

  // Determines if we should be stunned, and stuns us if so.
  protected void CalculateAndApplyStun(float stunDuration, bool overrideStunResistance = false)
  {
    if (!overrideStunResistance)
    {
      // TODO: Stun resistance/reduction should happen here
    }
    if (stunDuration > 0)
    {
      StartCoroutine(ApplyStun(stunDuration));
    }
  }

  // Make us stunned, then un-make-us stunned. Cannot move or attack while stunned.

  IEnumerator ApplyStun(float stunDuration)
  {
    stunned = true;
    yield return new WaitForSeconds(stunDuration);
    stunned = false;
  }

  // same as stun, basicallyation/combo and reset us to idle.
  // Used to keep us from finishing our attack after getting knocked across the screen.
  // TODO: it should be possible to "tank" some attacks and finish attacking
  void InterruptAnimation()
  {
    if (animator != null)
    {
      // animator.SetTrigger("transitionToIdle");
    }
  }
  // END DAMAGE FUNCTIONS


  public virtual void AssignTraitsForNextLife(TraitSlotToUpcomingTraitDictionary nextLifeTraits)
  {

  }

  // ATTRIBUTE/VITALS ACCESSORS
  public int GetAttribute(CharacterAttribute attributeToGet)
  {
    return 0;
    // bool exists = attributes.TryGetValue(attributeToGet, out int val);
    // if (!exists)
    // {
    //   Debug.LogError("Tried to access non-existant attribute " + attributeToGet);
    // }
    // return val;
  }

  // STAT GETTERS
  public float GetCurrentMaxHealth()
  {
    return GetCharacterVital(CharacterVital.CurrentMaxHealth);
  }
  public float GetBodyPartCurrentDamage(TraitSlot bodyPart)
  {
    return partStatusInfos[bodyPart].currentDamage;
  }

  public bool IsBodyPartBroken(TraitSlot bodyPart)
  {
    return partStatusInfos[bodyPart].IsBroken();
  }
  public bool IsActiveSkillPartBroken()
  {
    return activeSkill && traitSlotsForSkills.ContainsKey(activeSkill.id) && partStatusInfos[traitSlotsForSkills[activeSkill.id]].IsBroken();
  }
  public float GetBodyPartCurrentStamina(TraitSlot bodyPart)
  {
    return partStatusInfos[bodyPart].currentExertion;
  }
  public float GetTrueMaxHealth()
  {
    return defaultCharacterData.defaultStats[CharacterStat.MaxHealth];
  }

  public float GetMaxStamina()
  {
    return defaultCharacterData.defaultStats[CharacterStat.Stamina];
  }

  public float GetMaxCarapace()
  {
    return defaultCharacterData.defaultStats[CharacterStat.Carapace];
  }

  public float GetCurrentMaxCarapace()
  {
    return GetMaxCarapace()
        - (GetMaxCarapaceLostPerMolt() * GetCharacterVital(CharacterVital.CurrentMoltCount));
  }

  public float GetStaminaRecoverySpeed()
  {
    return defaultCharacterData
      .GetMetabolismAttributeData()
      .GetAttributeTier(this)
      .staminaRecoverySpeed;
  }

  public DarkVisionInfo[] GetDarkVisionInfos()
  {
    return defaultCharacterData.GetDarkVisionAttributeData().GetDarkVisionInfos(this);
  }

  public virtual float GetAwarenessRange()
  {
    return defaultCharacterData.GetAntennaeSensitivityAttributeData().GetAwarenessRange(this);
  }


  public float GetStaminaRecoveryRate()
  {
    return (Time.deltaTime * GetMaxStamina() / GetStaminaRecoverySpeed());
  }
  public float GetMoveAcceleration()
  {
    return GetStat(CharacterStat.MoveAcceleration); // * GetMoveSpeedMultiplier();
  }

  public float GetRotationSpeed()
  {
    return GetStat(CharacterStat.RotationSpeed);
  }

  public float GetMaxHealthLostPerMolt()
  {
    return defaultCharacterData.defaultStats[CharacterStat.MaxHealthLostPerMolt];
  }

  public float GetMaxCarapaceLostPerMolt()
  {
    return defaultCharacterData
    .GetMoltingEfficiencyAttributeData()
    .GetMoltCarapaceCost(this);
  }

  public float GetMoveSpeedMultiplier()
  {
    return defaultCharacterData
    .GetReflexesAttributeData()
    .GetMoveSpeedMultiplier(this);
  }
  public bool GetCanFly()
  {
    return defaultCharacterData
      .GetFlightAttributeData()
      .GetAttributeTier(this)
      .canFly;
  }
  public bool GetCanFlyUp()
  {
    return defaultCharacterData
      .GetFlightAttributeData()
      .GetAttributeTier(this)
      .canFlyUp;
  }

  public float GetMaxFlightDuration()
  {
    return defaultCharacterData
      .GetFlightAttributeData()
      .GetAttributeTier(this)
      .flightDuration;
  }

  public float GetDashStaminaCost()
  {
    return defaultCharacterData
      .GetDashAttributeData()
      .GetDashStaminaCost(this);
  }
  public float GetHazardImmunityDurationMultiplier()
  {

    return defaultCharacterData
      .GetHazardResistanceAttributeData()
      .GetHazardImmunityDurationMultiplier(this);
  }

  public int GetSightRange()
  {
    return
      defaultCharacterData
        .GetSightRangeAttributeData()
        .GetAttributeTier(this)
        .sightRange;
  }

  public float GetCamouflageRange()
  {
    return
      defaultCharacterData
        .GetCamouflageAttributeData()
        .GetAttributeTier(this)
        .camouflageDistance;
  }
  public float GetStat(CharacterStat statToGet)
  {
    float multiplier = 1.0f;

    if (activeSkill != null)
    {
      switch (statToGet)
      {
        case CharacterStat.MoveAcceleration:
          multiplier = activeSkill.GetMultiplierSkillProperty(this, SkillEffectFloatProperty.MoveSpeed);
          break;
        case CharacterStat.RotationSpeed:
          multiplier = activeSkill.GetMultiplierSkillProperty(this, SkillEffectFloatProperty.RotationSpeed);
          break;
        default:
          break;
      }
    }
    return defaultCharacterData.defaultStats[statToGet] * multiplier;
  }

  public void AddMovementAbility(CharacterMovementAbility movementAbility)
  {
    activeMovementAbilities.Add(movementAbility);
  }

  public void RemoveMovementAbility(CharacterMovementAbility movementAbility)
  {
    activeMovementAbilities.Remove(movementAbility);
  }


  //VITALS GETTERS/SETTERS
  public void AdjustCurrentHealth_OLD(float adjustment, bool isNonlethal = false)
  {
    vitals[CharacterVital.CurrentHealth] =
      Mathf.Clamp(vitals[CharacterVital.CurrentHealth] + adjustment, isNonlethal ? 1 : 0, GetCurrentMaxHealth());
  }


  public void AdjustBodyPartHealthAndStamina(float adjustment, float staminaAdjustment, Vector2 knockback = default(Vector2), bool lastActiveOnly = true, bool isNonlethal = false, bool isNonbreaking = false)
  {
    List<TraitSlot> unbrokenBodyParts = GetUnbrokenBodyParts();
    int damageInt = Mathf.FloorToInt(adjustment);
    for (int i = 0; i < damageInt; i++)
    {
      BreakRandomBodyPart(knockback);
    }
    Debug.Log("Adjust body part health and stamina, unbroken count " + unbrokenBodyParts.Count);
    // if (lastActiveSkill == null || GetBodyPartHealthForSkill(lastActiveSkill) <= 0 || !lastActiveOnly) // last active part broken; all other parts split damage
    if (unbrokenBodyParts.Count > 0)
    {
      int damagePerPart = Mathf.CeilToInt(adjustment); //Mathf.CeilToInt(adjustment / unbrokenBodyParts.Count);
      int staminaDamagePerPart = Mathf.CeilToInt(staminaAdjustment); //Mathf.CeilToInt(staminaAdjustment / unbrokenBodyParts.Count);
      foreach (TraitSlot bodyPart in unbrokenBodyParts)
      {
        AdjustPartCurrentHealth(bodyPart, damagePerPart, isNonbreaking);
        AdjustPartCurrentStamina(bodyPart, staminaDamagePerPart, isBreaking: true);
      }
    }
    else if (!isNonlethal && adjustment > 0)
    {
      Die(knockback);
    }
  }

  // adjustment - endingHealth + startingHealth

  // starting health = 30
  // adjustment = -40
  // ending helath = 0
  // leftover adjustment = -10

  // starting health = 70
  // adjustment = -40
  // ending helath = 30
  // leftover adjustment = 0

  // starting health = 80
  // adjustment = 30
  // ending health = 100
  // leftover adjustment = 10
  public float AdjustPartCurrentHealth(TraitSlot bodyPart, float adjustment, bool isNonbreaking = false)
  {
    return partStatusInfos[bodyPart].AdjustCurrentDamage(adjustment);
  }

  // currentStamina: 10
  // adjustment: -40
  // part damage: -30

  //current stamina: -10
  // adjustment: -40
  // part damage: -40
  public void AdjustCurrentStaminaForSkill(string skillId, float adjustment)
  {
    return;
    if (!traitSlotsForSkills.ContainsKey(skillId)) { return; }
    TraitSlot slot = traitSlotsForSkills[skillId];
    AdjustPartCurrentStamina(slot, adjustment, isBreaking: false);
  }

  public void AdjustPartCurrentStamina(TraitSlot slot, float adjustment, bool isBreaking = false)
  {
    partStatusInfos[slot].AdjustCurrentExertion(adjustment, isBreaking);
  }

  public void AdjustCurrentMaxHealth(float adjustment)
  {
    vitals[CharacterVital.CurrentMaxHealth] =
     Mathf.Clamp(vitals[CharacterVital.CurrentMaxHealth] + adjustment, 0, GetTrueMaxHealth());
    AdjustBodyPartHealthAndStamina(0, 0, Vector2.zero);
  }

  public void AdjustCurrentCarapace(float adjustment)
  {
    vitals[CharacterVital.CurrentCarapace] =
      Mathf.Clamp(vitals[CharacterVital.CurrentCarapace] + adjustment, 0, GetCurrentMaxCarapace());
  }

  public void AdjustCurrentMoltCount(float adjustment)
  {
    vitals[CharacterVital.CurrentMoltCount] += adjustment;
  }

  public void AdjustElementalDamageBuildup(DamageType type, float amount)
  {
    if (!elementalDamageBuildups.TryGetValue(type, out ElementalDamageBuildup buildup))
    {
      buildup = new ElementalDamageBuildup();
      buildup.remainingMagnitude = 0;
      buildup.timeElapsed = 0;
      elementalDamageBuildups[type] = buildup;
    }
    buildup.remainingMagnitude += amount;
    buildup.remainingMagnitude = Mathf.Clamp(buildup.remainingMagnitude, 0, 100);
  }

  public float GetCharacterVital(CharacterVital vital)
  {
    return vitals[vital];
  }

  public List<TraitSlot> GetUnbrokenBodyParts()
  {
    List<TraitSlot> ret = new List<TraitSlot>();
    foreach (TraitSlot slot in partStatusInfos.Keys)
    {
      if (!partStatusInfos[slot].IsBroken() && slot != TraitSlot.Thorax)
      {
        ret.Add(slot);
      }
    }
    return ret;
  }

  public List<PartStatusInfo> GetUnbrokenBodyPartStatuses()
  {
    List<PartStatusInfo> ret = new List<PartStatusInfo>();
    foreach (TraitSlot slot in partStatusInfos.Keys)
    {
      if (!partStatusInfos[slot].IsBroken())
      {
        ret.Add(partStatusInfos[slot]);
      }
    }
    return ret;
  }

  public float GetStaminaForSkill(CharacterSkillData skill)
  {
    return partStatusInfos[traitSlotsForSkills[skill.id]].currentExertion;
  }

  // public float GetBodyPartHealthForSkill(CharacterSkillData skill)
  // {
  //   return GetBodyPartHealthForSlot(traitSlotsForSkills[skill.id]);
  // }

  // public float GetBodyPartHealthForSlot(TraitSlot slot)
  // {
  //   return partStatusInfos[slot].currentHealth;
  // }
  public virtual bool HasStaminaForSkill(CharacterSkillData skill)
  {
    if (!traitSlotsForSkills.ContainsKey(skill.id))
    { // probably hop or another stamina-less skill
      return true;
    }
    return partStatusInfos[traitSlotsForSkills[skill.id]].HasStaminaRemaining();
  }

  public bool SkillPartIsBroken(CharacterSkillData skill)
  {
    return brokenParts.ContainsKey(traitSlotsForSkills[skill.id]);
  }

  public virtual void SetCurrentFloor(FloorLayer newFloorLayer)
  {
    if (!AllTilesOnTargetFloorClearOfObjects(newFloorLayer) && currentTile != null)
    {
      CenterCharacterOnCurrentTile();
    }
    currentFloor = newFloorLayer;
    currentTileLocation = CalculateCurrentTileLocation();
    ChangeLayersRecursively(transform, newFloorLayer);
    HandleTileCollision(GridManager.Instance.GetTileAtLocation(currentTileLocation));
    po.OnLayerChange();
  }

  public void EnableAttackReadyIndicator()
  {
    attackReadyIndicatorObject.SetActive(true);
  }

  public void UseTile()
  {
    EnvironmentTileInfo et = GridManager.Instance.GetTileAtLocation(currentTileLocation);
    if (et.ChangesFloorLayer())
    {
      StartAscentOrDescent(et.AscendsOrDescends());
    }
  }

  //TODO: delete, replace use with WorldObjectGetCurrentTileLocation()
  public TileLocation CalculateCurrentTileLocation()
  {
    return new TileLocation(
      transform.position,
      currentFloor
    );
  }

  protected virtual void HandleHealth()
  {
    if (vitals[CharacterVital.CurrentHealth] <= 0)
    {
      Die();
    }
    foreach (SpriteRenderer renderer in renderers)
    {

      renderer.color = Color.Lerp(
        damagedColor,
        Color.white,
        vitals[CharacterVital.CurrentHealth] / GetCurrentMaxHealth()
      );
    }
  }

  // Returns tiles we are "in contact with" but not necessarily overlapping
  protected HashSet<EnvironmentTileInfo> GetTouchingTiles(FloorLayer layerToConsider)
  {
    HashSet<EnvironmentTileInfo> touchingTiles = new HashSet<EnvironmentTileInfo>();
    foreach (Vector2 point in touchingCollider.points)
    {
      touchingTiles.Add(GridManager.Instance.GetTileAtWorldPosition(transform.TransformPoint(point.x, point.y, transform.position.z), layerToConsider));
    }
    return touchingTiles;
  }

  protected HashSet<WallObject> GetTouchingWallObjects(FloorLayer layerToConsider)
  {
    HashSet<WallObject> touchingWallObjects = new HashSet<WallObject>();
    foreach (Vector2 point in touchingCollider.points)
    {
      EnvironmentTileInfo eti = GridManager.Instance.GetTileAtWorldPosition(transform.TransformPoint(point.x, point.y, transform.position.z), layerToConsider);
      if (GridManager.Instance.ShouldHaveCollisionWith(eti, transform))
      {
        touchingWallObjects.Add(GridManager.Instance.GetWallObjectAtLocation(eti.tileLocation));
      }
    }
    return touchingWallObjects;
  }
  // Returns tiles we are, or would be, overlapping
  protected HashSet<EnvironmentTileInfo> GetOverlappingTiles(FloorLayer layerToConsider)
  {
    HashSet<EnvironmentTileInfo> overlappingTiles = new HashSet<EnvironmentTileInfo>();
    foreach (Vector2 point in physicsCollider.points)
    {
      overlappingTiles.Add(GridManager.Instance.GetTileAtWorldPosition(transform.TransformPoint(point.x, point.y, transform.position.z), layerToConsider));
    }
    return overlappingTiles;
  }

  protected HashSet<WallObject> GetOverlappingWallObjects(FloorLayer layerToConsider)
  {
    HashSet<WallObject> overlappingWallObjects = new HashSet<WallObject>();
    foreach (Vector2 point in physicsCollider.points)
    {
      overlappingWallObjects.Add(GridManager.Instance.GetWallObjectAtLocation(new TileLocation(transform.TransformPoint(point.x, point.y, transform.position.z), layerToConsider)));
    }
    return overlappingWallObjects;
  }


  public bool TouchingTileWithTag(TileTag tag)
  {
    HashSet<EnvironmentTileInfo> touchingTiles = GetTouchingTiles(currentFloor);
    foreach (EnvironmentTileInfo tile in touchingTiles)
    {
      if (tile.HasTileTag(tag))
      {
        return true;
      }
    }
    return false;
  }

  public bool TouchingWall()
  {
    return GetTouchingWallObjects(currentFloor).Count > 0;
  }
  protected virtual bool AllTilesOnTargetFloorEmpty(FloorLayer targetFloor)
  {
    // if we're flying, every tile above us needs to be empty.

    HashSet<EnvironmentTileInfo> overlappingTiles = GetOverlappingTiles(targetFloor);
    foreach (EnvironmentTileInfo tile in overlappingTiles)
    {

      if (!tile.IsEmpty())
      {
        return false;
      }
    }
    return true;
  }

  protected virtual bool AllTilesOnTargetFloorClearOfObjects(FloorLayer targetFloor)
  {

    HashSet<EnvironmentTileInfo> overlappingTiles = GetOverlappingTiles(targetFloor);
    foreach (EnvironmentTileInfo tile in overlappingTiles)
    {
      if (tile.HasSolidObject()) { return false; }
    }
    return true;
  }

  protected virtual bool ShouldFall() // we have removed sticking!! make it stick yourself!!!
  {
    if (
        activeMovementAbilities.Contains(CharacterMovementAbility.Hover)
          || UsingVerticalMovementSkill()
          || sticking
          || ascending
          || descending
        )
    {
      return false; // abilities prevent falling!
    }
    if (IsMidair())
    {
      return true;
    }

    return CanPassThroughFloorLayer(currentFloor);
  }

  float GetMaxFloorHeight()
  {
    HashSet<EnvironmentTileInfo> overlappingTiles = GetOverlappingTiles(currentFloor);
    float maxFloorHeight = 0;
    foreach (EnvironmentTileInfo tile in overlappingTiles)
    {
      maxFloorHeight = Mathf.Max(maxFloorHeight, tile.GroundHeight());
    }
    return maxFloorHeight;
  }

  bool CanPassThroughFloorLayer(FloorLayer targetFloor)
  {
    HashSet<EnvironmentTileInfo> overlappingTiles = GetOverlappingTiles(targetFloor);
    foreach (EnvironmentTileInfo tile in overlappingTiles)
    {
      if (tile.objectTileType != null || tile.groundTileType != null)
      {
        return false; // At least one corner is on a tile
      }
    }
    return true;
  }

  protected virtual void HandleTile()
  {
    TileLocation currentLoc = CalculateCurrentTileLocation();

    EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(currentLoc);
    if (tile == null)
    {
      // Debug.LogError("WARNING: no tile found at " + CalculateCurrentTileLocation().ToString());
      Die();
      return;
    }
    if (tile != currentTile)
    {
      currentTile = tile;
    }
    TileLocation nowTileLocation = CalculateCurrentTileLocation();
    if (currentTileLocation != nowTileLocation)
    {
      currentTileLocation = nowTileLocation;
    }
    GridManager.Instance.UpdateWallObjectCollisionsForCharacter(this);
    if (ascending || descending) { return; }

    if (tile.dealsDamage/* && vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] <= 0*/)
    {
      foreach (EnvironmentalDamage envDamage in tile.environmentalDamageSources)
      {
        if (envDamage.IsEnvironmentalDamageSourceActive())
        {
          TakeDamage(envDamage);
        }
      }
    }
    if (footstepCooldown <= 0 && timeMoving > 0 && !IsMidair())
    {
      footstepCooldown = tile.HandleFootstep(this);
    }
    if (tile.HasTileTag(TileTag.Water))
    {
      AdjustElementalDamageBuildup(DamageType.Heat, -10 * Time.deltaTime);
    }
    if (tile.CanRespawnPlayer())
    {
      if (!tile.CharacterCanCrossTile(this))
      {
        if (tile.groundTileType.tileTags.Contains(TileTag.Water))
        {
          AdjustElementalDamageBuildup(DamageType.Heat, -100);
        }
        RespawnCharacterAtLastSafeLocation();
      }
    }
    else if (tile.groundTileType != null && ascendingDescendingState != AscendingDescendingState.Ascending)
    {
      lastSafeTileLocation = currentTileLocation;
    }
  }

  public void OnWeaponHit(SkillEffect skillEffect, Collider2D collider)
  {
  }
  protected void RespawnCharacterAtLastSafeLocation()
  {
    EndSkill();
    transform.position =
      new Vector3(lastSafeTileLocation.cellCenterWorldPosition.x, lastSafeTileLocation.cellCenterWorldPosition.y, GridManager.GetZOffsetForGameObjectLayer(GetGameObjectLayerFromFloorLayer(lastSafeTileLocation.floorLayer)) - GridManager.Instance.GetTileAtLocation(lastSafeTileLocation).GroundHeight());
    if (currentFloor != lastSafeTileLocation.floorLayer)
    {
      SetCurrentFloor(lastSafeTileLocation.floorLayer);
    }
    CalculateAndApplyStun(.5f, true);
    po.HardSetVelocityToZero();
  }

  public virtual void HandleContactWithRespawnTile()
  {

  }

  public virtual void HandleTileCollision(EnvironmentTileInfo tile)
  {
    if (tile.GetColliderType() == Tile.ColliderType.None)
    {
      return;
    }
    else
    {
      if (tile.CharacterCanBurrowThroughObjectTile(this))
      {
        GridManager.Instance.MarkTileToRestoreOnPlayerRespawn(tile);
        tile.DestroyObjectTile();
      }
    }
  }

  public virtual void HandleConditionallyActivatedTraits()
  {
    foreach (TraitEffect trait in conditionallyActivatedTraitEffects)
    {
      switch (trait.activatingCondition)
      {
        case ConditionallyActivatedTraitCondition.NotMoving:
          if (timeStandingStill > trait.activatingConditionRequiredDuration)
          {
            trait.Apply(this);
          }
          else
          {
            trait.Expire(this);
          }
          break;
        case ConditionallyActivatedTraitCondition.Moving:
          if (timeMoving > trait.activatingConditionRequiredDuration)
          {
            trait.Apply(this);
          }
          else
          {
            trait.Expire(this);
          }
          break;
      }
    }
  }

  protected void HandleSkills()
  {
    if (UsingSkill())
    {
      activeSkill.UseSkill(this);
      timeSpentInSkillEffect += Time.deltaTime;
    }
    else
    {
      timeSpentInSkillEffect = 0f;
    }
  }

  public ElementalDamageBuildup GetElementalDamageBuildup(DamageType damageType)
  {
    elementalDamageBuildups.TryGetValue(damageType, out ElementalDamageBuildup buildup);
    return buildup;
  }
  protected void HandleCooldowns()
  {
    foreach (TraitSlot slot in characterSkills.Keys)
    {
      // if (characterSkills[slot] != activeSkill)
      // {
      AdjustPartCurrentStamina(slot, -100 / characterSkills[slot].staminaRecoveryTime * Time.deltaTime, isBreaking: false);
      // }
    }

    if (footstepCooldown > 0)
    {
      footstepCooldown -= Time.deltaTime;
    }
    if (reverseDirectionRotationCooldown > 0)
    {
      reverseDirectionRotationCooldown -= Time.deltaTime;
    }
    foreach (DamageType type in (DamageType[])Enum.GetValues(typeof(DamageType)))
    {
      elementalDamageBuildups.TryGetValue(type, out ElementalDamageBuildup buildup);
      if (buildup != null)
      {
        buildup.timeElapsed += Time.deltaTime;
        ElementalBuildupConstant elementConstant = GameMaster.Instance.elementalBuildupConstants[type];
        if (type == DamageType.Heat)
        {
          AdjustElementalDamageBuildup(DamageType.Heat, -distanceFromPreviousPosition * .5f);
        }
        if (buildup.timeElapsed >= elementConstant.delay)
        {
          float damage = Time.deltaTime * elementConstant.dps;
          float staminaDamage = Time.deltaTime * elementConstant.staminaDps;
          if (damage > buildup.remainingMagnitude)
          {
            damage = Mathf.Max(buildup.remainingMagnitude, 0);
            elementalDamageBuildups.Remove(type);
          }
          else
          {
            buildup.remainingMagnitude -= damage;
          }
          if (type == DamageType.Acid)
          {
            if (!IsMidair())
            {
              AdjustCurrentMaxHealth(-damage);
            }
          }
          else
          {
            AdjustBodyPartHealthAndStamina(damage, staminaDamage, Vector2.zero, false);
          }
        }
      }
    }
  }

  public void QueueDashAttack()
  {
    dashAttackQueued = true;
  }
  public void HandleSkillMovement()
  {
    //Consumed by physics so needs to happen in fixedupdate. Might still suck?
    if (UsingSkill() && activeSkill.SkillMovesCharacterForward(this))
    {
      easedSkillForwardMovementProgressIncrement = (CalculateCurveProgressIncrement(activeSkill.GetMovement(this, SkillEffectMovementProperty.MoveForward), true, activeSkill.IsContinuous(this)));
    }
  }

  public void HandleKnockbackCooldown()
  {
    //Consumed by physics so needs to happen in fixedupdate. Might still suck?
    if (knockbackAmount > 0)
    {
      float oldEasedKnockbackProgress = GetEasedKnockbackProgress();
      knockbackProgress += Time.deltaTime;
      float newEasedKnockbackProgress = GetEasedKnockbackProgress();
      if (knockbackProgress >= GetKnockbackDuration())
      {
        knockbackProgress = GetKnockbackDuration();
        newEasedKnockbackProgress = GetEasedKnockbackProgress();
        EndKnockback();
      }
      easedKnockbackProgressIncrement = (newEasedKnockbackProgress - oldEasedKnockbackProgress) * knockbackAmount; // kill me a little
    }
  }

  float NORMALIZED_HOP_HEIGHT = .25f;
  // accepts ownPosition so pathfinding can consider places we aren't currently
  public bool CanHopUpAtLocation(float ownZPosition, Vector3 wallPosition)
  {
    Vector3 hopCheckLocation = new Vector3(wallPosition.x, wallPosition.y, ownZPosition - NORMALIZED_HOP_HEIGHT * GridConstants.Z_SPACING); // the spot whose wallObject we want to compare // TODO: CLEAR MAGIC NUMBER
    EnvironmentTileInfo eti = GridManager.Instance.GetTileAtLocation(new TileLocation(hopCheckLocation));
    WallObject wallObject = GridManager.Instance.GetWallObjectAtLocation(new TileLocation(hopCheckLocation));
    float groundHeightOffset = eti.GroundHeight();
    return wallObject == null || !GridManager.Instance.ShouldHaveCollisionWith(eti, ownZPosition - NORMALIZED_HOP_HEIGHT * GridConstants.Z_SPACING); // TODO: CLEAR MAGIC NUMBER
  }

  public void OnWallObjectCollisionStay(Vector3 wallPosition, Vector2 normal)
  {
    float angle = Vector2.Angle(normal, movementInput);
    if ((!UsingSkill() || activeSkill.SkillIsCancelable(this)) && movementInput != Vector2.zero && angle < SCRAMBLE_MAX_ANGLE && CanUseSkill(hopSkill) /*&& CanHopUpAtLocation(transform.position.z, wallPosition)*/)
    {
      if (UsingSkill()) { InterruptSkill(hopSkill); return; }
      BeginSkill(hopSkill);
    }
  }

  public List<CharacterSkillData> GetSkillsThatCanCrossEmptyTiles()
  {
    List<CharacterSkillData> skills = new List<CharacterSkillData>();
    foreach (CharacterSkillData skill in characterSkills.Values)
    {
      if (skill.CanCrossEmptyTiles())
      {
        skills.Add(skill);
      }
    }
    return skills;
  }

  public List<CharacterSkillData> GetSkillsThatCanCrossTileWithoutRespawning(EnvironmentTileInfo tileInfo)
  {
    List<CharacterSkillData> skills = new List<CharacterSkillData>();
    foreach (CharacterSkillData skill in characterSkills.Values)
    {
      if (characterSkills[TraitSlot.Thorax] == skill) { continue; }
      foreach (CharacterMovementAbility movementAbility in tileInfo.GetMovementAbilitiesWhichBypassRespawn())
        if (skill.SkillProvidesMovementAbility(movementAbility))
        {
          skills.Add(skill);
        }
    }
    return skills;
  }

  public static TraitSlot RandomTraitSlot()
  {
    TraitSlot[] values = (TraitSlot[])Enum.GetValues(typeof(TraitSlot));
    return values[UnityEngine.Random.Range(0, values.Length)];
  }

}
