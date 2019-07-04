using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
// TODO: Character can probably extend CustomPhysicsController, which simplifies movement code a bit.
public class AnimationInfoObject {
	public Vector2 animationInput;
}

public enum CharacterVital {
  CurrentHealth,
  CurrentEnvironmentalDamageCooldown
}
public enum CharacterStat {
	MaxHealth,
  DetectableRange,
	MaxEnvironmentalDamageCooldown,
	MoveAcceleration,
	RotationSpeed,
}

public enum CharacterAttackValue {
	Damage,
	Range,
	HitboxSize,
	Knockback,
	Stun,
	AttackSpeed,
	Cooldown,
	Venom
}

// Special modes of character movement.
// Possibly unnecessary!!
public enum CharacterMovementAbility {
	Burrow,
	FastFeet,
	Halteres,
	Hover,
	StickyFeet,
	WaterStride
}

[System.Serializable]
public class CharacterStatModification {
  public CharacterStat statToModify;

  public int magnitude;

	public float applicationDuration;
  public float duration;
  public float delay;

  public string source;

	public CharacterStatModification(CharacterStat s, int m, float dur, float del, string src) {
		statToModify = s;
		magnitude = m;
		duration = dur;
		delay = del;
    source = src;
	}
}

[System.Serializable]
public class ActiveStatModification {
  public int magnitude;
  public string source;

  public ActiveStatModification(int m, string s) {
    magnitude = m;
    source = s;
	}
}

[System.Serializable]
public class CharacterAttack : ScriptableObject {
	public Hitbox hitboxObject;
	public HitboxInfo hitboxInfo;
	public float attackSpeed;
	public float range;
	public float hitboxSize;
	public float cooldown;

	#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create an TraitItem Asset
        [MenuItem("Assets/Create/CharacterAttack")]
        public static void CreatePassiveTrait()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Character Attack", "New Character Attack", "Asset", "Save Character Attack", "Assets/resources/Data/CharacterData/AttackData");
            if (path == "")
                return;
       		AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterAttack>(), path);
        }
    #endif
}
[System.Serializable]
public class TraitsLoadout {
  public string head;
  public string thorax;
  public string abdomen;
  public string legs;
  public string wings;

  public TraitSlotToTraitDictionary EquippedTraits() {
    TraitSlotToTraitDictionary d = new TraitSlotToTraitDictionary();
    d[TraitSlot.Head] = Resources.Load("Data/TraitData/PassiveTraits/"+head) as PassiveTrait;
    d[TraitSlot.Thorax] = Resources.Load("Data/TraitData/PassiveTraits/"+thorax) as PassiveTrait;
    d[TraitSlot.Abdomen] = Resources.Load("Data/TraitData/PassiveTraits/"+abdomen) as PassiveTrait;
    d[TraitSlot.Legs] = Resources.Load("Data/TraitData/PassiveTraits/"+legs) as PassiveTrait;
    d[TraitSlot.Wings] = Resources.Load("Data/TraitData/PassiveTraits/"+wings) as PassiveTrait;
    return d;
  }

  public string[] AllPopulatedTraitNames {
    get {
      List<string> ret = new List<string>();
      foreach (string traitName in new string[] {head, thorax, abdomen, legs, wings }) {
        if (traitName != null) {
          ret.Add(traitName);
        }
      }
      return ret.ToArray();
    }
  }
}

// TODO: Character should probably extend CustomPhysicsController, which should extend WorldObject
public class Character : WorldObject {
  [Header("Stats and Vitals")]
  public CharacterVitalToFloatDictionary vitals;
  public StatToActiveStatModificationsDictionary statModifications;
	public DamageTypeToFloatDictionary damageTypeResistances;

	[Header("Attack Info")]
  public CharacterAttack characterAttack;
	public CharacterAttackValueToIntDictionary attackModifiers;
	public Hitbox hitboxPrefab;

