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
public class LayerToLayerFloorDictionary : SerializableDictionary<Constants.FloorLayer, LayerFloor>{ }