using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Tilemaps;
// TODO: Character can probably extend CustomPhysicsController, which simplifies movement code a bit.
public class AnimationInfoObject
{
  public Vector2 animationInput;
}

public enum CharacterType
{
  Player,
  Enemy
}
// non-derivable values like health, current number of molts, etc
public enum CharacterVital
{
  CurrentHealth,
  CurrentEnvironmentalDamageCooldown,
  CurrentStamina,
  CurrentCarapace, // Carapace might be "balance" sometime
  CurrentMaxHealth,
  CurrentMoltCount
}

// values driving physical character behavior
// base values live in character data
// can include maximums, cooldown periods, etc
// not usually user facing, so can be pretty grody and granular if need be
public enum CharacterStat
{
  REMOVE_0 = 0,
  MaxHealthLostPerMolt = 1,
  DetectableRange = 2,
  MoveAcceleration = 3,
  FlightAcceleration = 4,
  Stamina = 5,
  DashDistance = 6,
  DashDuration = 7,
  DashRecoveryDuration = 8,
  RotationSpeed = 9,
  Carapace = 10,
  MoltDuration = 11,
  BlockingMoveAcceleration = 12,
  BlockingRotationSpeed = 13,
  MaxHealth = 14
}

public enum CharacterAttribute
{
  REMOVE_0 = 0,
  REMOVE_1 = 1,
  REMOVE_2 = 2,
  Burrow = 3,
  Camouflage = 4,
  HazardResistance = 5,
  Resist_Fungal = 6,
  Resist_Heat = 7,
  Resist_Acid = 8,
  Resist_Physical = 9,
  WaterResistance = 10,
  Flight = 11,
  Dash = 12,
  Health = 13,
  Metabolism = 14,
  SightRange = 15,
  DarkVision = 16,
  MoltingEfficiency = 17,
  Reflexes = 18,
  AntennaeSensitivity = 19
}

public enum BugSpecies
{
  Ant = 0,
  AssassinBug = 1,
  Bee = 2,
  Blowfly = 3,
  BombardierBeetle = 4,
  Booklouse = 5,
  Butterfly = 6,
  Cicada = 7,
  Cockroach = 8,
  CuckooWasp = 9,
  Dragonfly = 10,
  Firefly = 11,
  GiantHornet = 12,
  GoliathBeetle = 13,
  Ladybug = 14,
  Mantis = 15,
  Mayfly = 16,
  MoleCricket = 17,
  Mosquito = 18,
  Moth = 19,
  Scarab = 20,
  Shieldbug = 21,
  StickInsect = 22,
  Strepsiptera = 23,
  Termite = 24,
  Wasp = 25,
  WaterBoatman = 26,
  WaterBug = 27,
  WaterStrider = 28,
  Default = 1000,

}
public enum CharacterAttackValue
{
  Damage,
  Range,
  HitboxSize,
  Knockback,
  Stun,
  AttackSpeed,
  Cooldown,
  DurabilityDamage,
  Venom,
  AcidDamage,
}

// Special modes of character movement.
// Possibly unnecessary!!
public enum CharacterMovementAbility
{
  Burrow,
  FastFeet,
  Halteres,
  Hover,
  StickyFeet,
  WaterStride
}

public enum AscendingDescendingState
{ // negative is up and positive is down because the map is backwards, Do Not @ Me
  Ascending = -1,
  Descending = 1,
  None = 0
}

public enum CharacterPerceptionAbility
{
  SensitiveAntennae
}

[System.Serializable]
public class CharacterAttributeModification
{
  public CharacterAttribute statToModify;

  public int magnitude;

  public float applicationDuration;
  public float duration;
  public float delay;

  public string source;

  public CharacterAttributeModification(CharacterAttribute s, int m, float dur, float del, string src)
  {
    statToModify = s;
    magnitude = m;
    duration = dur;
    delay = del;
    source = src;
  }
}