  [Header("Trait Info")]
  public TraitSlotToTraitDictionary equippedTraits;
  public List<TraitEffect> conditionallyActivatedTraitEffects;
  public List<TraitEffect> activeConditionallyActivatedTraitEffects;
	public List<CharacterMovementAbility> activeMovementAbilities;

  [Header("Child Components")]
	public Transform orientation;
	public Transform crosshair;
	// our physics object
	public CustomPhysicsController po;
	// animation object
	protected Animator animator;

  [Header("Game State Info")]
	public bool attacking = false;
	protected bool stunned = false;
	public bool animationPreventsMoving = false;
	public bool sticking = false;
	public Coroutine attackCoroutine;
  public FloorLayer currentFloor;
	protected Vector2 movementInput;
	// point in space we would like to face
	protected Vector3 orientTowards;
	protected TileLocation currentTileLocation;
	protected EnvironmentTileInfo currentTile;
  protected float timeStandingStill = 0;
  protected Dictionary<string, GameObject> traitSpawnedGameObjects;
  protected List<string> sourceInvulnerabilities;

	[Header("Default Info")]
  public CharacterData defaultCharacterData;
  public TraitsLoadout initialEquippedTraits;
	public string initialActiveTrait1;
	public string initialActiveTrait2;

	protected virtual void Awake() {
		orientation = transform.Find("Orientation");
		if (orientation == null) {
			Debug.LogError("No object named 'Orientation' on Character object: "+gameObject.name);
		}
	}

	// TODO: this long list of GetComponents is messy; can it be cleaned up?
	protected virtual void Start () {
		movementInput = new Vector2(0,0);
		animator = GetComponent<Animator>();
		if (po == null) {
			Debug.LogError("No physics controller component on Character object: "+gameObject.name);
		}
        ChangeLayersRecursively(transform, currentFloor.ToString());
	}

	protected virtual void Init() {
    sourceInvulnerabilities = new List<string>();
    conditionallyActivatedTraitEffects = new List<TraitEffect>();
    traitSpawnedGameObjects = new Dictionary<string, GameObject>();
		InitializeFromCharacterData();
	}

	private void InitializeFromCharacterData() {
		if (defaultCharacterData != null) {
			CharacterData dataInstance = (CharacterData) ScriptableObject.Instantiate(defaultCharacterData);
      attackModifiers = dataInstance.attackModifiers;
			damageTypeResistances = dataInstance.damageTypeResistances;
			activeMovementAbilities.AddRange(dataInstance.movementAbilities);
		}
    vitals = new CharacterVitalToFloatDictionary();
	}

	// non-physics biz
	protected virtual void Update() {
		HandleHealth();
		HandleFacingDirection();
		HandleTile();
    HandleConditionallyActivatedTraits();
	}

