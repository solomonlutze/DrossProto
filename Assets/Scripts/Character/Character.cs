using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// TODO: Character can probably extend CustomPhysicsController, which simplifies movement code a bit.
public class AnimationInfoObject {
	public Vector2 animationInput;
}

public enum CharacterStat {
	CurrentHealth,
	CurrentMaxHealth,
	MaxHealth,
	CurrentEnvironmentalDamageCooldown,
	CurrentMaxEnvironmentalDamageCooldown,
	MaxEnvironmentalDamageCooldown
}

// Special modes of character movement.
// Possibly unnecessary!!
public enum CharacterMovementAbility {
	WaterStride,
	Burrow
}

[System.Serializable]
public class CharacterStatModification {
    public CharacterStat statToModify;
    public float magnitude;
    public float duration;
    public float delay;
}

// TODO: Character should probably extend CustomPhysicsController, which should extend WorldObject
public class Character : WorldObject {

	public CharacterStatToFloatDictionary stats;
	public List<CharacterMovementAbility> movementAbilities;
	public SuperTextMesh stm;

	public string characterName;
	public string characterClass;
	public Transform weaponParent;
	public CharacterData defaultData;

	[HideInInspector]
    [SerializeField]
	public string initialWeaponId;

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
	public float rotationSpeed = 5;
	public float defaultRotationSpeed = 5;
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
	// are we in an animation/
	public bool animationPreventsMoving = false;

	// animation object
	protected Animator animator;
	// currently "equipped" weapon. no notion of "equipping" yet but there will be eventually.
	protected Weapon weapon;

	// our physics object
	protected CustomPhysicsController po;
	// movement desired by character
	protected Vector2 movementInput;

	// point in space we would like to face
	protected Vector3 orientTowards;

	// We will interact with tiles at the same tile height or above
	// e.g. if our currentTileHeightLevel is Tall we will interact with Tall/Unpassable tiles but not Ground or Short tiles
	public TileHeight defaultTileHeightLevel = TileHeight.Ground;
	public TileHeight currentTileHeightLevel;
	protected TileLocation currentTileLocation;

    public Constants.FloorLayer currentFloor;

	public float maxTileStepHeight = 1.0f;
	public CharacterData defaultCharacterData;
	public Constants.FloorLayer? justCameFromFloor;
	public Vector3 previousTilePosition;
	protected virtual void Awake() {
		orientation = transform.Find("Orientation");
		if (orientation == null) {
			Debug.LogError("No object named 'Orientation' on Character object: "+gameObject.name);
		}
	}

	// TODO: this long list of GetComponents is messy; can it be cleaned up?
	protected virtual void Start () {
		InitializeFromCharacterData();
		movementInput = new Vector2(0,0);
		po = GetComponent<CustomPhysicsController>();
		animator = GetComponent<Animator>();
		if (animator == null) {
			Debug.LogError("No animator on Character object: "+gameObject.name);
		}
		weapon = GetComponentInChildren<Weapon>();
		if ((initialWeaponId != null && initialWeaponId != "") || weapon != null) {
			InitializeWeapon(initialWeaponId);
		}
		stm = transform.GetComponentInChildren<SuperTextMesh>();
		if (stm == null) {
			Debug.LogError("No SuperTextMesh component on any of Character's children: "+gameObject.name);
		}
		if (po == null) {
			Debug.LogError("No physics controller component on Character object: "+gameObject.name);
		}
        ChangeLayersRecursively(transform, currentFloor.ToString());
	}

	private void InitializeFromCharacterData() {
		if (defaultCharacterData != null) {
			stats = defaultCharacterData.defaultStats;
			ResetStats();
			damageTypeResistances = defaultCharacterData.damageTypeResistances;
			movementAbilities.AddRange(defaultCharacterData.movementAbilities);
			Debug.Log("movementAbilities: "+movementAbilities);
		}
	}

