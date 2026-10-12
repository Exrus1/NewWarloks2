using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public float gravity;
    [Header("Spawn Settings")]
    public NetworkObject playerPrefab;

    SoundManager musicManager;

    public override void OnStartNetwork()
    {
        musicManager = FindAnyObjectByType<SoundManager>();
        Physics.gravity = new Vector3(0, -9.81f * gravity, 0);
    }

    public override void OnStartServer()
    {
        if (playerPrefab == null)
        {
            GameObject go = Resources.Load<GameObject>("Player");
            if (go != null)
                playerPrefab = go.GetComponent<NetworkObject>();
        }

        foreach (NetworkConnection conn in ServerManager.Clients.Values)
            TrySpawnPlayer(conn);

        ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        if (ServerManager != null)
            ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
    }

    void OnRemoteConnectionState(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == FishNet.Transporting.RemoteConnectionState.Started)
            TrySpawnPlayer(conn);
    }

    void TrySpawnPlayer(NetworkConnection conn)
    {
        if (playerPrefab == null || conn == null || !conn.IsActive)
            return;

        // Dedicated server has no playable local client object to own.
        if (conn.IsLocalClient && !NetworkManager.IsClientStarted)
            return;

        foreach (NetworkObject owned in conn.Objects)
        {
            if (owned != null && owned.CompareTag("Player"))
                return;
        }

        Vector3 pos = new(Random.Range(-25, 25), 50, Random.Range(25, 80));
        NetworkObject nob = NetworkManager.GetPooledInstantiated(playerPrefab, pos, Quaternion.identity, true);
        ServerManager.Spawn(nob, conn);
    }

    public void EndGame()
    {
        if (musicManager != null)
            musicManager.PlayMusic();
        if (IsServerStarted)
            NetworkSceneHelper.LoadGlobalReplace("Lobby");
    }
}
