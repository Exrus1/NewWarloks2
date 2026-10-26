using FishNet.Object;
using UnityEngine;

public class WaterExplosion : NetworkBehaviour
{
    public float explosionForce = 15f;
    public float explosionRadius = 50f;
    public float upwardsModifier = 5f;
    public float damage = 5f;
    int ownerId;
    bool applied;

    public override void OnStartServer()
    {
        NetworkDespawn networkDespawn = GetComponent<NetworkDespawn>();
        if (networkDespawn != null)
            networkDespawn.DespawnAfter(gameObject, 3f);
    }

    void Start()
    {
        if (!IsSpawned)
            return;
        if (!base.Owner.IsLocalClient)
        {
            enabled = false;
            return;
        }
        ApplyExplosionForce();
    }

    [ObserversRpc]
    public void InitObserversRpc(int ownerClientId)
    {
        ownerId = ownerClientId;
    }

    public void SetOwner(int playerId)
    {
        ownerId = playerId;
    }

    void ApplyExplosionForce()
    {
        if (applied)
            return;
        applied = true;

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb == null)
                continue;

            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            if (!rb.gameObject.CompareTag("Player"))
                continue;

            NetworkObject playerNob = rb.gameObject.GetComponent<NetworkObject>();
            if (playerNob != null && playerNob.OwnerId == ownerId)
                continue;

            Player health = rb.gameObject.GetComponent<Player>();
            if (health != null)
                health.TakeDamage(damage);
        }
    }
}
