using Photon.Pun;
using UnityEngine;

public class Fireball : MonoBehaviourPunCallbacks
{
    public float speed = 10f;
    public float damage = 30;
    public float lifetime = 5f;
    public GameObject explosionEffect;
    PhotonDestroy photonDestroy;
    private int ownerId;
    [SerializeField] GameObject[] particles;
    public  Vector3 desiredPosition;
    void Start()
    {
        photonDestroy = GetComponent<PhotonDestroy>();
       
        if (!photonView.IsMine)
        { 
            enabled = false;
        }
        photonDestroy.PunDestroy(gameObject, lifetime);
       
    }
 
    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position,desiredPosition,speed*Time.deltaTime);
    }
    public void SetOwner(int playerId)
    {
        ownerId = playerId;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        // Игнорируем столкновение с собой
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.Owner.ActorNumber == ownerId)
                return;
        }

        // Наносим урон игроку
        if (other.CompareTag("Player"))
        {
            Player health = other.GetComponent<Player>();
            Debug.Log(other);
            if (health != null)
            {
                health.StartCoroutine(health.GradualDamage(lifetime,damage));
                SetNetworkParent(other.transform);
                GetComponent<BoxCollider>().enabled = false;
               
               
            }
        }
    }
 
    [PunRPC]
    void RPC_SetParent(int parentViewId)
    {
        if (parentViewId == -1)
        {
            // Устанавливаем родителя в null
            transform.SetParent(null);
            return;
        }

        PhotonView parentView = PhotonView.Find(parentViewId);
        if (parentView != null)
        {
            transform.SetParent(parentView.transform);
            transform.localPosition = new Vector3(Random.Range(-0.15f,0.15f), Random.Range(0.5f, 1.5f), Random.Range(-0.15f, 0.15f));
        }
        transform.localScale = Vector3.one * 0.5f;
        foreach (GameObject particle in particles)
        {
            particle.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
        enabled = false;
    }

    public void SetNetworkParent(Transform newParent)
    {
        int parentViewId = newParent != null ? newParent.GetComponent<PhotonView>().ViewID : -1;

        // Локальное применение
        transform.SetParent(newParent);

        // Синхронизация с другими клиентами
        photonView.RPC("RPC_SetParent", RpcTarget.All, parentViewId);
    }
}