[System.Serializable]
public class ActiveStatModification
{
  public int magnitude;
  public string source;

  public ActiveStatModification(int m, string s)
  {
    magnitude = m;
    source = s;
  }
}

[System.Serializable]
public class CharacterAttackModifiers
{
  public CharacterAttackValueToIntDictionary attackValueModifiers;
  public bool forcesLymphDrop;

  public CharacterAttackModifiers()
  {
    attackValueModifiers = new CharacterAttackValueToIntDictionary();
  }

}

[System.Serializable]
public class TraitsLoadout
{
  public Trait head;
  public Trait thorax;
  public Trait abdomen;
  public Trait legs;
  public Trait wings;

  public bool AllTraitsEmpty()
  {
    return head == null
        && thorax == null
        && abdomen == null
        && legs == null
        && wings == null;
  }
}

// [System.Serializable]
// public class Moveset
// {
//   public AttackTypeToCharacterSkillDataDictionary attacks;
//   public TraitSlotToCharacterSkillDataDictionary skills;

//   public Moveset()
//   {
//     attacks = new AttackTypeToCharacterSkillDataDictionary();
//     skills = new TraitSlotToCharacterSkillDataDictionary();
//   }

//   public Moveset(TraitSlotToTraitDictionary traits)
//   {
//     attacks = new AttackTypeToCharacterSkillDataDictionary();
//     skills = new TraitSlotToCharacterSkillDataDictionary();
//     foreach (TraitSlot slot in traits.Keys)
//     {
//       skills[slot] = traits[slot].skill;
//       // attacks[Character.GetAttackTypeForTraitSlot(slot)] = traits[slot].weaponData.attacks[Character.GetAttackTypeForTraitSlot(slot)];
//     }
//   }
// }

// TODO: Character should probably extend CustomPhysicsController, which should extend WorldObject
public class Character : WorldObject
{
  [Header("Stats and Vitals")]
  public CharacterType characterType;
  public CharacterVitalToFloatDictionary vitals;
  public StatToActiveStatModificationsDictionary statModifications;
  public DamageTypeToFloatDictionary damageTypeResistances;

  [Header("Attack Info")]
  public TraitSlotToCharacterSkillDataDictionary characterSkills;
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
  public CharacterSkillData queuedSkill;
  public CharacterSkillData pressingSkill;
  public float timeSpentInSkillEffect = 0f;
  public int currentSkillEffectSetIndex = 0;
  public int currentSkillEffectIndex = 0;
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
  protected Vector2 movementInput;
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

  public bool dashAttackQueued = false;
  public int chargeLevel = 0;

  [Header("Default Info")]
  public CharacterData defaultCharacterData;

  private Coroutine damageFlashCoroutine;

  public Color damagedColor;

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
    sourceInvulnerabilities = new List<string>();
    conditionallyActivatedTraitEffects = new List<TraitEffect>();
    elementalDamageBuildups = new Dictionary<DamageType, ElementalDamageBuildup>();
    ascendingDescendingState = AscendingDescendingState.None;
    traitSpawnedGameObjects = new Dictionary<string, GameObject>();
    characterSkills = CalculateSkills(traits);
    // attributes = CalculateAttributes(traits);
    AwarenessTrigger awareness = GetComponentInChildren<AwarenessTrigger>();

