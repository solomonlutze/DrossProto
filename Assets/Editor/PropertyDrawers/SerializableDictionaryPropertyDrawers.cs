using UnityEditor;
using UnityEngine;
using System.Collections;
using System;

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(DamageTypeToFloatDictionary))]
[CustomPropertyDrawer(typeof(CharacterStatToFloatDictionary))]
[CustomPropertyDrawer(typeof(LayerToLayerFloorDictionary))]
public class AnySerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer {}
#endif