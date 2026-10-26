using UnityEngine;

/// <summary>Legacy name retained so existing prefab references keep working; forwards to NetworkDespawn.</summary>
public class PhotonDestroy : MonoBehaviour
{
    NetworkDespawn networkDespawn;

    void Awake()
    {
        networkDespawn = GetComponent<NetworkDespawn>();
        if (networkDespawn == null)
            networkDespawn = gameObject.AddComponent<NetworkDespawn>();
    }

    public void PunDestroy(GameObject go, float timeToKill)
    {
        if (networkDespawn == null)
            networkDespawn = GetComponent<NetworkDespawn>();
        if (networkDespawn != null)
            networkDespawn.DespawnAfter(go, timeToKill);
    }
}
