using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerSkills : MonoBehaviourPunCallbacks
{
    //ТЕЛЕПОРТ
    [SerializeField] float teleportDelay;
    [SerializeField] float teleportCooldown;
    bool teleportIsReady = true;
    [SerializeField] GameObject teleportParticle;
    //ИНВИЗ
    [SerializeField] float invisibleDuration;
    [SerializeField] float invisibleCooldown;
    bool invisibleIsReady = true;
    [SerializeField] Material invisibleMaterial;
    [SerializeField] LayerMask defaultMask;
    [SerializeField] LayerMask invisibleMask;
    [SerializeField] SkinnedMeshRenderer mesh;
    Material defaultMaterial;
    public Transform shoot;
    public GameObject fireballPrefab;
    public GameObject electricBallPrefab; // Добавлено для ElectricBall
    PhotonDestroy photonDestroy;
    PlayerController playerController;
    PositionalSoundSync positionalSound;
    Transform marker;

    [SerializeField] float ElectricBallCooldown;
     float ElectricBallTimer;
    public bool canCastElectricBall = true;
    private void Start()
    {
        defaultMaterial = mesh.material;

        photonDestroy = GetComponent<PhotonDestroy>();
        playerController = GetComponent<PlayerController>();
        marker = playerController.marker.transform;
        positionalSound = GetComponent<PositionalSoundSync>();
    }
    public IEnumerator Teleport(Vector3 pos)
    {
        if (teleportIsReady)
        {
            teleportIsReady = false;
            positionalSound.PlaySoundAtPosition(3, transform.position);

            yield return new WaitForSeconds(teleportDelay);
            GameObject tp = PhotonNetwork.Instantiate(
          teleportParticle.name,
          transform.position,
          transform.rotation
      );
            photonDestroy.PunDestroy(tp, 1f);

            transform.position = pos;
            positionalSound.PlaySoundAtPosition(2, transform.position);
            GameObject tp2 = PhotonNetwork.Instantiate(
          teleportParticle.name,
          transform.position,
          transform.rotation
      );
            photonDestroy.PunDestroy(tp2, 1f);
            yield return new WaitForSeconds(teleportCooldown);

            teleportIsReady = true;
        }
    }
    public IEnumerator Invisible()
    {
        if (invisibleIsReady && photonView.IsMine)
        {
            photonView.RPC("SetInvision", RpcTarget.All);
            positionalSound.PlaySoundAtPosition(1, transform.position);
            yield return new WaitForSeconds(invisibleDuration);
            positionalSound.PlaySoundAtPosition(1, transform.position);
            photonView.RPC("ReSetInvision", RpcTarget.All);
            yield return new WaitForSeconds(invisibleCooldown);
            invisibleIsReady = true;
        }
    }
    [PunRPC]
    void SetInvision()
    {
        gameObject.layer = 6;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = 6;
        }
        Material defaultMaterial = mesh.material;
        invisibleIsReady = false;
        playerController.cam.cullingMask = invisibleMask;
        mesh.material = invisibleMaterial;
    }
    [PunRPC]
    void ReSetInvision()
    {
        gameObject.layer = 0;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = 0;
        }

        playerController.cam.cullingMask = defaultMask;
        mesh.material = defaultMaterial;
    }
    public void CastFireball()
    {
        if (!photonView.IsMine) return;
        // ������� �������� ��� �� ������-�������
        GameObject fireball = PhotonNetwork.Instantiate(
            fireballPrefab.name,
            shoot.position,
            transform.rotation
        );

        // ��������� ������
        Fireball fb = fireball.GetComponent<Fireball>();

        positionalSound.PlaySoundAtPosition(0, transform.position);
        fb.desiredPosition = marker.transform.position;
        fb.SetOwner(photonView.Owner.ActorNumber);
    }

    public void CastElectricBall()
    {
      
        if (!photonView.IsMine) return;
        canCastElectricBall = false;
        // Создаем электрический шар из префаба
        GameObject electricBall = PhotonNetwork.Instantiate(
            electricBallPrefab.name,
            shoot.position,
            transform.rotation
        );

        // Получаем компонент ElectricBall
        ElectricBall eb = electricBall.GetComponent<ElectricBall>();

        positionalSound.PlaySoundAtPosition(4, transform.position);
        eb.desiredPosition = marker.transform.position;
        eb.SetOwner(photonView.Owner.ActorNumber);
    }

    [PunRPC]
    void CastFireballPUN()
    {
        if (!photonView.IsMine) return;
        // ������� �������� ��� �� ������-�������
        GameObject fireball = PhotonNetwork.Instantiate(
            fireballPrefab.name,
            marker.position,
            transform.rotation
        );

        // ��������� ������
        Fireball fb = fireball.GetComponent<Fireball>();


        fb.desiredPosition = marker.transform.position;
        fb.SetOwner(photonView.Owner.ActorNumber);
    }

    [PunRPC]
    void CastElectricBallPUN()
    {
        if (!photonView.IsMine) return;
        // Создаем электрический шар из префаба
        GameObject electricBall = PhotonNetwork.Instantiate(
            electricBallPrefab.name,
            marker.position,
            transform.rotation
        );

        // Получаем компонент ElectricBall
        ElectricBall eb = electricBall.GetComponent<ElectricBall>();

        eb.desiredPosition = marker.transform.position;
        eb.SetOwner(photonView.Owner.ActorNumber);
    }
    private void FixedUpdate()
    {
       
        if (ElectricBallTimer > ElectricBallCooldown)
        {
            ElectricBallTimer = 0;
            canCastElectricBall = true;
        }
        else 
        {
            ElectricBallTimer += Time.deltaTime;
        }
    }
}