using FishNet.Object;
using UnityEngine;

public class Fireball : NetworkBehaviour
{
    public float speed = 10f;
    public float damage = 30;
    public float lifetime = 5f;
    public GameObject explosionEffect;
    NetworkDespawn networkDespawn;
    int ownerId;
    [SerializeField] GameObject[] particles;
    public Vector3 desiredPosition;

    public override void OnStartServer()
    {
        networkDespawn = GetComponent<NetworkDespawn>();
        if (networkDespawn != null)
            networkDespawn.DespawnAfter(gameObject, lifetime);
    }

    void Start()
    {
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
    }

    void FixedUpdate()
    {
        if (!IsOwner)
            return;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, speed * Time.deltaTime);
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
            {
                health.StartCoroutine(health.GradualDamage(lifetime, damage));
                SetNetworkParent(other.transform);
                BoxCollider box = GetComponent<BoxCollider>();
                if (box != null)
                    box.enabled = false;
            }
        }
    }

    [ObserversRpc]
    void RpcSetParent(int parentObjectId)
    {
        if (parentObjectId == -1)
        {
            transform.SetParent(null);
            return;
        }

        NetworkObject parentNob = null;
        if (IsClientInitialized && NetworkManager.ClientManager.Objects.Spawned.TryGetValue(parentObjectId, out parentNob) == false)
            parentNob = null;
        if (parentNob == null && IsServerInitialized)
            NetworkManager.ServerManager.Objects.Spawned.TryGetValue(parentObjectId, out parentNob);

        if (parentNob != null)
        {
            transform.SetParent(parentNob.transform);
            transform.localPosition = new Vector3(Random.Range(-0.15f, 0.15f), Random.Range(0.5f, 1.5f), Random.Range(-0.15f, 0.15f));
        }

        transform.localScale = Vector3.one * 0.5f;
        if (particles != null)
        {
            foreach (GameObject particle in particles)
            {
                if (particle != null)
                    particle.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
        enabled = false;
    }

    public void SetNetworkParent(Transform newParent)
    {
        int parentId = -1;
        if (newParent != null)
        {
            NetworkObject parentNob = newParent.GetComponent<NetworkObject>();
            if (parentNob != null)
                parentId = parentNob.ObjectId;
        }
        RpcSetParentServerRpc(parentId);
    }

    [ServerRpc]
    void RpcSetParentServerRpc(int parentObjectId)
    {
        RpcSetParent(parentObjectId);
    }
}
