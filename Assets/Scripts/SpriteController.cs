using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

	public void MoveAnimation(int index)
    {
        anim.SetInteger("Checkpoint", index);
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
