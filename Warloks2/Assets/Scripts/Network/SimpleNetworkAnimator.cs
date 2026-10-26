using FishNet.Object;
using UnityEngine;

/// <summary>Lightweight animator sync for key player params (no FISHNET_ANIMATOR_CHANNEL required).</summary>
public class SimpleNetworkAnimator : NetworkBehaviour
{
    [SerializeField] Animator animator;

    float lastH;
    float lastV;
    bool lastJump;
    bool lastFloating;
    bool lastCast;
    bool lastElectric;
    bool lastWater;

    public override void OnStartClient()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner || animator == null)
            return;

        float h = animator.GetFloat("h");
        float v = animator.GetFloat("v");
        bool jump = animator.GetBool("jump");
        bool floating = animator.GetBool("floating");
        bool cast = animator.GetBool("cast");
        bool electric = animator.GetBool("electricBallCast");
        bool water = animator.GetBool("waterExplosionCast");

        if (Mathf.Abs(h - lastH) > 0.01f || Mathf.Abs(v - lastV) > 0.01f ||
            jump != lastJump || floating != lastFloating || cast != lastCast ||
            electric != lastElectric || water != lastWater)
        {
            lastH = h;
            lastV = v;
            lastJump = jump;
            lastFloating = floating;
            lastCast = cast;
            lastElectric = electric;
            lastWater = water;
            SyncObserversRpc(h, v, jump, floating, cast, electric, water);
        }
    }

    [ObserversRpc(ExcludeOwner = true)]
    void SyncObserversRpc(float h, float v, bool jump, bool floating, bool cast, bool electric, bool water)
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (animator == null)
            return;

        animator.SetFloat("h", h);
        animator.SetFloat("v", v);
        animator.SetBool("jump", jump);
        animator.SetBool("floating", floating);
        animator.SetBool("cast", cast);
        animator.SetBool("electricBallCast", electric);
        animator.SetBool("waterExplosionCast", water);
    }
}