    float awarenessRange = GetAwarenessRange();
    if (awareness != null && awarenessRange > 0)
    {
      awareness.Init(this);
      awareness.transform.localScale = new Vector3(awarenessRange, 1, 1);
    }
    InitializeFromCharacterData();
    InitializeAnimationParameters();
  }
  public void InitializeVisuals()
  {
    characterVisuals.SetCharacterVisuals(traits);
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

  public virtual CharacterSkillData GetSelectedCharacterSkill()
  {
    if (characterSkills.Count > 0)
    {
      return characterSkills[0];
    }
    return null;
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

  void InitializeAnimationParameters()
  {
    animator.SetFloat("HeadAnimationType", (int)traits[TraitSlot.Head].bugSpecies);
    animator.SetFloat("ThoraxAnimationType", (int)traits[TraitSlot.Thorax].bugSpecies);
    animator.SetFloat("AbdomenAnimationType", (int)traits[TraitSlot.Abdomen].bugSpecies);
    animator.SetFloat("LegsAnimationType", (int)traits[TraitSlot.Legs].bugSpecies);
    animator.SetFloat("WingsAnimationType", (int)traits[TraitSlot.Wings].bugSpecies);
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
    currentSkillEffectIndex++;
    while (currentSkillEffectIndex <= activeSkill.GetActiveSkillEffectSet(this).skillEffects.Length - 1)
    {
      if (!activeSkill.GetShouldExecute(this))
      {
        currentSkillEffectIndex++;
        continue;
      }
      if (queuedSkill == activeSkill && activeSkill.CanAdvanceSkillEffectSet(this))
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

  public void EndSkill()
  {
    if (UsingSkill())
    {
      activeSkill.EndSkillEffect(this);
      activeSkill.CleanUp(this);
      activeSkill = null;
    }
    pressingSkill = null;
    currentSkillEffectSetIndex = 0;
    currentSkillEffectIndex = 0;
    timeSpentInSkillEffect = 0;
    chargeLevel = 0;
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
    skill.BeginSkillEffect(this);
  }

  public void InterruptSkill(CharacterSkillData skill)
  {
    activeSkill.CleanUp(this);
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

  public static float GetAttackValueModifier(CharacterAttackValueToIntDictionary attackModifiers, CharacterAttackValue value)
  {
    if (attackModifiers == null || !attackModifiers.ContainsKey(value))
    {
      return 0;
    }
    return attackModifiers[value] * DrossConstants.CharacterAttackAdjustmentIncrements[value];
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
      (animationPreventsMoving || stunned || carapaceBroken)
      && !activeMovementAbilities.Contains(CharacterMovementAbility.Halteres)
    )
    {
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

    if (GetZOffsetFromCurrentFloorLayer(increment) < 0)
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
    else if (GetZOffsetFromCurrentFloorLayer(increment) > 1)
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
    if (activeSkill && activeSkill.IsWhileAirborne(this))
    {
      AdvanceSkillEffect();
    }
  }
  // again this returns numbers you'd expect - positive if above, negative if below.
  float GetZOffsetFromCurrentFloorLayer(float withIncrement = 0)
  {
    return GridManager.GetZOffsetForGameObjectLayer(gameObject.layer) - (transform.position.z - withIncrement);
  }

  float GetMinDistanceFromOverlappingFloorTiles(float withIncrement = 0)
  {
    HashSet<EnvironmentTileInfo> overlappingTiles = GetOverlappingTiles(currentFloor);
    float minDistance = 1;
    foreach (EnvironmentTileInfo tile in overlappingTiles)
    {
      minDistance = Mathf.Min(GetDistanceFromFloorTile(tile.tileLocation, withIncrement), minDistance);
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

  float GetDistanceFromFloorTile(TileLocation loc, float withIncrement = 0)
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
      increment = -1 / ascendDescendSpeed * Time.deltaTime;
    }
    if (UsingSkill() && activeSkill.SkillMovesCharacterVertically(this))
    {
      // animationValue = CalculateVerticalMovementAnimationSpeed(activeSkill.GetMovement(this, SkillEffectCurveProperty.MoveUp), activeSkill.IsContinuous(this));
      easedSkillUpwardMovementProgressIncrement = (CalculateCurveProgressIncrement(activeSkill.GetMovement(this, SkillEffectMovementProperty.MoveUp), false, activeSkill.IsContinuous(this)));
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
      || !HasStamina()
      || animationPreventsMoving
    )
    {
      return false;
    }
    return true;
  }

  // can begin a dash
  protected virtual bool CanDash()
  {
    if (
      ascending
      || stunned
      || molting
      || carapaceBroken
      || UsingMovementSkill()
      || IsInKnockback()
      || animationPreventsMoving // I guess?
      || !HasStamina()
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
      (skillData.skillEffectSets[idx].alwaysExecute || activeSkill == pressingSkill || activeSkill == queuedSkill);
  }
  protected virtual bool CanUseSkill(CharacterSkillData skillData, int effectSetIndex = 0)
  {
    if (
      (IsMidair() && !skillData.skillEffectSets[effectSetIndex].canUseInMidair)
      || stunned
      || molting
      || carapaceBroken
      || IsInKnockback()
      || dashAttackQueued
      || !HasStamina()
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

  bool WithinDamageHeight(IDamageSource damageSource)
  {
    if (damageSource as EnvironmentalDamage != null)
    {
      return true;
    }
    // !!!FIXME FIXME FIXME!!!!
    return !IsMidair();
    // !!!FIXME FIXME FIXME!!!!

  }
  // DAMAGE FUNCTIONS
  protected virtual void TakeDamage(IDamageSource damageSource)
  {
    if (damageSource.IsOwnedBy(this)) { return; }
    if (damageSource.IsSameOwnerType(this)) { return; }
    if (!WithinDamageHeight(damageSource)) { return; }

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
    if (
      damageAfterResistances <= 0
      && damageSource.damageAmount > 0
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

      float knockbackDistance = knockback.magnitude;
      GameObject splatter = Instantiate(bloodSplatterPrefab, transform.position + knockback * knockbackDistance * .1f, transform.rotation);
      splatter.transform.localScale = new Vector3(.25f + knockbackDistance, .25f + knockbackDistance * .5f, 0);
      splatter.transform.rotation = GetDirectionAngle(knockback);
      bloodSplashParticleSystem.gameObject.transform.rotation = GetDirectionAngle(knockback);
      bloodSplashParticleSystem.Play();
      // float damageForBreak = damageAfterResistances;
      // while  (damageForBreak > 0) {
      //   characterVisuals.BreakOffRandomBodyPart();
      // }
      // if (damageAfterResistances % twentyPercentOfMaxHealth)
      // if (GetCharacterVital(CharacterVital.CurrentHealth) %  )
      BreakBodyParts(damageAfterResistances, knockback);
      PlayDamageSounds();
      AdjustCurrentHealth(Mathf.Floor(-damageAfterResistances), damageSource.isNonlethal);
    }
    StartCoroutine(ApplyInvulnerability(damageSource));
    if (knockback != Vector3.zero)
    {
      BeginKnockback(knockback);
    }
  }

  void BreakBodyParts(float damageAfterResistances, Vector2 knockback)
  {
    int twentyPercentOfMaxHealth = Mathf.FloorToInt(GetCurrentMaxHealth() / 5f);
    Debug.Log("current health: " + GetCharacterVital(CharacterVital.CurrentHealth));
    Debug.Log("damageAfterResistances: " + damageAfterResistances);
    Debug.Log("twentyPercent: " + twentyPercentOfMaxHealth);
    Debug.Log("num parts to break: " + Mathf.FloorToInt(damageAfterResistances / twentyPercentOfMaxHealth));
    Debug.Log("damage above threshhold: " + damageAfterResistances % twentyPercentOfMaxHealth + ", current health above threshold: " + GetCharacterVital(CharacterVital.CurrentHealth) % twentyPercentOfMaxHealth);
    for (int i = 0; i < Mathf.FloorToInt(damageAfterResistances / twentyPercentOfMaxHealth); i++)
    {
      characterVisuals.BreakOffRandomBodyPart(knockback);
    }
    if (
      GetCharacterVital(CharacterVital.CurrentHealth) < GetCurrentMaxHealth() // don't break on initial hit
      && damageAfterResistances % twentyPercentOfMaxHealth > GetCharacterVital(CharacterVital.CurrentHealth) % twentyPercentOfMaxHealth
    )
    {
      characterVisuals.BreakOffRandomBodyPart(knockback);
    }
  }
  public virtual void PlayDamageSounds()
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
  public virtual void Die()
  {
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
  public void AdjustCurrentHealth(float adjustment, bool isNonlethal = false)
  {
    vitals[CharacterVital.CurrentHealth] =
      Mathf.Clamp(vitals[CharacterVital.CurrentHealth] + adjustment, isNonlethal ? 1 : 0, GetCurrentMaxHealth());
  }

  public void AdjustCurrentMaxHealth(float adjustment)
  {
    vitals[CharacterVital.CurrentMaxHealth] =
     Mathf.Clamp(vitals[CharacterVital.CurrentMaxHealth] + adjustment, 0, GetTrueMaxHealth());
    AdjustCurrentHealth(0);
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

  public void AdjustCurrentStamina(float amount)
  {
    vitals[CharacterVital.CurrentStamina] = Mathf.Min(vitals[CharacterVital.CurrentStamina] + amount, GetMaxStamina()); // no lower bound on current stamina! DFIU!!
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

  public bool HasStamina()
  {
    return GetCharacterVital(CharacterVital.CurrentStamina) > 0;
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
    // Debug.Log("using tile");
    EnvironmentTileInfo et = GridManager.Instance.GetTileAtLocation(currentTileLocation);
    if (et.ChangesFloorLayer())
    {
      StartAscentOrDescent(et.AscendsOrDescends());
    }
  }


  //TODO: this could potentially cause offset issues
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
        Debug.Log("env damage source status: " + envDamage.GetEnvironmentalDamageSourceStatus());
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
        AdjustElementalDamageBuildup(DamageType.Heat, -100);
        RespawnCharacterAtLastSafeLocation();
      }
    }
    else if (tile.groundTileType != null && ascendingDescendingState != AscendingDescendingState.Ascending)
    {
      lastSafeTileLocation = currentTileLocation;
    }
  }

  protected void RespawnCharacterAtLastSafeLocation()
  {
    EndSkill();
    transform.position =
      new Vector3(lastSafeTileLocation.cellCenterWorldPosition.x, lastSafeTileLocation.cellCenterWorldPosition.y, GridManager.GetZOffsetForGameObjectLayer(GetGameObjectLayerFromFloorLayer(lastSafeTileLocation.floorLayer)));
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
    if (!UsingSkill())
    {
      AdjustCurrentStamina(GetStaminaRecoveryRate());
      dashAttackQueued = false;
    }

    if (footstepCooldown > 0)
    {
      footstepCooldown -= Time.deltaTime;
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
            AdjustCurrentHealth(-damage, false);
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

  // accepts ownPosition so pathfinding can consider places we aren't currently
  public bool CanHopUpAtLocation(float ownZPosition, Vector3 wallPosition)
  {
    Vector3 hopCheckLocation = new Vector3(wallPosition.x, wallPosition.y, ownZPosition - .25f); // the spot whose wallObject we want to compare // TODO: CLEAR MAGIC NUMBER
    EnvironmentTileInfo eti = GridManager.Instance.GetTileAtLocation(new TileLocation(hopCheckLocation));
    WallObject wallObject = GridManager.Instance.GetWallObjectAtLocation(new TileLocation(hopCheckLocation));
    float groundHeightOffset = eti.GroundHeight();
    return wallObject == null || !GridManager.Instance.ShouldHaveCollisionWith(eti, ownZPosition - .25f); // TODO: CLEAR MAGIC NUMBER
  }

  public void OnWallObjectCollisionStay(Vector3 wallPosition)
  {
    if (CanUseSkill(hopSkill) && CanHopUpAtLocation(transform.position.z, wallPosition)) // TODO: CLEAR MAGIC NUMBER
    {
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
}
