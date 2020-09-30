using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimStateListener : StateMachineBehaviour
{
    public float fNormalizedFinishTime = 0.99f;

    bool bCanFinish = false;
    float fLastTime = -1;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        fNormalizedFinishTime = Mathf.Clamp01(fNormalizedFinishTime);
        bCanFinish = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (Application.isPlaying && !bCanFinish)
        {
            if (stateInfo.normalizedTime >= fNormalizedFinishTime)
            {
                bCanFinish = true;
                Asset_Role role = animator.GetComponent<Asset_Role>();
                if (role && role.Master)
                    role.Master.OnAnimLogicOver(stateInfo.shortNameHash);
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!Application.isPlaying) return;
        Asset_Role role = animator.GetComponent<Asset_Role>();
        if (role && role.Master)
            role.Master.OnAnimStateExit(stateInfo.shortNameHash);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
