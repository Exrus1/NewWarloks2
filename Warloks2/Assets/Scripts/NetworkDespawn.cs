using System.Collections;
using FishNet.Object;
using UnityEngine;

/// <summary>Despawns a networked object after a delay (FishNet replacement for PhotonDestroy).</summary>
public class NetworkDespawn : NetworkBehaviour
{
    public void DespawnAfter(GameObject go, float timeToKill)
    {
        StartCoroutine(DespawnRoutine(go, timeToKill));
    }

    IEnumerator DespawnRoutine(GameObject go, float timeToKill)
    {
        yield return new WaitForSeconds(timeToKill);
        if (go == null)
            yield break;

        NetworkObject nob = go.GetComponent<NetworkObject>();
        if (nob != null && nob.IsSpawned)
        {
            if (IsServerStarted)
                nob.Despawn();
        }
        else
        {
            Destroy(go);
        }
    }
}
