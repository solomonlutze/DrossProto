using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TraitName {}; // just a list of names of every possible trait
public enum TraitType {Passive, Active};

[System.Serializable]
public enum TraitEffectType { Resistance, TileHeightLevel, AnimationInput, CharacterStat, CharacterMovementAbility }
[System.Serializable]
public class TraitEffect {
  public TraitEffectType effectType;
  public float magnitude;
  public DamageType damageType;
  public CharacterStat stat;
	public CharacterMovementAbility movementAbility;
  public TileHeight tileHeightLevel;
  public Vector2 animationInput;
  public bool blocksMovement;
	public GameObject objectToSpawn;
}

public abstract class Trait : ScriptableObject {
  TraitName tn; // do we use this?
  public string traitName;
	public TraitEffect[] passiveTraitEffects;

  abstract public TraitType traitType { get; set; }
		// Called when the trait is applied to the player (usually on spawn)
	public void OnTraitAdded (Character owner) {
		foreach (TraitEffect traitEffect in passiveTraitEffects) {
			ApplyTraitEffect(owner, traitEffect);
		}
	}

	// Called when the trait is removed from the character (usually on death)
	public void OnTraitRemoved (Character owner) {
		foreach (TraitEffect traitEffect in passiveTraitEffects) {
			ExpireTraitEffect(owner, traitEffect);
		}
	}
	public void ApplyTraitEffect(Character owner, TraitEffect traitEffect) {
		Debug.Log("applying trait effect: "+traitEffect.effectType);
		switch (traitEffect.effectType)
		{
			case TraitEffectType.Resistance:
				owner.damageTypeResistances[traitEffect.damageType] += traitEffect.magnitude;
				break;
			case TraitEffectType.AnimationInput:
				owner.SetAnimationInput(traitEffect.animationInput);
				if (traitEffect.blocksMovement) {
					owner.SetAnimationPreventsMoving(traitEffect.blocksMovement);
				}
				break;
			case TraitEffectType.CharacterMovementAbility:
				owner.AddMovementAbility(traitEffect.movementAbility);
				break;
			case TraitEffectType.TileHeightLevel:
				owner.currentTileHeightLevel = traitEffect.tileHeightLevel;
				break;
      case TraitEffectType.CharacterStat:
        Debug.LogError("character stats mods aren't implemented yet you dork");
        break;
		}
	}

	public void ExpireTraitEffect(Character owner, TraitEffect traitEffect) {
		switch (traitEffect.effectType)
		{
			case TraitEffectType.Resistance:
				owner.damageTypeResistances[traitEffect.damageType] -= traitEffect.magnitude;
				break;
			case TraitEffectType.AnimationInput:
				owner.SetAnimationInput(Vector2.zero);
				owner.SetAnimationPreventsMoving(false);
				break;
			case TraitEffectType.CharacterMovementAbility:
				owner.AddMovementAbility(traitEffect.movementAbility);
				break;
			case TraitEffectType.TileHeightLevel:
				owner.currentTileHeightLevel = owner.defaultTileHeightLevel;
				break;
      case TraitEffectType.CharacterStat:
        Debug.LogError("character stats mods aren't implemented yet you dork");
        break;
		}
	}
}

public abstract class TraitMono : MonoBehaviour {
 TraitName traitName;
 abstract public TraitType traitType { get; set; }
 //  other?
}

public class UpcomingLifeTrait {
  public string traitName;
  public InventoryEntry inventoryItem;

  public UpcomingLifeTrait(string t, InventoryEntry ie) {
    traitName = t;
    inventoryItem = ie;
  }
}

public class UpcomingLifeTraits {

  public UpcomingLifeTrait[] passiveTraits; // traits that are always active
  public UpcomingLifeTrait[] activeTraits; // traits that require activation

  public UpcomingLifeTraits(int numPassiveTraits, int numActiveTraits) {
    activeTraits = new UpcomingLifeTrait[numPassiveTraits];
    passiveTraits = new UpcomingLifeTrait[numActiveTraits];
  }
}