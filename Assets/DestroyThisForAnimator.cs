using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyThisForAnimator : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        HitEffectManager.instance.pool.Release(animator.GetComponent<SpriteRenderer>());
    }
}
