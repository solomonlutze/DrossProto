using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum TraitName { }; // just a list of names of every possible trait
public enum TraitType { Passive, Active };

public enum TraitSlot { Head, Thorax, Abdomen, Legs, Wings }
[System.Serializable]
// P sure this whole class is deprecated
public enum TraitEffectType
{
  Resistance,
  AnimationInput,
  CharacterStat,
  CharacterAttack,
  CharacterMovementAbility,
  CharacterPerceptionAbility,
  Aura,
  SpawnsObject,
  CharacterAttackModifiers
}

public enum ConditionallyActivatedTraitCondition { None, NotMoving, Moving, Dashing };

// P sure this whole class is deprecated
[System.Serializable]
public class TraitEffect
{

  public TraitEffectType effectType;
  public int magnitude;
  public DamageType damageType;
  public CharacterStat stat;
  public CharacterAttackModifiers attackModifiers;
  // public int[] attackModifiers;
  public CharacterAttackValue attackValue;
  public CharacterAttackValue dashAttackValue;
  public CharacterMovementAbility movementAbility;
  public CharacterPerceptionAbility perceptionAbility;
  public Vector2 animationInput;
  public bool blocksMovement;
  public GameObject auraPrefab;
  public ConditionallyActivatedTraitCondition activatingCondition;
  public float activatingConditionRequiredDuration;
  public bool triggeredByDash = false;
  public bool attackCorrodes = false;
  private string _sourceString = "";
  public string sourceString
  {
    get
    {
      if (_sourceString == "")
      {
        _sourceString = Guid.NewGuid().ToString("N").Substring(0, 15);
      }
      return _sourceString;
    }
  }

  public void Apply(Character owner)
  {
    if (activatingCondition != ConditionallyActivatedTraitCondition.None)
    {
      if (owner.activeConditionallyActivatedTraitEffects.Contains(this))
      {
        return;
      }
      else
      {
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
        if (blocksMovement)
        {
          owner.SetAnimationPreventsMoving(blocksMovement);
        }
        break;
      case TraitEffectType.CharacterMovementAbility:
        owner.AddMovementAbility(movementAbility);
        break;
      case TraitEffectType.CharacterPerceptionAbility:
        switch (perceptionAbility)
        {
          case CharacterPerceptionAbility.SensitiveAntennae:
            GameObject[] scentGameObjects = GameObject.FindGameObjectsWithTag("EnemyScent");
            foreach (GameObject obj in scentGameObjects)
            {
              // ParticleSystem ps = obj.GetComponent<ParticleSystem>();
              // if (ps != null)
              // {
              //   Debug.Log("setting emission active");
              //   ParticleSystem.EmissionModule em = ps.emission;
              //   em.enabled = true;
              // }
            }
            break;
        }
        break;
      case TraitEffectType.CharacterAttackModifiers:
        if (triggeredByDash)
        {
          owner.ApplyAttackModifier(attackModifiers, true);
        }
        else
        {
          owner.ApplyAttackModifier(attackModifiers, false);
        }
        break;
    }
  }

  public void Expire(Character owner)
  {
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
        switch (perceptionAbility)
        {
          case CharacterPerceptionAbility.SensitiveAntennae:
            GameObject[] scentGameObjects = GameObject.FindGameObjectsWithTag("EnemyScent");
            foreach (GameObject obj in scentGameObjects)
            {
              ParticleSystem ps = obj.GetComponent<ParticleSystem>();
              if (ps != null)
              {
                ParticleSystem.EmissionModule em = ps.emission;
                em.enabled = false;
              }
            }
            break;
        }
        break;
        // case TraitEffectType.CharacterStat:
        //     owner.RemoveStatMod(sourceString);
        //     break;
    }
    if (activatingCondition != ConditionallyActivatedTraitCondition.None
      && owner.activeConditionallyActivatedTraitEffects.Contains(this)
    )
    {
      owner.activeConditionallyActivatedTraitEffects.Remove(this);
    }
  }
}

// this is the actual trait
public class Trait : ScriptableObject
{
  public string traitName;
  public BugSpecies bugSpecies;

