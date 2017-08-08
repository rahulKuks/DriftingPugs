using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is not a Monobehaviour script, but a state machine behaviour that can be attached to animation clips. 
/// Attach this to the relevant animation clip of the sprite whenever you need to play sounds that are specific to a particular animation.
/// </summary>
public class PlaySound : StateMachineBehaviour 
{
	[Tooltip("The audio clip that will be played on state enter. To time the clip properly to the animation, ensure that its the same length as the animation.")]
	[SerializeField] AudioClip spriteSound;
	[Tooltip("Set the sound to loop")]
	[SerializeField] bool loop;

	 // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		//Acquire audio source and play sound.
		AudioSource spriteAudioSource = GameObject.Find ("Sprite").GetComponent<AudioSource> ();
		if (loop) 
		{
			spriteAudioSource.loop = true;
		} 
		else 
		{
			spriteAudioSource.loop = false;
		}
			
		spriteAudioSource.clip = spriteSound;
		spriteAudioSource.Play ();
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	//override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	//override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
	//override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
	//
	//}
}
