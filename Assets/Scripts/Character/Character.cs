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

//TODO: Character stats should not be referenced from here.
// We should have a getter for each stat that gets the actual value from here,
// then applies stat mods on top of it.
// CharacterStatModifiers should be a dictionary<CharacterStat, CharacterStatModifier>
// CharacterStatModifier should be a struct with source and magnitude
public enum CharacterStat {
	CurrentHealth,
	CurrentMaxHealth,
	MaxHealth,
	CurrentEnvironmentalDamageCooldown,
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

  public float magnitude;

	public float applicationDuration;
    public float duration;
    public float delay;

	public CharacterStatModification(CharacterStat s, float m, float dur, float del) {
		statToModify = s;
		magnitude = m;
		duration = dur;
		delay = del;
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
public class EquippedTraitsStringNames {

  [SerializeField]
  public string head;
  public string thorax;
  public string abdomen;
  public string legs;
  public string wings;

  public TraitSlotToTraitDictionary EquippedTraits() {
    Debug.Log("wtf");
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

  public EquippedTraitsStringNames initialEquippedTraits;
  public TraitSlotToTraitDictionary equippedTraits;
  public CharacterStatToFloatDictionary stats;
	public CharacterAttackValueToIntDictionary attackModifiers;
	public List<CharacterMovementAbility> movementAbilities;
	public SuperTextMesh stm;

	public string characterName;
	public string characterClass;
	public Transform weaponParent;
	public CharacterData defaultData;

	[HideInInspector]
    [SerializeField]
	public string initialWeaponId;
	public CharacterAttack characterAttack;
	public Hitbox hitboxPrefab;

	[HideInInspector]
  [SerializeField]
	public string initialPassiveTrait1;
	[HideInInspector]
  [SerializeField]
	public string initialPassiveTrait2;

	[HideInInspector]
  [SerializeField]
	public string initialActiveTrait1;
	[HideInInspector]
  [SerializeField]
	public string initialActiveTrait2;

	public string equippedWeaponId;

	// how fast we move
	public float acceleration;
	// how fast we turn
	public Transform orientation;
	public Transform crosshair;

	public bool facingRight = true;

	public Color damageFlashColor;
	public float damageFlashSpeed = 0.05f;
	public DamageTypeToFloatDictionary damageTypeResistances;

	// Length of time between environmental damage procs.
	// can we take damage (including knockback/stun etc) right now?
	protected bool invulnerable = false;
	// can we move or attack right now?
	protected bool stunned = false;

	// are we presently attacking?
	public bool attacking = false;
	public Coroutine attackCoroutine;
	// are we in an animation/
	public bool animationPreventsMoving = false;
	public bool sticking = false;

	// animation object
	protected Animator animator;
	// currently "equipped" weapon. no notion of "equipping" yet but there will be eventually.
	protected Weapon weapon;

	// our physics object
	public CustomPhysicsController po;
	// movement desired by character
	protected Vector2 movementInput;

	// point in space we would like to face
	protected Vector3 orientTowards;
	protected TileLocation currentTileLocation;
	protected EnvironmentTileInfo currentTile;

    public FloorLayer currentFloor;

	public CharacterData defaultCharacterData;
	public FloorLayer? justCameFromFloor;
	public Vector3 previousTilePosition;
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
		if (animator == null) {
			Debug.LogError("No animator on Character object: "+gameObject.name);
		}
		weapon = GetComponentInChildren<Weapon>();
		if ((initialWeaponId != null && initialWeaponId != "") || weapon != null) {
			InitializeWeapon(initialWeaponId);
		}
		// stm = transform.GetComponentInChildren<SuperTextMesh>();
		// if (stm == null) {
		// 	Debug.LogError("No SuperTextMesh component on any of Character's children: "+gameObject.name);
		// }
		if (po == null) {
			Debug.LogError("No physics controller component on Character object: "+gameObject.name);
		}
        ChangeLayersRecursively(transform, currentFloor.ToString());
	}

	protected virtual void Init() {
    Debug.Log("InitializeFromCharacterData!!");
		InitializeFromCharacterData();
	}

	private void InitializeFromCharacterData() {
		if (defaultCharacterData != null) {
			CharacterData dataInstance = (CharacterData) ScriptableObject.Instantiate(defaultCharacterData);
			stats = dataInstance.defaultStats;
			ResetStats();
      attackModifiers = dataInstance.attackModifiers;
			damageTypeResistances = dataInstance.damageTypeResistances;
			movementAbilities.AddRange(dataInstance.movementAbilities);
		}
	}

	private void ResetStats() {
		stats[CharacterStat.CurrentMaxHealth] = stats[CharacterStat.MaxHealth];
		stats[CharacterStat.CurrentHealth] = stats[CharacterStat.MaxHealth];

	}
	// non-physics biz
	protected virtual void Update() {
		HandleHealth();
		HandleFacingDirection();
		HandleTile();
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
	public float CalculateAcceleration() {
		float environmentAccelerationMod = currentTile != null ? currentTile.GetAccelerationMod() : 0;
		if (movementAbilities.Contains(CharacterMovementAbility.FastFeet)) {
			environmentAccelerationMod = Mathf.Max(environmentAccelerationMod, 0);
		}
		return stats[CharacterStat.MoveAcceleration] + environmentAccelerationMod;
	}

	public void ApplyAttackModifier(CharacterAttackValue attackValue, int magnitude) {
		attackModifiers[attackValue] += magnitude;
	}
// called via play input or npc AI
	protected void OLD__Attack() {
		if (weapon != null) {
			if (!attacking) {
				weapon.BeginAttack();
			}
			else {
				weapon.QueueNextAttack();
			}
		}
	}
	// ANIMATION HOOKS
	// These are functions called by animations, and are often passthroughs
	// to components on child objects.
	// They are either called from animation events or from animation state behaviors.
	public void OLD__CreateHitbox(string hitboxId) {
		weapon.CreateHitbox(hitboxId);
	}

	// Attack hook called via animation behavior. Exists on idle animation.
	public void OnIdleStart() {
		if (weapon != null) {
			weapon.FinishCombo();
			po.SetAnimationInputX(0f);
			po.SetAnimationInputY(0f);
		}
	}

	// Attack hook called via animation behavior. Should exist on every attack's FIRST animation.
	public void OnAttackStart() {
		weapon.OnEnterAttackAnimation();
	}

	// Attack hook called via animation behavior. Should exist on every attack's LAST animation.
	public void OnAttackEnd() {
		weapon.FinishAttack();
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
			(attacking || animationPreventsMoving)
			&& !movementAbilities.Contains(CharacterMovementAbility.Halteres)
		) {
			return;
		}
		Quaternion targetDirection = GetTargetDirection();
		orientation.rotation = Quaternion.Slerp(orientation.rotation, targetDirection, stats[CharacterStat.RotationSpeed] * Time.deltaTime);
		facingRight = true;
		if (orientation.eulerAngles.z > 90 || orientation.eulerAngles.z < 270) {
			facingRight = false;
		}
		// GetComponent<SpriteRenderer>().flipX = !facingRight;
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
			po.SetMovementInput(new Vector2(movementInput.x, movementInput.y));
		}
		else {
			po.SetMovementInput(Vector2.zero);
		}
	}

	// determines if input-based movement is allowed
	protected virtual bool CanMove() {
		if (!movementAbilities.Contains(CharacterMovementAbility.Halteres)) {
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
		if (invulnerable && !damageObj.ignoreInvulnerability) { return; }
		float damageAfterResistances = (1 - damageTypeResistances[damageObj.damageType]) * damageObj.damage;
		if (damageAfterResistances <= 0 && damageObj.characterStatModifications.Count == 0) { return; }
		if (damageObj.attackerTransform == transform) { return; }
		InterruptAnimation();
		AdjustHealth(Mathf.Floor(-damageAfterResistances));
		CalculateAndApplyStun(damageObj.stun);
		foreach(CharacterStatModification mod in damageObj.characterStatModifications) {
			ModCharacterStat(mod);
		}
		StartCoroutine(ApplyInvulnerability(damageObj.invulnerabilityWindow));
		StartCoroutine(ApplyDamageFlash(damageObj));
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

	public virtual void AssignTraitsForNextLife(UpcomingLifeTraits nextLifeTraits) {

	}

	// Make us invulnerable, then un-make-us invulnerable. Damage is ignorned while invulnerable.
	void CalculateAndApplyInvulnerability(float invulnerabilityDuration) {
		if (invulnerabilityDuration > 0) {
			StartCoroutine(ApplyInvulnerability(invulnerabilityDuration));
		}
	}

	IEnumerator ApplyInvulnerability(float duration) {
		invulnerable = true;
		yield return new WaitForSeconds(duration);
		invulnerable = false;
	}

	IEnumerator ApplyDamageFlash(DamageObject damageObj) {
		// SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		// Color baseColor = renderer.color;
		// for (int i = 0; i < 1 + (damageObj.damage / 3); i++) {
		// 	renderer.color = damageFlashColor;
			yield return new WaitForSeconds(damageFlashSpeed/2);
		// 	renderer.color = baseColor;
		// 	yield return new WaitForSeconds(damageFlashSpeed/2);
		// }
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
		if (weapon != null) {
			weapon.FinishAttack();
		}
		if (animator != null) {
			animator.SetTrigger("transitionToIdle");
		}
	}
	// END DAMAGE FUNCTIONS

	public void ModCharacterStat(CharacterStatModification mod) {
		StartCoroutine(ModCharacterStatCoroutine(mod));
	}

	public IEnumerator ModCharacterStatCoroutine(CharacterStatModification mod) {
		yield return new WaitForSeconds(mod.delay);
		if (mod.applicationDuration > 0) {
			float t = 0;
			while (t < mod.applicationDuration) {
				t += Time.deltaTime;
				AdjustStat(mod.statToModify, mod.magnitude * Time.deltaTime / mod.applicationDuration);
				yield return null;
			}
			// HACK: this line is probably going to result in some weird edge cases around stat mods.
			// If health is flickering or odd cases come up when multiple duration effects happen at once,
			// this may be the culprit!
			stats[mod.statToModify] = Mathf.Round(stats[mod.statToModify]);

		} else {
			AdjustStat(mod.statToModify, mod.magnitude);
		}
		if (mod.duration > 0) {
			yield return new WaitForSeconds(mod.duration);
			AdjustStat(mod.statToModify, -mod.magnitude);
		}
	}

	public void AddMovementAbility(CharacterMovementAbility movementAbility) {
		movementAbilities.Add(movementAbility);
	}

	public void RemoveMovementAbility(CharacterMovementAbility movementAbility) {
		movementAbilities.Remove(movementAbility);
	}

	public void AdjustStat(CharacterStat statToAdjust, float magnitude) {
		switch (statToAdjust) {
			case CharacterStat.CurrentHealth:
				AdjustHealth(magnitude);
				break;
			case CharacterStat.CurrentMaxHealth:
				AdjustCurrentMaxHealth(magnitude);
				break;
			case CharacterStat.MoveAcceleration:
				AdjustMoveAcceleration(magnitude);
				break;
		}
	}
	public void AdjustHealth(float adjustment) {
		stats[CharacterStat.CurrentHealth] = Mathf.Clamp(stats[CharacterStat.CurrentHealth] + adjustment, 0, stats[CharacterStat.CurrentMaxHealth]);
	}

	public void AdjustCurrentMaxHealth(float adjustment) {
		stats[CharacterStat.CurrentMaxHealth] = Mathf.Max(stats[CharacterStat.CurrentMaxHealth] + adjustment, 0);
		stats[CharacterStat.CurrentHealth] = Mathf.Min(stats[CharacterStat.CurrentHealth], stats[CharacterStat.CurrentMaxHealth]);
	}

	public void AdjustMoveAcceleration(float adjustment) {
		float actualAdjust = adjustment;
		if (movementAbilities.Contains(CharacterMovementAbility.FastFeet)) {
			actualAdjust = Mathf.Max(adjustment, 0);
		}
		stats[CharacterStat.MoveAcceleration] = Mathf.Max(stats[CharacterStat.MoveAcceleration] + actualAdjust, 0);
	}

	public void AdjustRotationSpeed(float adjustment) {
		stats[CharacterStat.RotationSpeed] = Mathf.Max(stats[CharacterStat.RotationSpeed] + adjustment, 0);
	}

	public void InitializeWeapon(string weaponId) {
		InitializeWeapon(weaponId, false);
	}
	public void InitializeWeapon(string weaponId, bool overrideExistingWeapon) {
		if (weapon == null || overrideExistingWeapon) {
			weapon = Instantiate(Resources.Load("Prefabs/Items/Weapons/"+weaponId) as GameObject, weaponParent, false).GetComponent<Weapon>();
		}
		equippedWeaponId = weaponId;
		weapon.animator = animator;
		weapon.owner = this;
		weapon.Init();
	}

    public virtual void SetCurrentFloor(FloorLayer newFloorLayer)
    {
		justCameFromFloor = currentFloor;
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
		if (stats[CharacterStat.CurrentHealth] <= 0) {
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
			justCameFromFloor = null;
		}
		if (stats[CharacterStat.CurrentEnvironmentalDamageCooldown] > 0) {
			stats[CharacterStat.CurrentEnvironmentalDamageCooldown] -= Time.deltaTime;
		}
		if (tile.objectTileType == null && tile.groundTileType == null) {
			if (
				(movementAbilities.Contains(CharacterMovementAbility.StickyFeet)
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
		if (tile.dealsDamage && tile.environmentalDamage != null && stats[CharacterStat.CurrentEnvironmentalDamageCooldown] <= 0) {
			TakeDamage(tile.environmentalDamage);
			stats[CharacterStat.CurrentEnvironmentalDamageCooldown] = stats[CharacterStat.MaxEnvironmentalDamageCooldown];
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
}
