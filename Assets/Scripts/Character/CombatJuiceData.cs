using UnityEngine;
[CreateAssetMenu(menuName = "GameplayConstants/CombatJuiceData")]
public class CombatJuiceData : ScriptableObject
{
  [Tooltip("splatter spawn position offset = knockback * knockbackDistance * splatterSpawnDistanceMult")]
  public float splatterSpawnDistanceMult = .1f;
  [Tooltip("min length of splatter (if knockback = 0)")]
  public float splatterBaseLength = .25f;
  [Tooltip("min width of splatter (if knockback = 0)")]
  public float splatterBaseWidth = .25f;
  [Tooltip("splatter length = knockbackDistance * splatterLengthMult")]
  public float splatterLengthMult = 1f;
  [Tooltip("splatter width = knockbackDistance * splatterWidthMult")]
  public float splatterWidthMult = .5f;
  [Tooltip("multiplier for duration slowdown (usually * knockbackDistance)")]
  public float slowdownDurationMult = .2f;
  [Tooltip("speed time moves at during hit slowdown")]
  public float slowdownTimescale = .2f;
  [Tooltip("base duration of slowdown when character dies")]
  public float deathSlowdownBaseDuration = 3f;
  [Tooltip("camera shake duration mult (* knockbackDistance) (player only)")]
  public float cameraShakeDurationMult_Player = .06f;
  [Tooltip("camera shake magnitude (* knockbackDistance) (player only)")]
  public float cameraShakeMagnitudeMult_Player = .04f;
  [Tooltip("camera shake duration mult (* knockbackDistance) (npc only)")]
  public float cameraShakeDurationMult_Npc = .05f;
  [Tooltip("camera shake magnitude (* knockbackDistance) (npc only)")]
  public float cameraShakeMagnitudeMult_Npc = .03f;
  [Tooltip("fewest particles that can spawn?")]
  public int bloodSplashParticleCountMin = 3;
  [Tooltip("how many damage points spawns an extra particle?")]
  public int extraBloodSplashParticlePerDamage = 10;
  [Tooltip("min count * bloodSplashParticleCountMaxMult = max that can spawn")]
  public float bloodSplashParticleCountMaxMult = 1.5f;
  public Gradient bloodSplashColorOverLife;
}