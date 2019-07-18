using System;
using System.Collections.Generic;
using UnityEngine;
public static class Constants
{
  public enum GameState { Play, Dead, Dialogue, Menu }
  public const float DEFAULT_DETECTION_RANGE = 20f;
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
  public static Dictionary<CharacterStat, float> CharacterStatAdjustmentIncrements
    = new Dictionary<CharacterStat, float>() {
        {CharacterStat.DetectableRange, .5f},
        {CharacterStat.MaxHealth, .02f},
        {CharacterStat.MoveAcceleration, .05f},
        {CharacterStat.MaxEnvironmentalDamageCooldown, 10f},
        {CharacterStat.RotationSpeed, .02f},
    };

  public static Dictionary<CharacterStat, float> BaseCharacterStats
    = new Dictionary<CharacterStat, float>() {
        {CharacterStat.DetectableRange, 10f},
        {CharacterStat.MaxHealth, 50f},
        {CharacterStat.MoveAcceleration, .5f},
        {CharacterStat.MaxEnvironmentalDamageCooldown, .25f},
        {CharacterStat.RotationSpeed, 2f},
    };
}