using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

public class SerializableDictionaries : MonoBehaviour {

}
[System.Serializable]
public class TestDictionary : SerializableDictionaryBase<DamageType, float> { }
[System.Serializable]
public class DamageTypeToFloatDictionary : SerializableDictionaryBase<DamageType, float>{ }

[System.Serializable]
public class CharacterStatToFloatDictionary : SerializableDictionaryBase<CharacterStat, float>{ }

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