using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
  RemainingFlightStamina,
  CurrentMoltCount
}

// values driving physical character behavior
// base values live in character data, actual values are always derived from
// base values with some modifications via attribute.
public enum CharacterStat
{
  MaxHealth,
  MaxHealthLostPerMolt,
  DetectableRange,
  MoveAcceleration,
  FlightAcceleration,
  FlightStamina,
  FlightStaminaRecoverySpeed,
  RotationSpeed
}

public enum CharacterAttribute
{
  Attack_Power,
  Attack_Agility,
  Attack_Range,
  Burrow,
  Camouflage,
  HazardResistance,
  Resist_Fungal,
  Resist_Heat,
  Resist_Acid,
  Resist_Physical,
  WaterResistance,
  Flight
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

[System.Serializable]
public class TraitsLoadout_OLD
{
  public string head;
  public string thorax;
  public string abdomen;
  public string legs;
  public string wings;

  public TraitSlotToTraitDictionary EquippedTraits()
  {
    TraitSlotToTraitDictionary d = new TraitSlotToTraitDictionary();
    // d[TraitSlot.Head] = Resources.Load("Data/TraitData/PassiveTraits/" + head) as PassiveTrait;
    // d[TraitSlot.Thorax] = Resources.Load("Data/TraitData/PassiveTraits/" + thorax) as PassiveTrait;
    // d[TraitSlot.Abdomen] = Resources.Load("Data/TraitData/PassiveTraits/" + abdomen) as PassiveTrait;
    // d[TraitSlot.Legs] = Resources.Load("Data/TraitData/PassiveTraits/" + legs) as PassiveTrait;
    // d[TraitSlot.Wings] = Resources.Load("Data/TraitData/PassiveTraits/" + wings) as PassiveTrait;
    return d;
  }

  public string[] AllPopulatedTraitNames
  {
    get
    {
      List<string> ret = new List<string>();
      foreach (string traitName in new string[] { head, thorax, abdomen, legs, wings })
      {
        if (traitName != null)
        {
          ret.Add(traitName);
        }
      }
      return ret.ToArray();
    }
  }
}

// TODO: Character should probably extend CustomPhysicsController, which should extend WorldObject
public class Character : WorldObject
{
  [Header("Stats and Vitals")]
  public CharacterType characterType;
  public CharacterVitalToFloatDictionary vitals;
  public StatToActiveStatModificationsDictionary statModifications;
  public DamageTypeToFloatDictionary damageTypeResistances;

  [Header("Attack Info")]
  public List<CharacterSkillData> characterSkills;
  public Dictionary<string, Weapon> weaponInstances;
  // public Weapon weaponInstance;
  public CharacterAttackModifiers attackModifiers;
  public Hitbox_OLD hitboxPrefab;
  private bool dashAttackEnabled = true;
  public AttackSkillData dashCharacterAttack;
  public CharacterAttackModifiers dashAttackModifiers;
  public Hitbox_OLD dashAttackHitboxPrefab;

  [Header("Trait Info", order = 1)]
  public TraitSlotToTraitDictionary traits;
  public CharacterAttributeToIntDictionary attributes;
  public List<TraitEffect> conditionallyActivatedTraitEffects;
  public List<TraitEffect> activeConditionallyActivatedTraitEffects;
  public List<CharacterMovementAbility> activeMovementAbilities;

  [Header("Child Components")]
  public Transform orientation;

  public Transform weaponPivot;
  public Transform crosshair;
  // our physics object
  public CustomPhysicsController po;
  // animation object
  protected Animator animator;
  public SpriteRenderer[] renderers;

  public CircleCollider2D circleCollider;
  public CharacterVisuals characterVisuals;

  [Header("Game State Info")]
  public Color damageFlashColor = Color.red;
  public Color attackColor = Color.grey;
  public float damageFlashSpeed = 1.0f;
  public bool attacking = false;
  public bool flying = false;
  protected bool attackCooldown = false;
  protected bool stunned = false;
  public bool animationPreventsMoving = false;
  public bool sticking = false;
  public Coroutine attackCoroutine;
  protected Coroutine flyCoroutine;
  protected Vector2 movementInput;
  // point in space we would like to face
  public Vector3 orientTowards;
  protected TileLocation currentTileLocation;
  protected EnvironmentTileInfo currentTile;
  protected float timeStandingStill = 0;
  protected float timeMoving = 0;
  protected Dictionary<string, GameObject> traitSpawnedGameObjects;
  protected List<string> sourceInvulnerabilities;

