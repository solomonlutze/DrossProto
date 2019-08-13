using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

public class SerializableDictionaries : MonoBehaviour {

}
[System.Serializable]
public class TraitSlotToUpcomingTraitDictionary : SerializableDictionaryBase<TraitSlot, UpcomingLifeTrait> {
  public TraitSlotToUpcomingTraitDictionary() {
    foreach(TraitSlot slot in (TraitSlot[]) Enum.GetValues(typeof(TraitSlot))) {
      Add(slot, new UpcomingLifeTrait(null, LymphType.None, null));
    }
  }
}

[System.Serializable]
public class LymphTypeToIntDictionary : SerializableDictionaryBase<LymphType, int> {

    public LymphTypeToIntDictionary() {
      foreach(LymphType lymphType in (LymphType[]) Enum.GetValues(typeof(LymphType))) {
        Add(lymphType, 0);
      }
    }
}
[System.Serializable]
public class TraitSlotToTraitDictionary : SerializableDictionaryBase<TraitSlot, Trait> {

    public TraitSlotToTraitDictionary() {
      foreach(TraitSlot slot in (TraitSlot[]) Enum.GetValues(typeof(TraitSlot))) {
        Add(slot, null);
      }
    }
}
[System.Serializable]
public class DamageTypeToFloatDictionary : SerializableDictionaryBase<DamageType, float>{ }


[System.Serializable]
public class StringToIntDictionary : SerializableDictionaryBase<string, int> {
}
[System.Serializable]
public class StatToActiveStatModificationsDictionary : SerializableDictionaryBase<CharacterStat, StringToIntDictionary>{
  public StatToActiveStatModificationsDictionary() {
      foreach(CharacterStat stat in (CharacterStat[]) Enum.GetValues(typeof(CharacterStat))) {
        Add(stat, new StringToIntDictionary());
      }
    }
  }

[System.Serializable]
public class CharacterVitalToFloatDictionary : SerializableDictionaryBase<CharacterVital, float>{
    public CharacterVitalToFloatDictionary() {
      Add(CharacterVital.CurrentHealth, Constants.BaseCharacterStats[CharacterStat.MaxHealth]);
      Add(CharacterVital.CurrentEnvironmentalDamageCooldown, Constants.BaseCharacterStats[CharacterStat.MaxEnvironmentalDamageCooldown]);
      Add(CharacterVital.CurrentDashCooldown, 0f);
    }
  }

[System.Serializable]
public class CharacterStatToFloatDictionary : SerializableDictionaryBase<CharacterStat, float>{
    public CharacterStatToFloatDictionary() {
      foreach(CharacterStat stat in (CharacterStat[]) Enum.GetValues(typeof(CharacterStat))) {
        Add(stat, Constants.BaseCharacterStats[stat]);
      }
    }
 }

[System.Serializable]
public class LayerToLayerFloorDictionary : SerializableDictionaryBase<FloorLayer, LayerFloor>{ }

[System.Serializable]
public class LymphTypeToLymphTypeSkillsDictionary : SerializableDictionaryBase<LymphType, LymphTypeSkills> {
  public LymphTypeToLymphTypeSkillsDictionary() {
    foreach(LymphType type in (LymphType[]) Enum.GetValues(typeof(LymphType))) {
      if (type == LymphType.None) { continue; }
      Add(type, new LymphTypeSkills());
    }
  }
}

[System.Serializable]
public class LymphTypeToSpriteDictionary : SerializableDictionaryBase<LymphType, Sprite> {
  public LymphTypeToSpriteDictionary() {
    foreach(LymphType type in (LymphType[]) Enum.GetValues(typeof(LymphType))) {
      if (type == LymphType.None) { continue; }
      Add(type, null);
    }
  }
}

[System.Serializable]
public class CharacterAttackValueToIntDictionary : SerializableDictionaryBase<CharacterAttackValue, int>{

    public CharacterAttackValueToIntDictionary() {
      foreach(CharacterAttackValue attackValue in (CharacterAttackValue[]) Enum.GetValues(typeof(CharacterAttackValue))) {
        Add(attackValue, 0);
      }
    }
}