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

  CharacterPerceptionAbility,
  Aura,
  SpawnsObject
}

public enum ConditionallyActivatedTraitCondition { None, NotMoving, Moving };
[System.Serializable]
public class TraitEffect {
  public TraitEffectType effectType;
  public int magnitude;
  public DamageType damageType;
  public CharacterStat stat;
	public CharacterAttackValue attackValue;
	public CharacterMovementAbility movementAbility;
	public CharacterPerceptionAbility perceptionAbility;
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
    if (activatingCondition != ConditionallyActivatedTraitCondition.None) {
      if (owner.activeConditionallyActivatedTraitEffects.Contains(this)) {
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
      case TraitEffectType.CharacterPerceptionAbility:
        Debug.Log("perception ability");
				switch (perceptionAbility) {
          case CharacterPerceptionAbility.SensitiveAntennae:
            Debug.Log("SensitiveAntennae");
            GameObject[] scentGameObjects = GameObject.FindGameObjectsWithTag("EnemyScent");
            foreach(GameObject obj in scentGameObjects) {
              ParticleSystem ps = obj.GetComponent<ParticleSystem>();
              if (ps != null) {
                Debug.Log("setting emission active");
                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = true;
              }
            }
            break;
        }
				break;
			case TraitEffectType.CharacterAttack:
				owner.ApplyAttackModifier(attackValue, magnitude);
				break;
      case TraitEffectType.CharacterStat:
				owner.AddStatMod(stat, magnitude, sourceString);
        break;
      case TraitEffectType.Aura:
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
			case TraitEffectType.CharacterPerceptionAbility:
				Debug.Log("perception ability");
				switch (perceptionAbility) {
          case CharacterPerceptionAbility.SensitiveAntennae:
            Debug.Log("SensitiveAntennae");
            GameObject[] scentGameObjects = GameObject.FindGameObjectsWithTag("EnemyScent");
            foreach(GameObject obj in scentGameObjects) {
              ParticleSystem ps = obj.GetComponent<ParticleSystem>();
              if (ps != null) {
                Debug.Log("setting emission inactive");
                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = false;
              }
            }
            break;
        }
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

[System.Serializable]
public class UpcomingLifeTrait {
  public Trait trait;
  public InventoryEntry inventoryItem;
  public LymphType lymphType;

  public UpcomingLifeTrait(Trait t, LymphType lt, InventoryEntry ie) {
    trait = t;
    lymphType = lt;
    inventoryItem = ie;
  }
}

[System.Serializable]
public class LymphTypeSkills {
  [StringInList(typeof(PropertyDrawerHelpers), "AllActiveTraitNames", new object[]{false})]
  public string primarySkill;

  [StringInList(typeof(PropertyDrawerHelpers), "AllActiveTraitNames", new object[]{false})]
  public string secondarySkill;

  public ActiveTrait GetPrimarySkill() {
    return Resources.Load("Data/TraitData/ActiveTraits/"+primarySkill) as ActiveTrait;
  }
  public ActiveTrait GetSecondarySkill() {
    return Resources.Load("Data/TraitData/ActiveTraits/"+secondarySkill) as ActiveTrait;
  }
  // public TraitSlotToTraitDictionary EquippedTraits() {
  //   TraitSlotToTraitDictionary d = new TraitSlotToTraitDictionary();
  //   d[TraitSlot.Head] = Resources.Load("Data/TraitData/PassiveTraits/"+head) as PassiveTrait;
  //   d[TraitSlot.Thorax] = Resources.Load("Data/TraitData/PassiveTraits/"+thorax) as PassiveTrait;
  //   d[TraitSlot.Abdomen] = Resources.Load("Data/TraitData/PassiveTraits/"+abdomen) as PassiveTrait;
  //   d[TraitSlot.Legs] = Resources.Load("Data/TraitData/PassiveTraits/"+legs) as PassiveTrait;
  //   d[TraitSlot.Wings] = Resources.Load("Data/TraitData/PassiveTraits/"+wings) as PassiveTrait;
  // return d;
  // }
}
// [System.Serializable]
// public class UpcomingLifeTraits {

//   public UpcomingLifeTrait[] passiveTraits; // traits that are always active
//   public UpcomingLifeTrait[] activeTraits; // traits that require activation

//   public UpcomingLifeTraits(int numPassiveTraits, int numActiveTraits) {
//     activeTraits = new UpcomingLifeTrait[numPassiveTraits];
//     passiveTraits = new UpcomingLifeTrait[numActiveTraits];
//   }
// }