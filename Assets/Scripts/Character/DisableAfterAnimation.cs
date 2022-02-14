// Add an Animation Event to a GameObject that has an Animator
using UnityEngine;
using System.Collections;

public class DisableAfterAnimation : MonoBehaviour
{
  public AnimationClip clip;
  public void OnEnable()
  {
    StartCoroutine(WaitThenDisable());
    // existing components on the GameObject
    // AnimationClip clip;
    // Animator anim;

    // // new event created
    // AnimationEvent evt;
    // evt = new AnimationEvent();

    // // put some parameters on the AnimationEvent
    // //  - call the function called PrintEvent()
    // //  - the animation on this object lasts 2 seconds
    // //    and the new animation created here is
    // //    set up to happen 1.3s into the animation
    // evt.intParameter = 12345;
    // evt.time = 1.3f;
    // evt.functionName = "PrintEvent";
    // clip.
    // // get the animation clip and add the AnimationEvent
    // anim = GetComponent<Animator>();
    // clip = anim.runtimeAnimatorController.animationClips[0];
    // clip.AddEvent(evt);
  }
  IEnumerator WaitThenDisable()
  {
    yield return new WaitForSeconds(clip.averageDuration);
    Debug.Log("disable!");
    gameObject.SetActive(false);
  }
}