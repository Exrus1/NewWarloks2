using Photon.Pun;
using UnityEngine;

public class ElectricBall : MonoBehaviourPunCallbacks
{
    public float speed = 10f;
    public float damage = 30;
    public float lifetime = 5f;
    public GameObject explosionEffect;
    PhotonDestroy photonDestroy;
    private int ownerId;

    public Vector3 desiredPosition;
    private Vector3 flightDirection; // Добавлено для направления полета
    private bool isMoving = true; // Добавлено для контроля движения

    void Start()
    {
        photonDestroy = GetComponent<PhotonDestroy>();

        if (!photonView.IsMine)
        {
            enabled = false;
        }
        photonDestroy.PunDestroy(gameObject, lifetime);

        // Вычисляем направление полета к desiredPosition
        flightDirection = (desiredPosition - transform.position).normalized;
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        // Двигаемся по направлению к desiredPosition, но продолжаем лететь дальше
        transform.position += flightDirection * speed * Time.deltaTime;
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

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }

        // Активируем эффект взрыва и останавливаем движение
        explosionEffect.SetActive(true);
       
        GetComponent<SphereCollider>().enabled = false;
        isMoving = false; // Останавливаем движение
        photonDestroy.PunDestroy(gameObject, 0.5f);
    }
}