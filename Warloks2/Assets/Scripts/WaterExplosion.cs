using Photon.Pun;
using UnityEngine;

public class WaterExplosion : MonoBehaviourPunCallbacks
{
    public float explosionForce = 15f;
    public float explosionRadius = 50f;
    public float upwardsModifier = 5f;
    public float damage = 5f;
    private int ownerId;
    void Start()
    {
        if (!photonView.IsMine)
        {
            enabled = false;
        }
        // ��������� ���� ������ ��� ������
        ApplyExplosionForce();

        // ���������� ����� 3 �������
        PhotonDestroy photonDestroy = GetComponent<PhotonDestroy>();
        photonDestroy.PunDestroy(gameObject, 3f);
    }
    public void SetOwner(int playerId)
    {
        ownerId = playerId;
    }
    void ApplyExplosionForce()
    {
        // ������� ��� ���������� � ������� ������
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
                if (rb.gameObject.CompareTag("Player"))
                {
                    PhotonView playerView = rb.gameObject.GetComponent<PhotonView>();
                    if (playerView != null && playerView.Owner.ActorNumber == ownerId)
                        return;
                }
                if (rb.gameObject.CompareTag("Player"))
                {
                    Player health = rb.gameObject.GetComponent<Player>();

                    if (health != null)
                    {
                        health.TakeDamage(damage);
                    }
                }
            }
        }
    }

   
}