	private void ResetStats() {
		stats[CharacterStat.CurrentMaxHealth] = stats[CharacterStat.MaxHealth];
		stats[CharacterStat.CurrentHealth] = stats[CharacterStat.MaxHealth];

	}
	// non-physics biz
	protected virtual void Update() {
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
	public void CreateHitbox(string hitboxId) {
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

	public void SetRotationSpeed(float newRotationSpeed) {
		rotationSpeed = newRotationSpeed;
	}

	public void SetAnimationPreventsMoving(bool newAnimationPreventsMoving) {
		animationPreventsMoving = newAnimationPreventsMoving;
	}
	// Point character towards a rotation target.
	void HandleFacingDirection() {
		if (attacking || animationPreventsMoving) {
			return;
		}
		Quaternion targetDirection = GetTargetDirection();
		orientation.rotation = Quaternion.Slerp(orientation.rotation, targetDirection, rotationSpeed * Time.deltaTime);
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
		if (attacking) {
			return false;
		}
		if (animationPreventsMoving) {
			return false;
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
		if (damageAfterResistances <= 0) { return; }
		if (damageObj.attackerTransform == transform) { return; }
		InterruptAnimation();
		AdjustHealth(Mathf.Floor(-damageAfterResistances));
		if (stats[CharacterStat.CurrentHealth] <= 0) {
			Die();
			return;
		}
		CalculateAndApplyStun(damageObj.stun);
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
	void CalculateAndApplyStun(float stunDuration) {
		// TODO: Stun resistance/reduction should happen here
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
		if (mod.duration > 0) {
			float t = 0;
			while (t < mod.duration) {
				t += Time.deltaTime;
				AdjustStat(mod.statToModify, mod.magnitude * Time.deltaTime / mod.duration);
				yield return null;
			}
			// HACK: this line is probably going to result in some weird edge cases around stat mods.
			// If health is flickering or odd cases come up when multiple duration effects happen at once,
			// this may be the culprit!
			stats[mod.statToModify] = Mathf.Round(stats[mod.statToModify]);

		} else {
			AdjustStat(mod.statToModify, mod.magnitude);
		}
	}

	public void AddMovementAbility(CharacterMovementAbility movementAbility) {
		movementAbilities.Add(movementAbility);
		Debug.Log("adding movement ability: "+movementAbility);
		Debug.Log("movementAbilities: "+movementAbilities);
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
		}
	}
	public void AdjustHealth(float adjustment) {
		stats[CharacterStat.CurrentHealth] = Mathf.Clamp(stats[CharacterStat.CurrentHealth] + adjustment, 0, stats[CharacterStat.CurrentMaxHealth]);
	}

	public void AdjustCurrentMaxHealth(float adjustment) {
		stats[CharacterStat.CurrentMaxHealth] = Mathf.Max(stats[CharacterStat.CurrentMaxHealth] + adjustment, 0);
		stats[CharacterStat.CurrentHealth] = Mathf.Min(stats[CharacterStat.CurrentHealth], stats[CharacterStat.CurrentMaxHealth]);
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

    public void SetCurrentFloor(Constants.FloorLayer newFloorLayer)
    {
		justCameFromFloor = currentFloor;
        currentFloor = newFloorLayer;
		currentTileLocation = CalculateCurrentTileLocation();
		ChangeLayersRecursively(transform, newFloorLayer.ToString());
		po.OnLayerChange();
    }

	protected void UseTile() {
		Debug.Log("usetile - currentTileLocation floor: "+currentTileLocation.floorLayer);
		EnvironmentTile et = GameMaster.Instance.GetTileAtLocation(currentTileLocation.position, currentTileLocation.floorLayer);
		if (et.changesFloorLayer) {
			SetCurrentFloor(et.targetFloorLayer);
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

	public TileLocation CalculateCurrentTileLocation() {
		return new TileLocation(
			new Vector3(
				Mathf.FloorToInt(transform.position.x) + .5f,
				Mathf.FloorToInt(transform.position.y) + .5f,
				Mathf.FloorToInt(transform.position.z)
			),
			currentFloor
		);
	}

	protected virtual void HandleTile() {
		EnvironmentTile tile = GameMaster.Instance.GetTileAtLocation(transform.position, currentFloor);
		TileLocation nowTileLocation = CalculateCurrentTileLocation();
		if (currentTileLocation != nowTileLocation) {
			currentTileLocation = nowTileLocation;
			justCameFromFloor = null;
		}
		if (tile.dealsDamage && tile.environmentalDamage != null && stats[CharacterStat.CurrentEnvironmentalDamageCooldown] <= 0) {
			TakeDamage(tile.environmentalDamage);
			stats[CharacterStat.CurrentEnvironmentalDamageCooldown] = stats[CharacterStat.MaxEnvironmentalDamageCooldown];
		}
		if (stats[CharacterStat.CurrentEnvironmentalDamageCooldown] > 0) {
			stats[CharacterStat.CurrentEnvironmentalDamageCooldown] -= Time.deltaTime;
		}
	}

	public virtual void HandleTileCollision(EnvironmentTile tile, Vector3 loc, Constants.FloorLayer floor) {
		if (tile.colliderType == Tile.ColliderType.None) {
			return;
		}
		else {
			Debug.Log("collided with "+tile);
		}
	}
}
