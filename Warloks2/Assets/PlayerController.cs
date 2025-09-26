
using Photon.Pun;

using UnityEngine;


public class PlayerController : MonoBehaviourPunCallbacks
{
    public float cooldown = 2f;
    public GameObject fireballPrefab;
    public Transform spawnPoint;
    float currentSpeed;
    private float lastCastTime;
    Vector3 direction;
    Rigidbody rb;
    bool isGrounded = true;
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] Animator anim;
    [SerializeField] float shiftSpeed = 10f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float stamina = 5f;
    Camera cam;
    public LayerMask playerLayer;
    public GameObject marker;
    [SerializeField] Transform spine;
    void ShootRayFromCenter()
    {
       
            // Получаем центр экрана
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            Ray ray = cam.ScreenPointToRay(screenCenter);

            // Создаем маску, исключающую слой игрока (используем побитовое НЕ ~)
            LayerMask layerMask = ~playerLayer;

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                marker.transform.position = hit.point;

                // Дополнительные действия при попадании
            }
        
       
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (photonView.IsMine)
        {
            cam = transform.Find("Main Camera").gameObject.GetComponent<Camera>();
        }
        else
        {
                marker.GetComponent<MeshRenderer>().enabled = false;
                enabled = false;
        }
        currentSpeed = movementSpeed;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
       
            ShootRayFromCenter();
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            direction = new Vector3(moveHorizontal, 0.0f, moveVertical);
            direction = transform.TransformDirection(direction);
            anim.SetFloat("h", moveHorizontal);
            anim.SetFloat("v", moveVertical);
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {

                isGrounded = false;
                anim.SetBool("jump", true);


            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                //dash
            }
            if (Input.GetMouseButtonDown(0) && !anim.GetBool("cast"))
            {
                anim.SetBool("cast", true);

            }
        
    }
    bool CanCast()
    {
        return Time.time - lastCastTime >= cooldown;
    }
  public  void CastFireball()
    {
        if (!photonView.IsMine) return;
        // ������� �������� ��� �� ������-�������
        GameObject fireball = PhotonNetwork.Instantiate(
            fireballPrefab.name,
            spawnPoint.position,
            transform.rotation
        );

        // ��������� ������
        Fireball fb = fireball.GetComponent<Fireball>();


        fb.desiredPosition = marker.transform.position;
        fb.SetOwner(photonView.Owner.ActorNumber);
    }


    [PunRPC]
    void CastFireballPUN()
    {
        if (!photonView.IsMine) return;
        // ������� �������� ��� �� ������-�������
        GameObject fireball = PhotonNetwork.Instantiate(
            fireballPrefab.name,
            spawnPoint.position,
            transform.rotation
        );

        // ��������� ������
        Fireball fb = fireball.GetComponent<Fireball>();
       
           
            fb.desiredPosition = marker.transform.position;
            fb.SetOwner(photonView.Owner.ActorNumber);
        
    }
    public void AddForceJump()
    {
        rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
    }
    public void SwitchToMain()
    {
        anim.SetBool("cast", false);


    }

    public void FloatingTrue()
    {
        anim.SetBool("floating", true);
    }
    void FixedUpdate()
    {
        rb.MovePosition(transform.position + direction * currentSpeed * Time.deltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        anim.SetBool("jump", false);
        anim.SetBool("floating", false);
    }

}
