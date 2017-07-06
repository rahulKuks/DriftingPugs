using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
	private Animator parentAnim;
	[SerializeField] Animator childAnim;

    private void Awake()
    {
        parentAnim = GetComponent<Animator>();
    }

	public void MoveAnimation(int index)
    {
        parentAnim.SetInteger("Checkpoint", index);
		childAnim.SetBool ("isIdling", true);
		/*switch (index)
		{
			case 5:
				anim.enabled = false;
				break;
			case 7:
				anim.enabled = true;
				break;
		}*/
    }
}
