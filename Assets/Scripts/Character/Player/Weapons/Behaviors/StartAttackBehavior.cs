using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this behavior should live on every attack's FIRST animation.
public class StartAttackBehavior : StateMachineBehaviour {

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		animator.gameObject.SendMessage("OnAttackStart");
		animator.SetBool("canAttack", false);
	}

}
