using System;
using System.Collections.Generic;
using UnityEngine;
public static class Constants
{
  public enum GameState { Play, Dead, Dialogue, Menu, ChooseBug }
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
        {CharacterAttackValue.DurabilityDamage, 1},
        {CharacterAttackValue.AcidDamage, 4f}
    };
  public static Dictionary<CharacterStat, float> CharacterStatAdjustmentIncrements
    = new Dictionary<CharacterStat, float>() {
        {CharacterStat.DetectableRange, .5f},
        {CharacterStat.MoveAcceleration, .05f},
        {CharacterStat.FlightAcceleration, .1f},
        {CharacterStat.Stamina, .3f},
        {CharacterStat.RotationSpeed, .02f},
    };
}