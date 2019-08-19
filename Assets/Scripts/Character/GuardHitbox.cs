using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct GuardBreakpointInfo {
  public int breakpoint;
  public Color col;
  public float damageReductionPercent;
  public GuardBreakpointInfo(int b, Color c, float dr) {
    breakpoint = b;
    col = c;
    damageReductionPercent = dr;
  }
}

public class GuardHitbox : MonoBehaviour
{
  public int maxGuardLevel = 100;

  public Character owner;
  public SpriteRenderer sr;
  public Color damageFlashColor = Color.white;
  public float damageFlashSpeed = .5f;

  public GuardBreakpointInfo currentGuard;
  private Coroutine damageFlashCoroutine;
  public bool damageFlashRunning = false;
    // Update is called once per frame
  void Update()
  {
    currentGuard = Constants.DefaultGuardBreakpointInfo[0];
    foreach(GuardBreakpointInfo gbi in Constants.DefaultGuardBreakpointInfo) {
      if (maxGuardLevel - (owner.maxStamina - owner.stamina) < gbi.breakpoint) {
        currentGuard = gbi;
      } else {
        break;
      }
    }
    if (!damageFlashRunning) {
      sr.color = currentGuard.col;
    }
  }

  void TakeDamage(DamageObject damageObj) {
		if (
      owner.sourceInvulnerabilities.Contains(damageObj.sourceString)
      && !damageObj.ignoreInvulnerability)
    { return; }
		if (damageObj.attackerTransform == owner.transform) { return; }
    Debug.Log("TakeDamage, damage: "+damageObj.damage+", damageReductionPercent" +currentGuard.damageReductionPercent);
    owner.CalculateAndApplyDamage(damageObj, currentGuard.damageReductionPercent / 100);
    if (damageFlashRunning) {
      StopCoroutine(damageFlashCoroutine);
    }
		damageFlashCoroutine = StartCoroutine(ApplyDamageFlash(damageObj));
  }

  private IEnumerator ApplyDamageFlash(DamageObject damageObj) {
    // Todo: might wanna change this!
    damageFlashRunning = true;
    Color baseColor = currentGuard.col;
    sr.color = damageFlashColor;
    yield return new WaitForSeconds(damageFlashSpeed / 3);
    sr.color = baseColor;
    yield return new WaitForSeconds(damageFlashSpeed / 3);
    sr.color = damageFlashColor;
    yield return new WaitForSeconds(damageFlashSpeed / 3);
    sr.color = baseColor;
    damageFlashRunning = false;
  }

 }
