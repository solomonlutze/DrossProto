using System;
using System.Collections.Generic;
using UnityEngine;
public static class Constants
{
    public enum GameState { Play, Dead, Dialogue, Menu }
    public const float DEFAULT_DETECTION_RANGE = 20f;
    public static int firstFloorLayerIndex = LayerMask.NameToLayer("B6");
    public static int numberOfFloorLayers = Enum.GetNames(typeof(FloorLayer)).Length;

    public static Dictionary<CharacterAttackValue, float> CharacterAttackAdjustmentIncrements
      = new Dictionary<CharacterAttackValue, float>() {
          {CharacterAttackValue.AttackSpeed, .02f},
          {CharacterAttackValue.Cooldown, .05f},
          {CharacterAttackValue.Damage, 10f},
          {CharacterAttackValue.HitboxSize, .25f},
          {CharacterAttackValue.Knockback, .5f},
          {CharacterAttackValue.Range, .5f},
          {CharacterAttackValue.Stun, .3f},
          {CharacterAttackValue.Venom, -12f}
      };
}