  [Header("Default Info")]
  public CharacterData defaultCharacterData;

  private Coroutine damageFlashCoroutine;

  public Color damagedColor;

  [Header("Prefabs")]
  public MoltCasting moltCasingPrefab;

  protected virtual void Awake()
  {
    orientation = transform.Find("Orientation");
    circleCollider = GetComponent<CircleCollider2D>();
    if (orientation == null)
    {
      Debug.LogError("No object named 'Orientation' on Character object: " + gameObject.name);
      return;
    }
    // weaponPivot = orientation.Find("WeaponPivot");
    // if (weaponPivot == null)
    // {
    //   Debug.LogError("No object named 'WeaponPivot' on Character object: " + gameObject.name);
    // }
  }

  protected virtual void Start()
  {
    movementInput = new Vector2(0, 0);
    animator = GetComponent<Animator>();
    if (po == null)
    {
      Debug.LogError("No physics controller component on Character object: " + gameObject.name);
    }
    WorldObject.ChangeLayersRecursively(transform, currentFloor);
  }

  protected virtual void Init()
  {
    sourceInvulnerabilities = new List<string>();
    conditionallyActivatedTraitEffects = new List<TraitEffect>();
    traitSpawnedGameObjects = new Dictionary<string, GameObject>();
    if (weaponInstances != null)
    {
      foreach (Weapon w in weaponInstances.Values)
      {
        Destroy(w.gameObject);
      }
      weaponInstances.Clear();
    }
    else
    {
      weaponInstances = new Dictionary<string, Weapon>();
    }
    characterSkills = CalculateSkills(traits);
    if (!HasAttackSkill(characterSkills))
    {
      characterSkills.Insert(0, defaultCharacterData.defaultCharacterAttack);
    }
    for (int i = 0; i < characterSkills.Count; i++)
    {
      characterSkills[i].Init(this);
    }
    attributes = CalculateAttributes(traits);
    InitializeFromCharacterData();
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
    // statModifications = new StatToActiveStatModificationsDictionary();
    vitals[CharacterVital.CurrentHealth] = GetMaxHealth();
    vitals[CharacterVital.RemainingFlightStamina] = GetMaxFlightDuration();
    // vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] = GetStat(CharacterStat.MaxEnvironmentalDamageCooldown);
    // vitals[CharacterVital.CurrentDashCooldown] = GetStat(CharacterStat.MaxDashCooldown);
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
    foreach (Trait trait in traits.Values)
    {
      if (trait == null) { continue; }
      foreach (CharacterAttribute attribute in trait.attributeModifiers.Keys)
      {
        ret[attribute] += trait.attributeModifiers[attribute];
      }
    }
    return ret;
  }

  public virtual CharacterSkillData GetSelectedCharacterSkill()
  {
    if (characterSkills.Count > 0)
    {
      return characterSkills[0];
    }
    return null;
  }

  public virtual Weapon GetSelectedWeaponInstance()
  {
    if (characterSkills.Count > 0)
    {
      string skillName = GetSkillNameFromIndex(0);
      return weaponInstances[skillName];
    }
    return null;
  }
  public static List<CharacterSkillData> CalculateSkills(TraitSlotToTraitDictionary traits)
  {
    List<CharacterSkillData> ret = new List<CharacterSkillData>();
    foreach (Trait trait in traits.Values)
    {
      if (trait == null) { continue; }
      if (trait.skillData != null)
      {
        ret.Add(trait.skillData);
      }
    }
    return ret;
  }

  public static bool HasAttackSkill(List<CharacterSkillData> skills)
  {
    foreach (CharacterSkillData skill in skills)
    {
      if ((AttackSkillData)skill != null)
      {
        return true;
      }
    }
    return false;
  }
  // non-physics biz
  protected virtual void Update()
  {
    HandleHealth();
    HandleFacingDirection();
    HandleTile();
    HandleCooldowns();
    HandleConditionallyActivatedTraits();
  }

