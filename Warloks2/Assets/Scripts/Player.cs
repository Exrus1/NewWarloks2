using System.Collections;
using FishNet.Object;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    float health = 1000;
    bool isDead;
    [SerializeField] TMP_Text hpText;
    Animator anim;
    PlayerController controller;
    ThirdPersonCamera third;
    [SerializeField] Rigidbody[] childrenRb;
    [SerializeField] Collider[] colliders;

    public override void OnStartClient()
    {
        anim = GetComponent<Animator>();
        if (hpText != null)
            hpText.text = health.ToString();
        controller = GetComponent<PlayerController>();
        third = GetComponent<ThirdPersonCamera>();
        foreach (Rigidbody rb in childrenRb)
            rb.isKinematic = true;
        foreach (Collider col in colliders)
            col.enabled = false;

        if (!IsOwner)
        {
            Transform cam = transform.Find("Main Camera");
            if (cam != null)
                cam.gameObject.SetActive(false);
            Transform canvas = transform.Find("Canvas");
            if (canvas != null)
                canvas.gameObject.SetActive(false);
            enabled = false;
        }
    }

    public IEnumerator GradualDamage(float duration, float damage)
    {
        while (duration > 0)
        {
            duration -= 0.25f;
            TakeDamage(damage);
            yield return new WaitForSeconds(0.25f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || IsOwner)
            return;
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(float damage)
    {
        TakeDamageObserversRpc(damage);
    }

    [ObserversRpc]
    void TakeDamageObserversRpc(float damage)
    {
        if (isDead)
            return;

        health -= damage;
        if (hpText != null)
            hpText.text = health.ToString("f0");

        if (health <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        foreach (Rigidbody rb in childrenRb)
        {
            rb.isKinematic = false;
            rb.mass = 0.1f;
        }
        foreach (Collider col in colliders)
            col.enabled = true;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            capsule.radius = 0.01f;
            capsule.height = 0.01f;
        }

        if (anim != null)
            anim.enabled = false;

        if (IsOwner)
        {
            if (controller != null)
                controller.enabled = false;
            if (third != null)
                third.enabled = false;
            enabled = false;
        }
    }
}