  [TextArea]
  public string traitDescription;
  [HideInInspector]
  public CharacterAttributeToIntDictionary attributeModifiers;
  [HideInInspector]
  public CharacterSkillData skillData_old; // Not deprecated yet but soon I think?
  public GameObject owningBugPrefab;
  public LymphType lymphType;
  public CharacterSkillData skill; // When this gets used depends on what body part this is equipped to!
  public Sprite visual1;

  public Sprite visual2;
  public Color primaryColor;
  public Color secondaryColor;
  public BugSkeletonImagesData imagesData;

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an Trait Asset
  [MenuItem("Assets/Create/Trait/Trait")]
  public static void CreateTrait()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Trait", "New Trait", "Asset", "Save Trait", "Assets/resources/Data/TraitData/Traits");
    if (path == "")
      return;
    Trait tAsset = ScriptableObject.CreateInstance<Trait>();
    tAsset.attributeModifiers = new CharacterAttributeToIntDictionary(false);
    AssetDatabase.CreateAsset(tAsset, path);
  }

  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/Trait/TraitSet")]
  public static void CreateTraitSet()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Trait", "New Trait", "Asset", "Save Trait", "Assets/resources/Data/TraitData/Traits");
    if (path == "")
      return;
    Debug.Log("path: " + path);
    string[] splitPath = path.Split('/');
    string objName = splitPath[splitPath.Length - 1];
    int indexOfPeriod = objName.IndexOf('.');
    objName = objName.Substring(0, indexOfPeriod);
    Debug.Log("objName: " + objName);
    foreach (TraitSlot slot in Enum.GetValues(typeof(TraitSlot)))
    {
      Trait tAsset = ScriptableObject.CreateInstance<Trait>();
      tAsset.attributeModifiers = new CharacterAttributeToIntDictionary(false);
      tAsset.name = objName + "_" + slot.ToString();
      splitPath[splitPath.Length - 1] = tAsset.name + ".asset";
      AssetDatabase.CreateAsset(tAsset, String.Join("/", splitPath));
    }
  }

  // public static void CreateTraitSetForBug(string bugName)
  // {
  //     foreach (TraitSlot slot in Enum.GetValues(typeof(TraitSlot)))
  //     {
  //         Trait tAsset = ScriptableObject.CreateInstance<Trait>();
  //         tAsset.attributeModifiers = new CharacterAttributeToIntDictionary(false);
  //         tAsset.name = objName + "_" + slot.ToString();
  //         splitPath[splitPath.Length - 1] = tAsset.name + ".asset";
  //         AssetDatabase.CreateAsset(tAsset, String.Join("/", splitPath));
  //     }
  // }

  public static Trait CreateTraitForBug(Character bug, TraitSlot slot)
  {
    Trait tAsset = ScriptableObject.CreateInstance<Trait>();
    tAsset.name = bug.gameObject.name + "_" + slot.ToString();
    string completePath = "Assets/resources/Data/TraitData/Traits/" + tAsset.name + ".asset";
    Debug.Log("complete path: " + completePath);
    tAsset.attributeModifiers = new CharacterAttributeToIntDictionary(false);
    tAsset.owningBugPrefab = bug.gameObject;
    AssetDatabase.CreateAsset(tAsset, completePath);
    // Select the object in the project folder
    Selection.activeObject = tAsset;

    // Also flash the folder yellow to highlight it
    EditorGUIUtility.PingObject(tAsset);
    return tAsset;
  }

#endif


  public static TraitSlot[] slots = new TraitSlot[] { TraitSlot.Head, TraitSlot.Abdomen, TraitSlot.Legs, TraitSlot.Wings, TraitSlot.Thorax };
}


// public abstract class TraitMono : MonoBehaviour {
//  TraitName traitName;
//  abstract public TraitType traitType { get; set; }
//  //  other?
// }

[System.Serializable]
public class UpcomingLifeTrait
{
  public Trait trait;
  public InventoryEntry inventoryItem;
  public LymphType lymphType;

  public UpcomingLifeTrait(Trait t, LymphType lt, InventoryEntry ie)
  {
    trait = t;
    lymphType = lt;
    inventoryItem = ie;
  }
}
