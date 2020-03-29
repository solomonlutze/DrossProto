using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterData : ScriptableObject
{

  public CharacterStatToFloatDictionary defaultStats;
  public DamageTypeToFloatDictionary damageTypeResistances;
  public CharacterAttackModifiers attackModifiers;
  public CharacterAttackModifiers dashAttackModifiers;
  public List<CharacterMovementAbility> movementAbilities;
  [HideInInspector]
  public CharacterAttributeToIAttributeDataInterfaceDictionary attributeDatas;

  public void Awake()
  {
    if (defaultStats == null)
    {
      defaultStats = new CharacterStatToFloatDictionary();
    }
    if (attributeDatas == null)
    {
      Debug.Log("creating new dictionary?");
      attributeDatas = new CharacterAttributeToIAttributeDataInterfaceDictionary();
      Debug.Log("attribute datas? " + attributeDatas);
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
    attributeDatas = new CharacterAttributeToIAttributeDataInterfaceDictionary();
    IAttributeDataInterface ad;
    foreach (Object obj in dataObjects)
    {
      ad = obj as FlightAttributeData;
      if (ad != null)
      {
        Debug.Log("adding " + ad + "as flightAttributeData");
        AddIAttributeDataInterfaceToAttributeDatas(ad);
        continue;
      }
      ad = obj as AttributeData;
      if (ad != null)
      {
        Debug.Log("adding " + ad + "as attributeData");
        AddIAttributeDataInterfaceToAttributeDatas(ad);
        continue;
      }
    }
    // attributeDatas.AddRange(dataObjects as AttributeData[]); //Resources.LoadAll("Data/TraitData/Attributes") as AttributeData[]);  
  }

  void AddIAttributeDataInterfaceToAttributeDatas(IAttributeDataInterface ad)
  {
    if (!attributeDatas.ContainsKey(ad.attribute))
    {
      Debug.Log("adding attribute: " + ad.attribute);
      attributeDatas.Add(ad.attribute, ad);
    }
    else
    {
      Debug.Log("updating attribute: " + ad.attribute);
      attributeDatas[ad.attribute] = ad;
    }
  }


  public IAttributeDataInterface GetAttributeData(CharacterAttribute attribute)
  {

    return attributeDatas[attribute];
  }

  public FlightAttributeData GetFlightAttributeData()
  {
    Debug.Log("attribute data for flight: " + GetAttributeData(CharacterAttribute.Flight));
    Debug.Log("flight attribute data for flight: " + (FlightAttributeData)GetAttributeData(CharacterAttribute.Flight));
    return (FlightAttributeData)GetAttributeData(CharacterAttribute.Flight);
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