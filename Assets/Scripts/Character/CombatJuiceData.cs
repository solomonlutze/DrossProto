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

  /*
  float knockbackDistance = knockback.magnitude;
    GameObject splatter = Instantiate(bloodSplatterPrefab, transform.position + knockback * knockbackDistance * .1f, transform.rotation);
    splatter.transform.localScale = new Vector3(.25f + knockbackDistance, .25f + knockbackDistance * .5f, 0);
    splatter.transform.rotation = GetDirectionAngle(knockback);
    bloodSplashParticleSystem.gameObject.transform.rotation = GetDirectionAngle(knockback);
    bloodSplashParticleSystem.Play();
    float slowdown = knockbackDistance;
    if (damageAfterResistances >= GetCharacterVital(CharacterVital.CurrentHealth))
    {
      slowdown = 3;
    }
    GameMaster.Instance.DoSlowdown(.2f, slowdown);
    BreakBodyParts(damageAfterResistances, knockback);
    PlayDamageSounds();
    DoCameraShake(damageAfterResistances, knockbackDistance);
    */
}