using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

public class SerializableDictionaries : MonoBehaviour
{

}
[System.Serializable]
public class TraitSlotToUpcomingTraitDictionary : SerializableDictionaryBase<TraitSlot, UpcomingLifeTrait>
{
  public TraitSlotToUpcomingTraitDictionary()
  {
    foreach (TraitSlot slot in (TraitSlot[])Enum.GetValues(typeof(TraitSlot)))
    {
      Add(slot, new UpcomingLifeTrait(null, LymphType.None, null));
    }
  }
}

[System.Serializable]
public class LymphTypeToIntDictionary : SerializableDictionaryBase<LymphType, int>
{

  public LymphTypeToIntDictionary()
  {
    foreach (LymphType lymphType in (LymphType[])Enum.GetValues(typeof(LymphType)))
    {
      Add(lymphType, 0);
    }
  }
}

// [System.Serializable]
// public class TraitSlotToGameObjectDictionary : SerializableDictionaryBase<TraitSlot, GameObject>
// {

//     public TraitSlotToGameObjectDictionary()
//     {
//         foreach (TraitSlot slot in (TraitSlot[])Enum.GetValues(typeof(TraitSlot)))
//         {
//             Add(slot, null);
//         }
//     }
// }

[System.Serializable]
public class TraitSlotToCharacterCharacterBodyPartVisualDictionary : SerializableDictionaryBase<TraitSlot, CharacterBodyPartVisual>
{

  public TraitSlotToCharacterCharacterBodyPartVisualDictionary()
  {
    foreach (TraitSlot slot in (TraitSlot[])Enum.GetValues(typeof(TraitSlot)))
    {
      Add(slot, null);
    }
  }
}

[System.Serializable]
public class TraitSlotToTraitDictionary : SerializableDictionaryBase<TraitSlot, Trait>
{

  public TraitSlotToTraitDictionary()
  {
    foreach (TraitSlot slot in (TraitSlot[])Enum.GetValues(typeof(TraitSlot)))
    {
      Add(slot, null);
    }
  }

  public TraitSlotToTraitDictionary(TraitSlotToTraitDictionary toClone)
  {
    foreach (TraitSlot slot in toClone.Keys)
    {
      Add(slot, toClone[slot]);
    }
  }
}
[System.Serializable]
public class DamageTypeToFloatDictionary : SerializableDictionaryBase<DamageType, float> { }


[System.Serializable]
public class CharacterAttributeToGameObjectDictionary : SerializableDictionaryBase<CharacterAttribute, GameObject>
{
  public CharacterAttributeToGameObjectDictionary()
  {
    {
      foreach (CharacterAttribute attribute in (CharacterAttribute[])Enum.GetValues(typeof(CharacterAttribute)))
      {
        Add(attribute, null);
      }
    }
  }
}


[System.Serializable]
public class CharacterAttributeToScriptableObjectDictionary : SerializableDictionaryBase<CharacterAttribute, ScriptableObject>
{
  public CharacterAttributeToScriptableObjectDictionary()
  {
    {
      foreach (CharacterAttribute attribute in (CharacterAttribute[])Enum.GetValues(typeof(CharacterAttribute)))
      {
        Add(attribute, null);
      }
    }
  }
}
[System.Serializable]
public class CharacterAttributeToIntDictionary : SerializableDictionaryBase<CharacterAttribute, int>
{

  public CharacterAttributeToIntDictionary(bool populate = true)
  {
    if (populate)
    {
      foreach (CharacterAttribute attribute in (CharacterAttribute[])Enum.GetValues(typeof(CharacterAttribute)))
      {
        if (attribute.ToString().StartsWith("REMOVE")) { continue; }
        Add(attribute, 0);
      }
    }
  }
}

[System.Serializable]
public class StringToIntDictionary : SerializableDictionaryBase<string, int>
{
}
[System.Serializable]
public class StatToActiveStatModificationsDictionary : SerializableDictionaryBase<CharacterStat, StringToIntDictionary>
{
  public StatToActiveStatModificationsDictionary()
  {
    foreach (CharacterStat stat in (CharacterStat[])Enum.GetValues(typeof(CharacterStat)))
    {
      Add(stat, new StringToIntDictionary());
    }
  }
}

[System.Serializable]
public class CharacterVitalToFloatDictionary : SerializableDictionaryBase<CharacterVital, float>
{
  public CharacterVitalToFloatDictionary()
  {
    Add(CharacterVital.CurrentHealth, 0);
    Add(CharacterVital.CurrentMoltCount, 0);
    Add(CharacterVital.CurrentStamina, 0);
    Add(CharacterVital.CurrentCarapace, 0);
    // Add(CharacterVital.CurrentDashCooldown, 0f);
  }
}

[System.Serializable]
public class CharacterStatToFloatDictionary : SerializableDictionaryBase<CharacterStat, float>
{
  public CharacterStatToFloatDictionary()
  {
    foreach (CharacterStat stat in (CharacterStat[])Enum.GetValues(typeof(CharacterStat)))
    {
      Add(stat, 0);
    }
  }
}

[System.Serializable]
public class CharacterAttributeToIAttributeDataInterfaceDictionary : SerializableDictionaryBase<CharacterAttribute, IAttributeDataInterface>
{
  public CharacterAttributeToIAttributeDataInterfaceDictionary()
  {

    foreach (CharacterAttribute attribute in (CharacterAttribute[])Enum.GetValues(typeof(CharacterAttribute)))
    {
      Add(attribute, null);
    }
  }
}

[System.Serializable]
public class LayerToLayerFloorDictionary : SerializableDictionaryBase<FloorLayer, LayerFloor> { }


[System.Serializable]
public class LymphTypeToSpriteDictionary : SerializableDictionaryBase<LymphType, Sprite>
{
  public LymphTypeToSpriteDictionary()
  {
    foreach (LymphType type in (LymphType[])Enum.GetValues(typeof(LymphType)))
    {
      if (type == LymphType.None) { continue; }
      Add(type, null);
    }
  }
}

[System.Serializable]
public class CharacterAttackValueToIntDictionary : SerializableDictionaryBase<CharacterAttackValue, int>
{

  public CharacterAttackValueToIntDictionary()
  {
    foreach (CharacterAttackValue attackValue in (CharacterAttackValue[])Enum.GetValues(typeof(CharacterAttackValue)))
    {
      Add(attackValue, 0);
    }
  }
}



[System.Serializable]
public class AttackTypeToCharacterSkillDataDictionary : SerializableDictionaryBase<AttackType, CharacterSkillData>
{

  public AttackTypeToCharacterSkillDataDictionary()
  {
    foreach (AttackType type in (AttackType[])Enum.GetValues(typeof(AttackType)))
    {
      Add(type, null);
    }
  }
}

// [System.Serializable]
// public class AttackTypeToSkillEffectDictionary : SerializableDictionaryBase<AttackType, SkillEffect>
// {

//   public AttackTypeToSkillEffectDictionary()
//   {
//     foreach (AttackType type in (AttackType[])Enum.GetValues(typeof(AttackType)))
//     {
//       Add(type, null);
//     }
//   }
// }
[System.Serializable]
public class AttackTypeToWeaponDataDictionary : SerializableDictionaryBase<AttackType, WeaponData>
{

  public AttackTypeToWeaponDataDictionary()
  {
    foreach (AttackType type in (AttackType[])Enum.GetValues(typeof(AttackType)))
    {
      Add(type, null);
    }
  }
}