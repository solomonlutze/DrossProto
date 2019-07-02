using System;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

public class SerializableDictionaries : MonoBehaviour {

}
[System.Serializable]
public class TraitSlotToTraitDictionary : SerializableDictionaryBase<TraitSlot, PassiveTrait> {

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
    }
  }

[System.Serializable]
public class CharacterStatToFloatDictionary : SerializableDictionaryBase<CharacterStat, float>{
 }

[System.Serializable]
public class LayerToLayerFloorDictionary : SerializableDictionaryBase<FloorLayer, LayerFloor>{ }
[System.Serializable]
public class CharacterAttackValueToIntDictionary : SerializableDictionaryBase<CharacterAttackValue, int>{

    // public CharacterAttackValueToIntDictionary() {
    //     Add(CharacterAttackValue.AttackSpeed, 0);
    //     Add(CharacterAttackValue.Cooldown, 0);
    //     Add(CharacterAttackValue.Damage, 0);
    //     Add(CharacterAttackValue.HitboxSize, 0);
    //     Add(CharacterAttackValue.Knockback, 0);
    //     Add(CharacterAttackValue.Range, 0);
    //     Add(CharacterAttackValue.Stun, 0);
    //     Add(CharacterAttackValue.Venom, 0);
    // }
}