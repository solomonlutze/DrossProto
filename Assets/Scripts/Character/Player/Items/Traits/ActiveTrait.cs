using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO: AI characters need crosshairs too!!
public enum SpawnedObjectSpawnLocation { Owner, Crosshair }

//TODO: For now, only spawns hitboxes. We should make "things a gameobject spawns" more generic.
[System.Serializable]
public class TraitSpawnedObject {
	public Hitbox hitboxPrefab;
	public HitboxInfo hitboxInfo;
	public SpawnedObjectSpawnLocation spawnLocation;
	public bool followOwner;
	public bool expiresWithTraitEffect;
}

// Indicates trait modifies an environment tile when activated
[System.Serializable]
public class TraitChangedEnvironmentTile {
	public EnvironmentTile tileToSpawn;
	public List<TileTag> validTileTagsToSpawnOn;
	public FloorTilemapType targetFloorTilemapType;
	public TilemapDirection spawnDirection;
}

[System.Serializable]
public class ActiveTraitEffect {
	public float duration;
	public float delay;
	public float applicationDuration;
	public bool cancelOnButtonRelease;
	public TraitEffect[] traitEffects;
	public TraitSpawnedObject[] objectsToSpawn;
	public TraitChangedEnvironmentTile[] changedEnvironmentTiles;
}


// effect timings:
// when the button is pressed, timeSinceStart begins counting
// when it is released, it stops counting (unless cancelOnButtonRelease is false, in which case uh, do something)
// we have a currentEffectIndex defaulted to 0; when the button is pressed we call
public class ActiveTrait : Trait {
	public float cooldown;
	public ActiveTraitEffect[] activeTraitEffects;

	private float timeSincePressed;

	public override TraitType traitType {
		get { return TraitType.Active; }
		set { }
	}

	#if UNITY_EDITOR
    // The following is a helper that adds a menu item to create an TraitItem Asset
        [MenuItem("Assets/Create/Trait/ActiveTrait")]
        public static void CreateActiveTrait()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Active Trait", "New Active Trait", "Asset", "Save Active Trait", "Assets/resources/Data/TraitData/ActiveTraits");
            if (path == "")
                return;
       		AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ActiveTrait>(), path);
        }
		// ensure traitEffects are ordered by delay
		public void OnValidate() {
			System.Array.Sort(activeTraitEffects, delegate(ActiveTraitEffect traitEffect1, ActiveTraitEffect traitEffect2) {
				return traitEffect1.delay.CompareTo(traitEffect2.delay);
			});
		}
    #endif
}

// effect timings:
// when the button is pressed, timeSinceStart begins counting
// when it is released, it stops counting (unless cancelOnButtonRelease is false, in which case uh, do something)
// we have a currentEffectIndex defaulted to 0; when the button is pressed we call
public class ActiveTraitInstance : MonoBehaviour {
	private ActiveTrait traitData;
	private Dictionary<int, Coroutine> delayEffectCoroutines;
	private Coroutine cooldownCoroutine;
	private bool abilityReady = true;
	private bool abilityInUse = false;
	private float timeSincePressed;
	private int currentEffectIndex;
	private Character owner;
	private List<GameObject> instancedGameObjects;


	// Called when the trait is activated. Surprise!
	// start a coroutine for each active effect
	// then when it procs, apply the effects listed
	public void Init(Character c, ActiveTrait td) {
		instancedGameObjects = new List<GameObject>();
		owner = c;
		traitData = td;
		delayEffectCoroutines = new Dictionary<int, Coroutine>();
		traitData.OnTraitAdded(c);
	}

	public void OnActivateTraitPressed () {
		if (abilityReady) {
			PlayerController pc = (PlayerController) owner;
			if (pc != null) { pc.lastActivatedTrait =  traitData.traitName;	}
			Debug.Log("pc.lastActivatedTrait: "+pc.lastActivatedTrait);
			abilityInUse = true;
			if (traitData.cooldown > 0) {
				abilityReady = false;
				Invoke("ApplyCooldown", traitData.cooldown);
			}
		}
	}

	// please call this from update or it will be nonsense
	public void WhileActiveTraitPressed() {
		if (!abilityInUse) { return; }
		if (currentEffectIndex < traitData.activeTraitEffects.Length) {
			ActiveTraitEffect nextActiveTraitEffect = traitData.activeTraitEffects[currentEffectIndex];
			if (nextActiveTraitEffect.delay <= timeSincePressed) {
				ApplyActiveTraitEffect(nextActiveTraitEffect);
				currentEffectIndex++;
			}
			timeSincePressed += Time.deltaTime;
		}
	}

	public void OnActivateTraitReleased () {
		for (int i = 0; i < currentEffectIndex; i++ ) {
			ActiveTraitEffect activeTraitEffect = traitData.activeTraitEffects[i];
			if (activeTraitEffect.cancelOnButtonRelease) {
				ExpireEffect(activeTraitEffect);
			}
		}
		// TODO: these should have something indicating whether or not this process should destroy them
		foreach (GameObject obj in instancedGameObjects) {
			Destroy(obj);
		}
		instancedGameObjects.Clear();
		timeSincePressed = 0;
		currentEffectIndex = 0;
		abilityInUse = false;
	}

