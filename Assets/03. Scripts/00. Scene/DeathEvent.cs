using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEvent : MonoBehaviour
{
    [SerializeField]
    private Animator anim;

    public void SetAnimator(bool isActive)
    {
        anim.enabled = isActive;
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
