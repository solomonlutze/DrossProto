using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionaries : MonoBehaviour {

}
[System.Serializable]
public class DamageTypeToFloatDictionary : SerializableDictionary<DamageType, float>{ }

[System.Serializable]
public class CharacterStatToFloatDictionary : SerializableDictionary<CharacterStat, float>{ }

[System.Serializable]
public class LayerToLayerFloorDictionary : SerializableDictionary<FloorLayer, LayerFloor>{ }
[System.Serializable]
public class CharacterAttackValueToIntDictionary : SerializableDictionary<CharacterAttackValue, int>{

    public CharacterAttackValueToIntDictionary() {
        Add(CharacterAttackValue.AttackSpeed, 0);
        Add(CharacterAttackValue.Cooldown, 0);
        Add(CharacterAttackValue.Damage, 0);
        Add(CharacterAttackValue.HitboxSize, 0);
        Add(CharacterAttackValue.Knockback, 0);
        Add(CharacterAttackValue.Range, 0);
        Add(CharacterAttackValue.Stun, 0);
        Add(CharacterAttackValue.Venom, 0);
    }
}