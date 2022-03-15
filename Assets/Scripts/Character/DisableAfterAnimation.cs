// Add an Animation Event to a GameObject that has an Animator
using UnityEngine;
using System.Collections;

public class DisableAfterAnimation : MonoBehaviour
{
  public AnimationClip clip;
  public void OnEnable()
  {
    StartCoroutine(WaitThenDisable());
  }
  IEnumerator WaitThenDisable()
  {
    yield return new WaitForSeconds(clip.averageDuration);
    gameObject.SetActive(false);
  }
}