	// called if an active trait effect is delayed
	private IEnumerator ApplyDelayedActiveTraitCoroutine(ActiveTraitEffect activeTraitEffect) {
		yield return new WaitForSeconds(activeTraitEffect.delay);
		ApplyActiveTraitEffect(activeTraitEffect);
	}

	private void ApplyActiveTraitEffect(ActiveTraitEffect activeTraitEffect) {
		foreach (TraitEffect traitEffect in activeTraitEffect.traitEffects) {
			Debug.Log("applying trait effect: input = "+traitEffect.animationInput);
			traitData.ApplyTraitEffect(owner, traitEffect);
		}

		foreach (TraitSpawnedObject traitSpawnedObject in activeTraitEffect.objectsToSpawn) {
			SpawnTraitSpawnedObject(traitSpawnedObject);
		}
		foreach(TraitChangedEnvironmentTile changedEnvironmentTile in activeTraitEffect.changedEnvironmentTiles) {
			ChangeEnvironmentTile(changedEnvironmentTile);
		}
	}

	// TODO: Right now, only instantiates hitboxes.
	// Make it more generic!!
	private void SpawnTraitSpawnedObject(TraitSpawnedObject traitSpawnedObject) {
		Transform spawnTransform;
		Vector3 spawnPosition;
		Quaternion spawnRotation;
		switch (traitSpawnedObject.spawnLocation) {
			case SpawnedObjectSpawnLocation.Crosshair:
				spawnTransform = owner.orientation.transform;
				spawnPosition = owner.crosshair.position;
				spawnRotation = owner.orientation.rotation;
				break;
			case SpawnedObjectSpawnLocation.Owner:
			default:
				spawnTransform = owner.transform;
				spawnPosition = spawnTransform.position;
				spawnRotation = spawnTransform.rotation;
				break;
		}
		Hitbox obj = Instantiate(traitSpawnedObject.hitboxPrefab, spawnPosition, spawnRotation);
		if (obj != null) {
			obj.Init(spawnTransform, owner, traitSpawnedObject.hitboxInfo);
			instancedGameObjects.Add(obj.gameObject);
		}
	}

	private void ChangeEnvironmentTile(TraitChangedEnvironmentTile changedEnvironmentTile) {
		if (changedEnvironmentTile == null) { return; }
		EnvironmentTile currentFloorTile = GameMaster.Instance.GetTileAtLocation(owner.transform.position, owner.currentFloor);
		GameMaster.Instance.ReplaceAdjacentTile(owner.transform.position, owner.currentFloor, changedEnvironmentTile.tileToSpawn, changedEnvironmentTile.spawnDirection);
	}

	private void ExpireEffect(ActiveTraitEffect activeTraitEffect) {
		PlayerController pc = (PlayerController) owner;
		if (pc.lastActivatedTrait == traitData.traitName) {
			// pc.lastActivatedTrait = null;
		}
		foreach (TraitEffect traitEffect in activeTraitEffect.traitEffects) {
			traitData.ExpireTraitEffect(owner, traitEffect);
		}
	}

	private void ApplyCooldown() {
		abilityReady = true;
	}
}
// public class ActiveTraitMono : TraitMono {

// 	public float duration = 5;
// 	public float cooldown = 10;
// 	public bool deactivatesOnButtonUp = false;
// 	protected Coroutine expireEffectCoroutine;
// 	protected Coroutine cooldownCoroutine;
// 	protected bool abilityReady = true;

// 	public override TraitType traitType {
// 		get { return TraitType.Active; }
// 		set { }
// 	}

// 	// Called when the trait is activated
// 	public virtual void OnTraitActivated (Character owner) {
// 		if (abilityReady) {
// 			abilityReady = false;
// 			ApplyEffect(owner);
// 			expireEffectCoroutine = StartCoroutine(ExpireEffectCoroutine(owner));
// 			cooldownCoroutine = StartCoroutine(ApplyCooldown());
// 		}
// 	}

// 	public virtual void WhileTraitActive(Character owner) {

// 	}
// 	public virtual void OnTraitDeactivated (Character owner) {
// 		if (deactivatesOnButtonUp) {
// 			Debug.LogWarning("deactivated from button up!!");
// 			StopCoroutine(expireEffectCoroutine);
// 			ExpireEffect(owner);
// 		}
// 	}

// 	// waits <cooldown> seconds, then clears the coroutine and resets abilityReady so OnTraitActivated will work
// 	protected virtual IEnumerator ApplyCooldown() {
// 		yield return new WaitForSeconds(cooldown);
// 		abilityReady = true;
// 		cooldownCoroutine = null;
// 	}

// 	protected virtual IEnumerator ExpireEffectCoroutine(Character owner) {
// 		yield return new WaitForSeconds(duration);
// 		ExpireEffect(owner);
// 	}

// 	protected virtual void ApplyEffect(Character owner) { }

// 	protected virtual void ExpireEffect(Character owner) {
// 		expireEffectCoroutine = null;
// 	}
// }