  // physics biz
  protected virtual void FixedUpdate()
  {
    CalculateMovement();
  }

  // WIP: COMBOS

  // How combos should work

  // if you're not attacking, Attack calls BeginAttack and queues your first attack
  // if you ARE attacking, and haven't queued an attack,
  //   Attack calls QueueNextAttack and queues a subsequent attack
  // if you are attacking and HAVE queued an attack,
  //   Attack resets the coroutine
  // at the end of an attack, (Weapon.FinishAttack) if a next attack is queued and present, it fires
  // if an attack is NOT queued, the combo is reset, and attacking is reset to false (Weapon.FinishCombo)

  // called via play input or npc AI

  public void UseAttack(AttackSkillData attack)
  {
    if (attack != null)
    {
      if (attackCoroutine == null)
      // if (!attackCooldown)
      {
        attackCoroutine = StartCoroutine(DoAttack(attack));
      }
    }
  }

  public IEnumerator DoAttack(AttackSkillData attack)
  {
    attacking = true;
    Debug.Log("performing attack??");
    yield return StartCoroutine(attack.PerformAttackCycle(this));
    attacking = false;
    attackCooldown = false;
    attackCoroutine = null;
  }

  public static float GetAttackValueModifier(CharacterAttackValueToIntDictionary attackModifiers, CharacterAttackValue value)
  {
    if (attackModifiers == null || !attackModifiers.ContainsKey(value))
    {
      return 0;
    }
    return attackModifiers[value] * Constants.CharacterAttackAdjustmentIncrements[value];
  }

  // // TODO: Refactor attack info so that it all lives on a single object (...maybe)
  // public void CreateAttackHitbox()
  // {
  //   CreateHitbox(defaultCharacterAttack, attackModifiers);
  // }

  public void CreateDashAttackHitbox()
  {
    CreateHitbox(dashCharacterAttack, dashAttackModifiers);
  }

  public string GetSkillNameFromIndex(int idx)
  {
    return characterSkills[idx].name;
  }
  public float GetAttackRange(int skillIdxForAttack = 0)
  {
    string skillName = GetSkillNameFromIndex(skillIdxForAttack);
    return weaponInstances[skillName].range;
    // return attack.range + Character.GetAttackValueModifier(mods.attackValueModifiers, CharacterAttackValue.Range);
  }

  public float GetEffectiveAttackRange(int skillIdxForAttack = 0)
  {
    string skillName = GetSkillNameFromIndex(skillIdxForAttack);
    return weaponInstances[skillName].effectiveRange;
  }

  public float GetRange(int skillIdxForAttack = 0)
  {
    string skillName = GetSkillNameFromIndex(skillIdxForAttack);
    return weaponInstances[skillName].range;
    // return attack.range + Character.GetAttackValueModifier(mods.attackValueModifiers, CharacterAttackValue.Range);
  }
  public int GetAttackRadiusInDegrees(int skillIdxForAttack)
  {
    string skillName = GetSkillNameFromIndex(skillIdxForAttack);
    return weaponInstances[skillName].sweepRadiusInDegrees;
  }
  public void CreateHitbox(AttackSkillData atk, CharacterAttackModifiers mods)
  {
    // HitboxData hbi = atk.hitboxData;
    // if (hbi == null) { Debug.LogError("no attack object defined for " + gameObject.name); }
    // Vector3 pos = orientation.TransformPoint(Vector3.right * GetAttackRange(atk, mods) / 2);
    // Hitbox_OLD hb = GameObject.Instantiate(hitboxPrefab, pos, orientation.rotation) as Hitbox_OLD;
    // hb.gameObject.layer = LayerMask.NameToLayer(currentFloor.ToString());
    // hb.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = currentFloor.ToString();
    // hb.Init(orientation, this, hbi, mods, GetAttackRange(atk, mods));
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
  // public void ApplyDashAttackModifier(CharacterAttackValue attackValue, int magnitude)
  // {
  //   dashAttackEnabled = true;
  //   Debug.Log("applying dash attack modifier: " + attackValue + "," + magnitude);
  //   dashAttackModifiers[attackValue] += magnitude;
  // }

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
      (attacking || animationPreventsMoving || stunned)
      && !activeMovementAbilities.Contains(CharacterMovementAbility.Halteres)
    )
    {
      return;
    }
    Quaternion targetDirection = GetTargetDirection();
    orientation.rotation = Quaternion.Slerp(orientation.rotation, targetDirection, GetRotationSpeed() * Time.deltaTime);
  }