	// physics biz
	void FixedUpdate () {
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
	protected void Attack() {
		if (characterAttack != null) {
			if (!attacking) {
				attackCoroutine = StartCoroutine(DoAttack());
			}
		}
	}

	public IEnumerator DoAttack() {
		attacking = true;
		yield return new WaitForSeconds(characterAttack.attackSpeed);
		CreateHitbox();
		yield return new WaitForSeconds(characterAttack.cooldown);
		attacking = false;
		attackCoroutine = null;
	}
	public static float GetAttackValueModifier(CharacterAttackValueToIntDictionary attackModifiers, CharacterAttackValue value) {
		if (attackModifiers == null || !attackModifiers.ContainsKey(value)) {
			return 0;
		}
		return attackModifiers[value] * Constants.CharacterAttackAdjustmentIncrements[value];
	}

	// TODO: Refactor attack info so that it all lives on a single object (...maybe)
	public void CreateHitbox() {
		HitboxInfo hbi = characterAttack.hitboxInfo;
		if (hbi == null) { Debug.LogError("no attack object defined for "+gameObject.name); }
		Vector3 pos = orientation.TransformPoint(Vector3.right * (characterAttack.range + Character.GetAttackValueModifier(attackModifiers, CharacterAttackValue.Range)));
 		Hitbox hb = GameObject.Instantiate(hitboxPrefab, pos, orientation.rotation) as Hitbox;
		hb.gameObject.layer = LayerMask.NameToLayer(currentFloor.ToString());
		hb.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = currentFloor.ToString();
		hb.Init(orientation, this, hbi, characterAttack.hitboxInfo.damageMultipliers, attackModifiers);
	}

	public void ApplyAttackModifier(CharacterAttackValue attackValue, int magnitude) {
		attackModifiers[attackValue] += magnitude;
	}

	public void SetAnimationInput(Vector2 newAnimationInput) {
		po.SetAnimationInput(newAnimationInput);
	}

	public void SetAnimationPreventsMoving(bool newAnimationPreventsMoving) {
		animationPreventsMoving = newAnimationPreventsMoving;
	}
	// Point character towards a rotation target.
	void HandleFacingDirection() {
		if (
			(attacking || animationPreventsMoving || stunned)
			&& !activeMovementAbilities.Contains(CharacterMovementAbility.Halteres)
		) {
			return;
		}
		Quaternion targetDirection = GetTargetDirection();
		orientation.rotation = Quaternion.Slerp(orientation.rotation, targetDirection, GetStat(CharacterStat.RotationSpeed) * Time.deltaTime);
	}

	// Rotate character smoothly towards a particular orientation around the Z axis.
	// Warning: I don't understand this math. If character rotation seems buggy, this is a
	// potential culprit.
	Quaternion GetTargetDirection() {
		Vector3 target = orientTowards - transform.position;
		float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
		return Quaternion.AngleAxis(angle, Vector3.forward);
	}

	// used to calculate how far our facing direction is from our target facing direction.
	// Useful for e.g. deciding if an enemy is facing a player enough for attacking to be a good idea.
	public float GetAngleToTarget() {
		return Quaternion.Angle(GetTargetDirection(), orientation.rotation);
	}

	// add input to our velocity, if necessary/possible.
	protected void CalculateMovement() {
		if (CanMove()) {
      if (movementInput == Vector2.zero) { // should be an approximate equals
        timeStandingStill += Time.deltaTime;
      } else {
        timeStandingStill = 0f;
      }
      po.SetMovementInput(new Vector2(movementInput.x, movementInput.y));

		}
		else {
			po.SetMovementInput(Vector2.zero);
		}
	}

	// determines if input-based movement is allowed
	protected virtual bool CanMove() {
		if (!activeMovementAbilities.Contains(CharacterMovementAbility.Halteres)) {
			if (attacking) {
				return false;
			}
			if (animationPreventsMoving) {
				return false;
			}
		}
		if (stunned) {
			return false;
		}
		return true;
	}

	// DAMAGE FUNCTIONS

	// Called from Hitbox's OnTriggerEnter. Calls other functions to determine outcome of getting hit.
	protected void TakeDamage(DamageObject damageObj) {
		if (
      sourceInvulnerabilities.Contains(damageObj.sourceString)
      && !damageObj.ignoreInvulnerability)
    { return; }
		float damageAfterResistances = (100 - damageTypeResistances[damageObj.damageType]) * damageObj.damage;
		if (
      damageAfterResistances <= 0
      && damageObj.damage > 0
      && damageObj.characterStatModifications.Count == 0
    ) { return; }
		if (damageObj.attackerTransform == transform) { return; }
		InterruptAnimation();
		AdjustHealth(Mathf.Floor(-damageAfterResistances));
		CalculateAndApplyStun(damageObj.stun);
		foreach(CharacterStatModification mod in damageObj.characterStatModifications) {
			AddStatMod(mod);
		}
		StartCoroutine(ApplyInvulnerability(damageObj));
		// StartCoroutine(ApplyDamageFlash(damageObj));
		if (damageObj.attackerTransform != null && damageObj.hitboxTransform != null) {
			po.ApplyImpulseForce(damageObj.knockback *
				(damageObj.hitboxTransform.position
				- damageObj.attackerTransform.position)
			);
		}
	}

	public virtual void Die() {
		Destroy(gameObject);
	}

	public virtual void AssignTraitsForNextLife(TraitSlotToUpcomingTraitDictionary nextLifeTraits) {

	}

	// Make us invulnerable, then un-make-us invulnerable. Damage is ignorned while invulnerable.
	void CalculateAndApplyInvulnerability(DamageObject dObj) {
		if (dObj.invulnerabilityWindow > 0) {
			StartCoroutine(ApplyInvulnerability(dObj));
		}
	}

	IEnumerator ApplyInvulnerability(DamageObject dObj) {
    string src = dObj.sourceString;
		sourceInvulnerabilities.Add(src);
		yield return new WaitForSeconds(dObj.invulnerabilityWindow);
		if (sourceInvulnerabilities.Contains(src)) {
      sourceInvulnerabilities.Remove(src);
    }
	}

	// Determines if we should be stunned, and stuns us if so.
	protected void CalculateAndApplyStun(float stunDuration, bool overrideStunResistance = false) {
		if (!overrideStunResistance) {
			// TODO: Stun resistance/reduction should happen here
		}
		if (stunDuration > 0) {
			StartCoroutine(ApplyStun(stunDuration));
		}
	}

	// Make us stunned, then un-make-us stunned. Cannot move or attack while stunned.

	IEnumerator ApplyStun(float stunDuration) {
		stunned = true;
		yield return new WaitForSeconds(stunDuration);
		stunned = false;
	}

	// End current attack/attack animation/combo and reset us to idle.
	// Used to keep us from finishing our attack after getting knocked across the screen.
	// TODO: it should be possible to "tank" some attacks and finish attacking
	void InterruptAnimation() {
		if (animator != null) {
			animator.SetTrigger("transitionToIdle");
		}
	}
	// END DAMAGE FUNCTIONS

  public float GetStat(CharacterStat statToGet) {
    StringToIntDictionary statMods = statModifications[statToGet];
    int modValue = 0;
    float returnValue = Constants.BaseCharacterStats[statToGet];
    foreach(int modMagnitude in statMods.Values) {
      modValue += modMagnitude;
    }
    modValue = Mathf.Clamp(-10, modValue, 10);
    if (modValue >= 0) {
      returnValue *= ((3 + modValue) / 3);
    } else {
      Debug.Log ("some math: "+ 3f / (3 + Mathf.Abs(modValue)));
      returnValue *= (3f / (3 + Mathf.Abs(modValue)));
      Debug.Log ("return value: "+ returnValue);
    }
    return returnValue;
  }

public void AddStatMod(CharacterStatModification mod) {
  AddStatMod(mod.statToModify, mod.magnitude, mod.source);
}

public void AddStatMod(CharacterStat statToMod, int magnitude, string source) {
    Debug.Log("addStatMod " + statToMod + source + magnitude);
    statModifications[statToMod][source] = magnitude;
  }

  public void RemoveStatMod(string source) {
    foreach(CharacterStat stat in (CharacterStat[]) Enum.GetValues(typeof(CharacterStat))) {
      StringToIntDictionary statMods =  statModifications[stat];
      if (statMods.ContainsKey(source)) {
        statMods.Remove(source);
      }
    }
  }

	public void AddMovementAbility(CharacterMovementAbility movementAbility) {
		activeMovementAbilities.Add(movementAbility);
	}

	public void RemoveMovementAbility(CharacterMovementAbility movementAbility) {
		activeMovementAbilities.Remove(movementAbility);
	}


//TODO: SHOULD PROBS BE DEPRECATED
	public void AdjustHealth(float adjustment) {
		vitals[CharacterVital.CurrentHealth] =
      Mathf.Clamp(vitals[CharacterVital.CurrentHealth] + adjustment, 0, GetStat(CharacterStat.MaxHealth));
	}

  public virtual void SetCurrentFloor(FloorLayer newFloorLayer)
    {
      currentFloor = newFloorLayer;
      currentTileLocation = CalculateCurrentTileLocation();
      ChangeLayersRecursively(transform, newFloorLayer.ToString());
      po.OnLayerChange();
    }

	public void UseTile() {
		EnvironmentTileInfo et = GridManager.Instance.GetTileAtLocation(currentTileLocation);
		if (et.ChangesFloorLayer()) {
			SetCurrentFloor(et.GetTargetFloorLayer());
		}
	}

	private static void ChangeLayersRecursively(Transform trans, string name)
	{
		trans.gameObject.layer = LayerMask.NameToLayer(name);
		SpriteRenderer r = trans.gameObject.GetComponent<SpriteRenderer>();
		if (r != null) {
			r.sortingLayerName = name;
		}
		foreach(Transform child in trans)
		{
			ChangeLayersRecursively(child, name);
		}
 	}

	//TODO: this could potentially cause offset issues
	public TileLocation CalculateCurrentTileLocation() {
		return new TileLocation(
			new Vector2Int(
				Mathf.FloorToInt(transform.position.x),
				Mathf.FloorToInt(transform.position.y)
			),
			currentFloor
		);
	}
	protected virtual void HandleHealth() {
		if (vitals[CharacterVital.CurrentHealth] <= 0) {
			Die();
		}
	}
	protected virtual void HandleTile() {
		EnvironmentTileInfo tile = GridManager.Instance.GetTileAtLocation(CalculateCurrentTileLocation());
		if (tile == null) {
			Debug.LogError("WARNING: no tile found at "+CalculateCurrentTileLocation().ToString());
			return;
		}
		if (tile != currentTile) {
			currentTile = tile;
		}
		TileLocation nowTileLocation = CalculateCurrentTileLocation();
		if (currentTileLocation != nowTileLocation) {
			currentTileLocation = nowTileLocation;
		}
		if (vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] > 0) {
			vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] -= Time.deltaTime;
		}
		if (tile.objectTileType == null && tile.groundTileType == null) {
			if (
				(activeMovementAbilities.Contains(CharacterMovementAbility.StickyFeet)
				&& GridManager.Instance.CanStickToAdjacentTile(transform.position, currentFloor))
				|| sticking
			) {
				// do nothing. no fall plz.
			} else {
				transform.position = new Vector3 (
					nowTileLocation.position.x + .5f,
					nowTileLocation.position.y + .5f,
					0f
				);
				SetCurrentFloor(currentFloor - 1);
			}
			return;
		}
		if (tile.dealsDamage && tile.environmentalDamage != null && vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] <= 0) {
			TakeDamage(tile.environmentalDamage);
			vitals[CharacterVital.CurrentEnvironmentalDamageCooldown] = GetStat(CharacterStat.MaxEnvironmentalDamageCooldown);
		}
	}

	public virtual void HandleTileCollision(EnvironmentTileInfo tile) {
		if (tile.GetColliderType() == Tile.ColliderType.None) {
			return;
		}
		else {
			// Debug.Log("collided with "+tile);
		}
	}

  public virtual void HandleConditionallyActivatedTraits() {
    foreach(TraitEffect trait in conditionallyActivatedTraitEffects) {
      switch (trait.activatingCondition) {
        case ConditionallyActivatedTraitCondition.NotMoving:
          if (timeStandingStill > trait.activatingConditionRequiredDuration) {
            trait.Apply(this);
          }
          else {
            trait.Expire(this);
          }
          break;
      }
    }
  }

  public virtual void AddAura(TraitEffect effect) {
    GameObject go = GameObject.Instantiate(effect.auraPrefab, transform.position, transform.rotation);
    go.layer = gameObject.layer;
    go.GetComponent<Renderer>().sortingLayerName = LayerMask.LayerToName(go.layer);
    go.transform.parent = transform;
    traitSpawnedGameObjects.Add(effect.sourceString, go);
  }

  public virtual void RemoveAura(TraitEffect effect) {
    if (traitSpawnedGameObjects.ContainsKey(effect.sourceString)) {
      Destroy(traitSpawnedGameObjects[effect.sourceString]);
      traitSpawnedGameObjects.Remove(effect.sourceString);
    }
  }
}
