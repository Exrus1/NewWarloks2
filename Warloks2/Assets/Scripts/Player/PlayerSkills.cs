using System.Collections;
using FishNet.Object;
using UnityEngine;

public class PlayerSkills : NetworkBehaviour
{
    [SerializeField] float teleportDelay;
    [SerializeField] float teleportCooldown;
    bool teleportIsReady = true;
    [SerializeField] NetworkObject teleportParticle;

    [SerializeField] float invisibleDuration;
    [SerializeField] float invisibleCooldown;
    bool invisibleIsReady = true;
    [SerializeField] Material invisibleMaterial;
    [SerializeField] LayerMask defaultMask;
    [SerializeField] LayerMask invisibleMask;
    [SerializeField] SkinnedMeshRenderer mesh;
    Material defaultMaterial;

    public Transform shoot;
    public NetworkObject fireballPrefab;
    public NetworkObject electricBallPrefab;
    public NetworkObject waterExplosionPrefab;

    NetworkDespawn networkDespawn;
    PlayerController playerController;
    PositionalSoundSync positionalSound;
    Transform marker;

    [SerializeField] float ElectricBallCooldown;
    float ElectricBallTimer;
    public bool canCastElectricBall = true;

    [SerializeField] float waterExplosionCooldown;
    float waterExplosionTimer;
    public bool canCastWaterExplosion = true;

    public override void OnStartClient()
    {
        if (mesh != null)
            defaultMaterial = mesh.material;
        networkDespawn = GetComponent<NetworkDespawn>();
        playerController = GetComponent<PlayerController>();
        if (playerController != null && playerController.marker != null)
            marker = playerController.marker.transform;
        positionalSound = GetComponent<PositionalSoundSync>();
        ResolvePrefabs();
    }

    public override void OnStartServer()
    {
        ResolvePrefabs();
    }

    void ResolvePrefabs()
    {
        if (fireballPrefab == null)
            fireballPrefab = LoadNob("Fireball");
        if (electricBallPrefab == null)
            electricBallPrefab = LoadNob("ElectricBall");
        if (waterExplosionPrefab == null)
            waterExplosionPrefab = LoadNob("WaterExplosion");
        if (teleportParticle == null)
            teleportParticle = LoadNob("TeleportEffect");
    }

    static NetworkObject LoadNob(string resourceName)
    {
        GameObject go = Resources.Load<GameObject>(resourceName);
        return go != null ? go.GetComponent<NetworkObject>() : null;
    }

    public IEnumerator Teleport(Vector3 pos)
    {
        if (!IsOwner || !teleportIsReady)
            yield break;

        teleportIsReady = false;
        if (positionalSound != null)
            positionalSound.PlaySoundAtPosition(3, transform.position);

        yield return new WaitForSeconds(teleportDelay);
        TeleportServerRpc(pos);
        yield return new WaitForSeconds(teleportCooldown);
        teleportIsReady = true;
    }

    [ServerRpc]
    void TeleportServerRpc(Vector3 pos)
    {
        SpawnTeleportEffect(transform.position);
        transform.position = pos;
        SpawnTeleportEffect(pos);
        TeleportObserversRpc(pos);
    }

    [ObserversRpc(ExcludeOwner = true)]
    void TeleportObserversRpc(Vector3 pos)
    {
        transform.position = pos;
    }

    void SpawnTeleportEffect(Vector3 position)
    {
        if (teleportParticle == null)
            return;
        NetworkObject nob = NetworkManager.GetPooledInstantiated(teleportParticle, position, transform.rotation, true);
        ServerManager.Spawn(nob, Owner);
        NetworkDespawn nd = nob.GetComponent<NetworkDespawn>();
        if (nd != null)
            nd.DespawnAfter(nob.gameObject, 1f);
    }

    public IEnumerator Invisible()
    {
        if (!invisibleIsReady || !IsOwner)
            yield break;

        SetInvisionServerRpc(true);
        if (positionalSound != null)
            positionalSound.PlaySoundAtPosition(1, transform.position);
        yield return new WaitForSeconds(invisibleDuration);
        if (positionalSound != null)
            positionalSound.PlaySoundAtPosition(1, transform.position);
        SetInvisionServerRpc(false);
        yield return new WaitForSeconds(invisibleCooldown);
        invisibleIsReady = true;
    }

