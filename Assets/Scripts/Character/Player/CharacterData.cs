using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterData : ScriptableObject
{

  public CharacterStatToFloatDictionary defaultStats;
  public CharacterSkillData defaultCharacterAttack;
  public DamageTypeToFloatDictionary damageTypeResistances;
  public CharacterAttackModifiers attackModifiers;
  public CharacterAttackModifiers dashAttackModifiers;
  public List<CharacterMovementAbility> movementAbilities;
  public CharacterAttributeToScriptableObjectDictionary attributeDatas;

  [Tooltip("Time it takes to move 1 unit while being knocked back.")]
  public float knockbackRate;

  public void Awake()
  {
    if (defaultStats == null)
    {
      defaultStats = new CharacterStatToFloatDictionary();
    }
    if (attributeDatas == null)
    {
      attributeDatas = new CharacterAttributeToScriptableObjectDictionary();
    }
    if (damageTypeResistances == null)
    {
      damageTypeResistances = new DamageTypeToFloatDictionary();
    }
    if (attackModifiers == null)
    {
      attackModifiers = new CharacterAttackModifiers();
    }
    if (dashAttackModifiers == null)
    {
      attackModifiers = new CharacterAttackModifiers();
    }
    if (movementAbilities == null)
    {
      movementAbilities = new List<CharacterMovementAbility>();
    }
  }

  public void RefreshAttributeDatas()
  {
    UnityEngine.Object[] dataObjects = Resources.LoadAll("Data/TraitData/Attributes");
    UnityEngine.ScriptableObject[] cast = dataObjects as ScriptableObject[];
    Debug.Log("dataObjects: " + dataObjects);
    Debug.Log("cast: " + cast);
    attributeDatas = new CharacterAttributeToScriptableObjectDictionary();
    IAttributeDataInterface ad;
    foreach (ScriptableObject obj in dataObjects)
    {
      ad = obj as FlightAttributeData;
      if (ad != null)
      {
        Debug.Log("adding " + ad + "as flightAttributeData");
        AddScriptableObjectToAttributeDatas(ad.attribute, obj);
        continue;
      }
      ad = obj as AttributeData;
      if (ad != null)
      {
        Debug.Log("adding " + ad + "as attributeData");
        AddScriptableObjectToAttributeDatas(ad.attribute, obj);
        continue;
      }
    }
    // attributeDatas.AddRange(dataObjects as AttributeData[]); //Resources.LoadAll("Data/TraitData/Attributes") as AttributeData[]);  
  }

  void AddScriptableObjectToAttributeDatas(CharacterAttribute attr, ScriptableObject ad)
  {
    if (!attributeDatas.ContainsKey(attr))
    {
      Debug.Log("adding attribute: " + attr + " to " + ad);
      attributeDatas.Add(attr, ad);
    }
    else
    {
      Debug.Log("updating attribute: " + attr + " to " + ad);
      attributeDatas[attr] = ad;
    }
  }

  // if this code base ever gets publicly scrutinized I want all of you to leave this section alone
  // I know it's possible to do better than this I just don't care
  public IAttributeDataInterface GetAttributeDataInterface(CharacterAttribute attribute)
  {
    return attributeDatas[attribute] as IAttributeDataInterface;
  }

  public AttributeData GetAttributeData(CharacterAttribute attribute)
  {
    return (AttributeData)GetAttributeDataInterface(attribute);
  }

  public FlightAttributeData GetFlightAttributeData()
  {
    return (FlightAttributeData)GetAttributeDataInterface(CharacterAttribute.Flight);
  }
  public DashAttributeData GetDashAttributeData()
  {
    return (DashAttributeData)GetAttributeDataInterface(CharacterAttribute.Dash);
  }

  public HealthAttributeData GetHealthAttributeData()
  {
    return (HealthAttributeData)GetAttributeDataInterface(CharacterAttribute.Health);
  }

  public MetabolismAttributeData GetMetabolismAttributeData()
  {
    return (MetabolismAttributeData)GetAttributeDataInterface(CharacterAttribute.Metabolism);
  }
  public HazardResistanceAttributeData GetHazardResistanceAttributeData()
  {
    return (HazardResistanceAttributeData)GetAttributeDataInterface(CharacterAttribute.HazardResistance);
  }

  public SightRangeAttributeData GetSightRangeAttributeData()
  {
    return (SightRangeAttributeData)GetAttributeDataInterface(CharacterAttribute.SightRange);
  }

  public DarkVisionAttributeData GetDarkVisionAttributeData()
  {
    return (DarkVisionAttributeData)GetAttributeDataInterface(CharacterAttribute.DarkVision);
  }

  public MoltingEfficiencyAttributeData GetMoltingEfficiencyAttributeData()
  {
    return (MoltingEfficiencyAttributeData)GetAttributeDataInterface(CharacterAttribute.MoltingEfficiency);
  }

  public ReflexesAttributeData GetReflexesAttributeData()
  {
    return (ReflexesAttributeData)GetAttributeDataInterface(CharacterAttribute.Reflexes);
  }

  public ProtectionAttributeData GetProtectionAttributeDataForDamageType(DamageType damageType)
  {
    Debug.Log("damage type: " + damageType);
    Debug.Log("protection attribute: " + ProtectionAttributeData.DamageTypeToProtectionAttribute[damageType]);
    Debug.Log("physical attribute protection: " + ProtectionAttribute.Physical);
    return GetProtectionAttributeData(ProtectionAttributeData.DamageTypeToProtectionAttribute[damageType]);
  }

  public ProtectionAttributeData GetProtectionAttributeData(ProtectionAttribute attribute)
  {
    return (ProtectionAttributeData)GetAttributeDataInterface((CharacterAttribute)attribute);
  }


#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an PlayerCharacterData Asset
  [MenuItem("Assets/Create/CharacterData/PlayerCharacterData")]
  public static void CreatePlayerCharacterData()
  {
    string path = EditorUtility.SaveFilePanelInProject(
        "Save Player Character Data",
        "New Player Character Data",
        "Asset",
        "Save Player Character Data",
        "Assets/resources/Data/CharacterData/PlayerCharacterData"
    );
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterData>(), path);
  }

  // The following is a helper that adds a menu item to create an PlayerCharacterData Asset
  [MenuItem("Assets/Create/CharacterData/NpcData")]
  public static void CreateNPCData()
  {
    string path = EditorUtility.SaveFilePanelInProject(
        "Save NPC Data",
        "New NPC Data",
        "Asset",
        "Save NPC Data",
        "Assets/resources/Data/CharacterData/NpcData"
    );
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterData>(), path);
  }
#endif
}