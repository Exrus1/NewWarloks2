using Photon.Pun;
using UnityEngine;

public class Fireball : MonoBehaviourPunCallbacks
{
    public float speed = 10f;
    public int damage = 30;
    public float lifetime = 5f;
    public GameObject explosionEffect;

    private int ownerId;

  public  Vector3 desiredPosition;
    void Start()
    {
        if (!photonView.IsMine)
        { 
            enabled = false;
        }
        Invoke("Destr", lifetime);
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
                health.TakeDamage(damage, ownerId);
            }
        }

        // Создаем эффект взрыва


        PhotonNetwork.Destroy(gameObject);
    }
    private void Destr()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