    [ServerRpc]
    void SetInvisionServerRpc(bool invisible)
    {
        SetInvisionObserversRpc(invisible);
    }

    [ObserversRpc]
    void SetInvisionObserversRpc(bool invisible)
    {
        if (invisible)
        {
            invisibleIsReady = false;
            gameObject.layer = 6;
            foreach (Transform child in gameObject.transform)
                child.gameObject.layer = 6;
            if (playerController != null && playerController.cam != null)
                playerController.cam.cullingMask = invisibleMask;
            if (mesh != null && invisibleMaterial != null)
                mesh.material = invisibleMaterial;
        }
        else
        {
            gameObject.layer = 0;
            foreach (Transform child in gameObject.transform)
                child.gameObject.layer = 0;
            if (playerController != null && playerController.cam != null)
                playerController.cam.cullingMask = defaultMask;
            if (mesh != null && defaultMaterial != null)
                mesh.material = defaultMaterial;
        }
    }

    public void CastFireball()
    {
        if (!IsOwner || marker == null || shoot == null)
            return;
        CastFireballServerRpc(shoot.position, marker.position);
        if (positionalSound != null)
            positionalSound.PlaySoundAtPosition(0, transform.position);
    }

    [ServerRpc]
    void CastFireballServerRpc(Vector3 spawnPos, Vector3 desired)
    {
        if (fireballPrefab == null)
            return;
        NetworkObject nob = NetworkManager.GetPooledInstantiated(fireballPrefab, spawnPos, transform.rotation, true);
        Fireball fb = nob.GetComponent<Fireball>();
        if (fb != null)
        {
            fb.desiredPosition = desired;
            fb.SetOwner(Owner.ClientId);
        }
        ServerManager.Spawn(nob, Owner);
        if (fb != null)
            fb.InitObserversRpc(desired, Owner.ClientId);
    }

    public void CastElectricBall()
    {
        if (!IsOwner || marker == null || shoot == null)
            return;
        canCastElectricBall = false;
        CastElectricBallServerRpc(shoot.position, marker.position);
        if (positionalSound != null)
            positionalSound.PlaySoundAtPosition(4, transform.position);
    }

    [ServerRpc]
    void CastElectricBallServerRpc(Vector3 spawnPos, Vector3 desired)
    {
        if (electricBallPrefab == null)
            return;
        NetworkObject nob = NetworkManager.GetPooledInstantiated(electricBallPrefab, spawnPos, transform.rotation, true);
        ElectricBall eb = nob.GetComponent<ElectricBall>();
        if (eb != null)
        {
            eb.desiredPosition = desired;
            eb.SetOwner(Owner.ClientId);
        }
        ServerManager.Spawn(nob, Owner);
        if (eb != null)
            eb.InitObserversRpc(desired, Owner.ClientId);
    }

    public void CastWaterExplosion()
    {
        if (!IsOwner || marker == null)
            return;
        canCastWaterExplosion = false;
        CastWaterExplosionServerRpc(marker.position);
    }

    [ServerRpc]
    void CastWaterExplosionServerRpc(Vector3 pos)
    {
        if (waterExplosionPrefab == null)
            return;
        NetworkObject nob = NetworkManager.GetPooledInstantiated(waterExplosionPrefab, pos, Quaternion.identity, true);
        WaterExplosion we = nob.GetComponent<WaterExplosion>();
        if (we != null)
            we.SetOwner(Owner.ClientId);
        ServerManager.Spawn(nob, Owner);
        if (we != null)
            we.InitObserversRpc(Owner.ClientId);
    }

    void FixedUpdate()
    {
        if (!IsOwner)
            return;

        if (ElectricBallTimer > ElectricBallCooldown)
        {
            ElectricBallTimer = 0;
            canCastElectricBall = true;
        }
        else
            ElectricBallTimer += Time.deltaTime;

        if (waterExplosionTimer > waterExplosionCooldown)
        {
            waterExplosionTimer = 0;
            canCastWaterExplosion = true;
        }
        else
            waterExplosionTimer += Time.deltaTime;
    }
}
