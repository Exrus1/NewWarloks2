using Photon.Pun;
using UnityEngine;

public class Fireball : MonoBehaviourPunCallbacks
{
    public float speed = 10f;
    public int damage = 30;
    public float lifetime = 5f;
    public GameObject explosionEffect;
    PhotonDestroy photonDestroy;
    private int ownerId;

  public  Vector3 desiredPosition;
    void Start()
    {
        photonDestroy = GetComponent<PhotonDestroy>();
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

        // ���������� ������������ � �����
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.Owner.ActorNumber == ownerId)
                return;
        }

        // ������� ���� ������
        if (other.CompareTag("Player"))
        {
            Player health = other.GetComponent<Player>();
            Debug.Log(other);
            if (health != null)
            {
                health.TakeDamage(damage, ownerId);
                photonDestroy.PunDestroy(gameObject, 0.2f);
            }
        }

        // ������� ������ ������


      
    }
}
