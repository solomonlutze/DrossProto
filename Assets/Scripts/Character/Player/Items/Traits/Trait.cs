using System;
using UnityEngine;

public enum TraitName {}; // just a list of names of every possible trait
public enum TraitType {Passive, Active};

public enum TraitSlot {Head, Thorax, Abdomen, Legs, Wings}
[System.Serializable]
public enum TraitEffectType {
  Resistance,
  AnimationInput,
  CharacterStat,
  CharacterAttack,
  CharacterMovementAbility,
  Aura,
  SpawnsObject
}

public enum ConditionallyActivatedTraitCondition { None, NotMoving };
[System.Serializable]
public class TraitEffect {
  public TraitEffectType effectType;
  public int magnitude;
  public DamageType damageType;
  public CharacterStat stat;
	public CharacterAttackValue attackValue;
	public CharacterMovementAbility movementAbility;
  public Vector2 animationInput;
  public bool blocksMovement;
	public GameObject auraPrefab;
  public ConditionallyActivatedTraitCondition activatingCondition;
  public float activatingConditionRequiredDuration;

  private string _sourceString = "";
  public string sourceString {
    get {
      if (_sourceString == "") {
        _sourceString = Guid.NewGuid().ToString("N").Substring(0, 15);
      }
      return _sourceString;
    }
  }

  public void Apply(Character owner) {
    Debug.Log("contains this (pre-add): " + owner.activeConditionallyActivatedTraitEffects.Contains(this));
    if (activatingCondition != ConditionallyActivatedTraitCondition.None) {
      if (owner.activeConditionallyActivatedTraitEffects.Contains(this)) {
        Debug.Log("Already applied trait effect. returning!");
        return;
      } else {
        owner.activeConditionallyActivatedTraitEffects.Add(this);
      }
    }
    switch (effectType)
		{
			case TraitEffectType.Resistance:
				owner.damageTypeResistances[damageType] += magnitude;
				break;
			case TraitEffectType.AnimationInput:
				owner.SetAnimationInput(animationInput);
				if (blocksMovement) {
					owner.SetAnimationPreventsMoving(blocksMovement);
				}
				break;
			case TraitEffectType.CharacterMovementAbility:
				owner.AddMovementAbility(movementAbility);
				break;
			case TraitEffectType.CharacterAttack:
				owner.ApplyAttackModifier(attackValue, magnitude);
				break;
      case TraitEffectType.CharacterStat:
				owner.AddStatMod(stat, magnitude, sourceString);
        break;
      case TraitEffectType.Aura:
        Debug.Log("addingStatMod - Aura");
        owner.AddAura(this);
        break;
		}
	}

	public void Expire(Character owner) {
    switch (effectType)
		{
			case TraitEffectType.Resistance:
				owner.damageTypeResistances[damageType] -= magnitude;
				break;
			case TraitEffectType.AnimationInput:
				owner.SetAnimationInput(Vector2.zero);
				owner.SetAnimationPreventsMoving(false);
				break;
			case TraitEffectType.CharacterMovementAbility:
				owner.RemoveMovementAbility(movementAbility);
				break;
      case TraitEffectType.CharacterStat:
				owner.RemoveStatMod(sourceString);
        break;
      case TraitEffectType.Aura:
        owner.RemoveAura(this);
        break;
		}
    if (activatingCondition != ConditionallyActivatedTraitCondition.None
      && owner.activeConditionallyActivatedTraitEffects.Contains(this)
    ) {
      Debug.Log("removing condiitonally active trait effect");
      owner.activeConditionallyActivatedTraitEffects.Remove(this);
    }
	}
}

public abstract class Trait : ScriptableObject {
  TraitName tn; // do we use this?
  public string traitName;
	public TraitEffect[] passiveTraitEffects;

  abstract public TraitType traitType { get; set; }
		// Called when the trait is applied to the player (usually on spawn)
	public void OnTraitAdded (Character owner) {
		foreach (TraitEffect traitEffect in passiveTraitEffects) {
      if (traitEffect.activatingCondition != ConditionallyActivatedTraitCondition.None) {
        owner.conditionallyActivatedTraitEffects.Add(traitEffect);
        Debug.Log("added "+traitEffect+ " to conditionally activated trait effects");
        continue;
      }
			traitEffect.Apply(owner);
		}
	}

	// Called when the trait is removed from the character (usually on death)
	public void OnTraitRemoved (Character owner) {
		foreach (TraitEffect traitEffect in passiveTraitEffects) {
			traitEffect.Expire(owner);
		}
	}
}

// public abstract class TraitMono : MonoBehaviour {
//  TraitName traitName;
//  abstract public TraitType traitType { get; set; }
//  //  other?
// }

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