  // Rotate character smoothly towards a particular orientation around the Z axis.
  // Warning: I don't understand this math. If character rotation seems buggy, this is a
  // potential culprit.
  Quaternion GetTargetDirection()
  {
    return GetDirectionAngle(orientTowards);
  }

  Quaternion GetDirectionAngle(Vector3 targetPoint)
  {
    Vector2 target = targetPoint - transform.position;
    float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    return Quaternion.AngleAxis(angle, Vector3.forward);
  }

  // used to calculate how far our facing direction is from our target facing direction.
  // Useful for e.g. deciding if an enemy is facing a player enough for attacking to be a good idea.
  public float GetAngleToTarget()
  {
    return Quaternion.Angle(GetTargetDirection(), orientation.rotation);
  }

  public float GetAngleToDirection(Vector3 targetPoint)
  {
    return Quaternion.Angle(GetDirectionAngle(targetPoint), orientation.rotation);
  }

  // add input to our velocity, if necessary/possible.
  protected void CalculateMovement()
  {
    if (CanMove())
    {
      if (movementInput == Vector2.zero)
      { // should be an approximate equals
        timeStandingStill += Time.deltaTime;
        timeMoving = 0;
      }
      else
      {
        timeMoving += Time.deltaTime;
        timeStandingStill = 0f;
      }
      // if (flying)
      // {
      //   po.SetMovementInput(orientation.rotation * new Vector3(1, 0, 0));
      // }
      // else
      // {
      po.SetMovementInput(new Vector2(movementInput.x, movementInput.y));
      // }
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
      currentTileLocation.position.x + .5f,
      currentTileLocation.position.y + .5f,
      0f
    );
  }
  protected void Molt()
  {
    Instantiate(moltCasingPrefab, orientation.transform.position, orientation.transform.rotation);
    // moltCasingPrefab.Init(mainRenderer.color, this);
    AdjustCurrentMoltCount(1);
    AdjustCurrentHealth(5);
  }
  // Traits activated on dash are handled in HandleConditionallyActivatedTraits()
  // todo: fly not dash??
  protected void Fly()
  {
    // if (vitals[CharacterVital.CurrentFlightCooldown] > 0) { return; }
    // DoDashAttack();
    BeginFly();
    // flyCoroutine = StartCoroutine(DoFly());
    // vitals[CharacterVital.CurrentFlightCooldown] = GetStat(CharacterStat.MaxDashCooldown);
  }

  // protected IEnumerator DoFly()
  // {
  //   float t = 0;
  //   BeginFly();
  //   while (t < GetMaxFlightDuration() && flying)
  //   {
  //     t += Time.deltaTime;
  //     yield return null;
  //   }
  //   EndFly();
  // }

  protected void BeginFly()
  {
    flying = true;
    if (GetAttribute(CharacterAttribute.Flight) < 3)
    { // TODO: don't hardcode this!!
      activeMovementAbilities.Add(CharacterMovementAbility.Hover);
    }
    ChangeLayersRecursively(transform, currentFloor, -.5f);
  }

  protected void EndFly()
  {
    // StopCoroutine(flyCoroutine);
    flying = false;
    if (GetAttribute(CharacterAttribute.Flight) < 3)
    { // TODO: don't hardcode this!!
      activeMovementAbilities.Remove(CharacterMovementAbility.Hover);
    }
    ChangeLayersRecursively(transform, currentFloor, .0f);
  }

  protected void FlyUp()
  {
    // consume stamina here
    AscendOneFloor();
  }

