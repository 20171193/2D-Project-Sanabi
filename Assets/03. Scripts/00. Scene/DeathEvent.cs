using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeathEvent : MonoBehaviour
{ 
    [SerializeField]
    private Animator anim;

    public void EnableAnimator()
    {
        anim.SetBool("IsActive", true);
    }
    public void DisableAnimator()
    {
        anim.SetBool("IsActive", false);
    }

    public void EnableGlitch()
    {
        Manager.Camera.OnGlitchEffect();
    }
    public void DisableGlitch()
    {
        Manager.Camera.OffGlitchEffect();
    }

}
