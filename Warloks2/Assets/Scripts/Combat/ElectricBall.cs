using FishNet.Object;
using UnityEngine;

public class ElectricBall : NetworkBehaviour
{
    public float speed = 10f;
    public float damage = 30;
    public float lifetime = 5f;
    public GameObject explosionEffect;
    NetworkDespawn networkDespawn;
    int ownerId;
    public Vector3 desiredPosition;
    Vector3 flightDirection;
    bool isMoving = true;

    public override void OnStartServer()
    {
        networkDespawn = GetComponent<NetworkDespawn>();
        if (networkDespawn != null)
            networkDespawn.DespawnAfter(gameObject, lifetime);
        flightDirection = (desiredPosition - transform.position).normalized;
    }

    void Start()
    {
        flightDirection = (desiredPosition - transform.position).normalized;
        if (!IsSpawned)
            return;
        if (!base.Owner.IsLocalClient)
            enabled = false;
    }

    [ObserversRpc]
    public void InitObserversRpc(Vector3 desired, int ownerClientId)
    {
        desiredPosition = desired;
        ownerId = ownerClientId;
        flightDirection = (desiredPosition - transform.position).normalized;
    }

    void FixedUpdate()
    {
        if (!IsOwner || !isMoving)
            return;
        transform.position += flightDirection * speed * Time.deltaTime;
    }

    public void SetOwner(int playerId)
    {
        ownerId = playerId;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
            return;

        if (other.CompareTag("Player"))
        {
            NetworkObject playerNob = other.GetComponent<NetworkObject>();
            if (playerNob != null && playerNob.OwnerId == ownerId)
                return;

            Player health = other.GetComponent<Player>();
            if (health != null)
                health.TakeDamage(damage);
        }

        if (explosionEffect != null)
            explosionEffect.SetActive(true);

        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null)
            sphere.enabled = false;
        isMoving = false;
        if (networkDespawn != null)
            networkDespawn.DespawnAfter(gameObject, 0.5f);
    }
}