  protected void FlyDown()
  {
    if (GridManager.Instance.TileIsValidAndEmpty(GetTileLocation()))
    {
      DescendOneFloor();
    }
    else
    {
      EndFly();
    }
  }

  protected void DoDashAttack()
  {
    if (dashAttackEnabled)
    {
      CreateDashAttackHitbox();
    }
  }

  protected void AscendOneFloor()
  {

    Debug.Log("ascending one floor!");
    SetCurrentFloor(currentFloor + 1);
  }

  protected void DescendOneFloor()
  {
    SetCurrentFloor(currentFloor - 1);
  }

  // determines if input-based movement is allowed
  protected virtual bool CanMove()
  {
    if (!activeMovementAbilities.Contains(CharacterMovementAbility.Halteres))
    {
      if (attacking)
      {
        return false;
      }
      if (animationPreventsMoving)
      {
        return false;
      }
    }
    if (stunned)
    {
      return false;
    }
    return true;
  }

  // DAMAGE FUNCTIONS
  protected virtual void TakeDamage(IDamageSource damageSource)
  {
    if (damageSource.IsOwnedBy(this)) { return; }
    if (damageSource.IsSameOwnerType(this)) { return; }
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
    InterruptAnimation();
    AdjustCurrentHealth(Mathf.Floor(-damageAfterResistances));
    CalculateAndApplyStun(damageSource.stunMagnitude);
    StartCoroutine(ApplyInvulnerability(damageSource));
    Vector3 knockback = damageSource.GetKnockbackForCharacter(this);
    if (knockback != Vector3.zero)
    {
      po.ApplyImpulseForce(knockback);
    }
  }

  public int GetDamageTypeResistanceLevel(DamageType type)
  {
    bool exists = Enum.TryParse("Resist_" + type.ToString(), out CharacterAttribute resistAttribute);
    if (!exists)
    {
      Debug.LogError("Could not find attribute Resist_" + type.ToString());
      return 0;
    }
    return GetAttribute(resistAttribute);
  }

  private IEnumerator ApplyDamageFlash(DamageData_OLD damageObj)
  {
    // Todo: might wanna change this!
    Color baseColor = Color.white;
    yield return null;
    // mainRenderer.color = damageFlashColor;
    // yield return new WaitForSeconds(damageFlashSpeed / 3);
    // mainRenderer.color = baseColor;
    // yield return new WaitForSeconds(damageFlashSpeed / 3);
    // mainRenderer.color = damageFlashColor;
    // yield return new WaitForSeconds(damageFlashSpeed / 3);
    // mainRenderer.color = baseColor;
  }
  public virtual void Die()
  {
    Destroy(gameObject);
  }

  IEnumerator ApplyInvulnerability(IDamageSource damageSource)
  {
    float invulnerabilityDuration = damageSource.invulnerabilityWindow;
    if (invulnerabilityDuration <= 0) { yield break; }
    string src = damageSource.sourceString;
    sourceInvulnerabilities.Add(src);
    yield return new WaitForSeconds(invulnerabilityDuration);
    if (sourceInvulnerabilities.Contains(src))
    {
      sourceInvulnerabilities.Remove(src);
    }
  }
  // IEnumerator ApplyInvulnerability(Damage_OLD damage)
  // {
  //     string src = damage.sourceString;
  //     sourceInvulnerabilities.Add(src);
  //     yield return new WaitForSeconds(damage.GetInvulnerabilityWindow());
  //     if (sourceInvulnerabilities.Contains(src))
  //     {
  //         sourceInvulnerabilities.Remove(src);
  //     }
  // }

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

  // End current attack/attack animation/combo and reset us to idle.
  // Used to keep us from finishing our attack after getting knocked across the screen.
  // TODO: it should be possible to "tank" some attacks and finish attacking
  void InterruptAnimation()
  {
    if (animator != null)
    {
      animator.SetTrigger("transitionToIdle");
    }
  }
  // END DAMAGE FUNCTIONS


  public virtual void AssignTraitsForNextLife(TraitSlotToUpcomingTraitDictionary nextLifeTraits)
  {

  }

