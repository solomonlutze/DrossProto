using UnityEditor;
using UnityEngine;
using System.Collections;
using System;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DamageTypeToFloatDictionary))]
[CustomPropertyDrawer(typeof(CharacterStatToFloatDictionary))]
[CustomPropertyDrawer(typeof(LayerToLayerFloorDictionary))]
[CustomPropertyDrawer(typeof(CharacterAttackValueToIntDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}
#endif