  // ATTRIBUTE/VITALS ACCESSORS
  public int GetAttribute(CharacterAttribute attributeToGet)
  {
    bool exists = attributes.TryGetValue(attributeToGet, out int val);
    if (!exists)
    {
      Debug.LogError("Tried to access non-existant attribute " + attributeToGet);
    }
    return val;
  }

  // STAT GETTERS
  public float GetMaxHealth()
  {
    return
        defaultCharacterData.defaultStats[CharacterStat.MaxHealth]
        - (GetMaxHealthLostPerMolt() * GetCharacterVital(CharacterVital.CurrentMoltCount));
  }

  public float GetRotationSpeed()
  {
    return defaultCharacterData.defaultStats[CharacterStat.RotationSpeed];
  }

  public float GetMaxHealthLostPerMolt()
  {
    return defaultCharacterData.defaultStats[CharacterStat.MaxHealthLostPerMolt];
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
    Debug.Log("defaultCharacterData: " + defaultCharacterData);
    Debug.Log("flightAttributeData: " + defaultCharacterData.GetFlightAttributeData());
    Debug.Log("tier: " + defaultCharacterData.GetFlightAttributeData().GetAttributeTier(this));
    return defaultCharacterData
      .GetFlightAttributeData()
      .GetAttributeTier(this)
      .flightDuration;
  }

  public float GetStat(CharacterStat statToGet)
  {
    StringToIntDictionary statMods = statModifications[statToGet];
    int modValue = 0;
    float returnValue = defaultCharacterData.defaultStats[statToGet];
    foreach (int modMagnitude in statMods.Values)
    {
      modValue += modMagnitude;
    }
    modValue = Mathf.Clamp(-12, modValue, 12);
    if (modValue >= 0)
    {
      returnValue *= ((3 + modValue) / 3);
    }
    else
    {
      returnValue *= (3f / (3 + Mathf.Abs(modValue)));
    }
    return returnValue;
  }


  // public void ApplyStatMod(CharacterStatModification mod)
  // {
  //     if (mod.duration > 0)
  //     {
  //         StartCoroutine(ApplyTemporaryStatMod(mod));
  //     }
  //     else
  //     {
  //         AddStatMod(mod);
  //     }
  // }

  // public IEnumerator ApplyTemporaryStatMod(CharacterStatModification mod)
  // {
  //     AddStatMod(mod);
  //     yield return new WaitForSeconds(mod.duration);
  //     RemoveStatMod(mod.source);
  // }

  // public void AddStatMod(CharacterStatModification mod)
  // {
  //     AddStatMod(mod.statToModify, mod.magnitude, mod.source);
  // }

  // public void AddStatMod(CharacterStat statToMod, int magnitude, string source)
  // {
  //     statModifications[statToMod][source] = magnitude;
  // }

  // public void RemoveStatMod(string source)
  // {
  //     foreach (CharacterStat stat in (CharacterStat[])Enum.GetValues(typeof(CharacterStat)))
  //     {
  //         StringToIntDictionary statMods = statModifications[stat];
  //         if (statMods.ContainsKey(source))
  //         {
  //             statMods.Remove(source);
  //         }
  //     }
  // }

  public void AddMovementAbility(CharacterMovementAbility movementAbility)
  {
    activeMovementAbilities.Add(movementAbility);
  }

  public void RemoveMovementAbility(CharacterMovementAbility movementAbility)
  {
    activeMovementAbilities.Remove(movementAbility);
  }


  //VITALS GETTERS/SETTERS
  public void AdjustCurrentHealth(float adjustment)
  {
    vitals[CharacterVital.CurrentHealth] =
      Mathf.Clamp(vitals[CharacterVital.CurrentHealth] + adjustment, 0, GetMaxHealth());
  }

  public void AdjustCurrentMoltCount(float adjustment)
  {
    vitals[CharacterVital.CurrentMoltCount] += adjustment;
  }
  public float GetCharacterVital(CharacterVital vital)
  {
    return vitals[vital];
  }
  public virtual void SetCurrentFloor(FloorLayer newFloorLayer)
  {
    currentFloor = newFloorLayer;
    currentTileLocation = CalculateCurrentTileLocation();
    CenterCharacterOnCurrentTile();
    ChangeLayersRecursively(transform, newFloorLayer, flying ? .5f : 0);
    po.OnLayerChange();
  }

  public void UseTile()
  {
    EnvironmentTileInfo et = GridManager.Instance.GetTileAtLocation(currentTileLocation);
    if (et.ChangesFloorLayer())
    {
      SetCurrentFloor(et.GetTargetFloorLayer(currentFloor));
    }
  }


  //TODO: this could potentially cause offset issues
  public TileLocation CalculateCurrentTileLocation()
  {
    return new TileLocation(
      new Vector2Int(
        Mathf.FloorToInt(transform.position.x),
        Mathf.FloorToInt(transform.position.y)
      ),
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
        vitals[CharacterVital.CurrentHealth] / GetStat(CharacterStat.MaxHealth)
      );
    }
  }
  protected virtual void HandleTile()
  {
    EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
    if (tile == null)
    {
      Debug.LogError("WARNING: no tile found at " + CalculateCurrentTileLocation().ToString());
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
    if (tile.objectTileType == null && tile.groundTileType == null) // Falling logic
    {
      bool canstick = GridManager.Instance.CanStickToAdjacentTile(transform.position, currentFloor);
      if (
        (activeMovementAbilities.Contains(CharacterMovementAbility.StickyFeet) && canstick)
        || activeMovementAbilities.Contains(CharacterMovementAbility.Hover)
        || sticking
      )
      {
        //Character is flying or sticking to something; do not fall
      }
      else
      {
        Debug.Log("Falling: " + gameObject.name);
        transform.position = new Vector3(
          nowTileLocation.position.x + .5f,
          nowTileLocation.position.y + .5f,
          0f
        );
        SetCurrentFloor(currentFloor - 1);
      }
      return;
    }
    if (tile.dealsDamage/* && vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] <= 0*/)
    {
      foreach (EnvironmentalDamage envDamage in tile.environmentalDamageSources)
      {
        TakeDamage(envDamage);
      }
    }
  }

  public virtual void HandleTileCollision(EnvironmentTileInfo tile)
  {
    if (tile.GetColliderType() == Tile.ColliderType.None)
    {
      return;
    }
    else
    {
      // Debug.Log("collided with "+tile);
    }
  }

  public virtual void HandleConditionallyActivatedTraits()
  {
    foreach (TraitEffect trait in conditionallyActivatedTraitEffects)
    {
      switch (trait.activatingCondition)
      {
        case ConditionallyActivatedTraitCondition.Dashing:
          if (flying)
          {
            trait.Apply(this);
          }
          else
          {
            trait.Expire(this);
          }
          break;
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

  protected void HandleCooldowns()
  {
    if (flying)
    {
      vitals[CharacterVital.RemainingFlightStamina] -= Time.deltaTime;
      if (vitals[CharacterVital.RemainingFlightStamina] <= 0)
      {
        EndFly();
      }
    }
    else
    {
      vitals[CharacterVital.RemainingFlightStamina]
        = Mathf.Min(vitals[CharacterVital.RemainingFlightStamina] + Time.deltaTime, GetMaxFlightDuration());
    }
    // if (vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] > 0)
    // {
    //     vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] -= Time.deltaTime;
    // }
    // if (vitals[CharacterVital.CurrentDashCooldown] > 0)
    // {
    //     vitals[CharacterVital.CurrentDashCooldown] -= Time.deltaTime;
    // }
  }

  public virtual void AddAura(TraitEffect effect)
  {
    GameObject go = GameObject.Instantiate(effect.auraPrefab, transform.position, transform.rotation);
    go.layer = gameObject.layer;
    go.GetComponent<Renderer>().sortingLayerName = LayerMask.LayerToName(go.layer);
    go.transform.parent = transform;
    traitSpawnedGameObjects.Add(effect.sourceString, go);
  }

  public virtual void RemoveAura(TraitEffect effect)
  {
    if (traitSpawnedGameObjects.ContainsKey(effect.sourceString))
    {
      Destroy(traitSpawnedGameObjects[effect.sourceString]);
      traitSpawnedGameObjects.Remove(effect.sourceString);
    }
  }
}
