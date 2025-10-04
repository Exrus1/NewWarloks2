using System.Collections;
using Photon.Pun;

using UnityEngine;



public class PlayerController : MonoBehaviourPunCallbacks
{
    public float cooldown = 2f;
    public GameObject fireballPrefab;

    float currentSpeed;

    Vector3 direction;
    Rigidbody rb;
    bool isGrounded = true;
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] Animator anim;
    [SerializeField] float shiftSpeed = 10f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float stamina = 5f;
    public Camera cam;
    public LayerMask playerLayer;
    public GameObject marker;
    [SerializeField] Transform spine;
    PlayerSkills skills;


    private bool isCastingElectricBall = false;
    private Coroutine electricBallCoroutine;

    

    PositionalSoundSync positionalSound;
    AudioSource footSound;

    CapsuleCollider capsuleCollider;
    Coroutine jumpCoroutine;
    void ShootRayFromCenter()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);
        LayerMask layerMask = ~(1 << 2);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            marker.transform.position = hit.point;
        }
    }

    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        cam = transform.Find("Main Camera").gameObject.GetComponent<Camera>();
        if (photonView.IsMine)
        {
            skills = GetComponent<PlayerSkills>();
        }
        else
        {
            marker.GetComponent<MeshRenderer>().enabled = false;
            enabled = false;
        }
        currentSpeed = movementSpeed;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        footSound = GetComponent<AudioSource>();
        positionalSound = GetComponent<PositionalSoundSync>();
    }

    // Update is called once per frame
    void Update()
    {
        ShootRayFromCenter();
        if (isCastingElectricBall) return;
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        direction = new Vector3(moveHorizontal, 0.0f, moveVertical);
        direction = transform.TransformDirection(direction);
        anim.SetFloat("h", moveHorizontal);
        anim.SetFloat("v", moveVertical);
        if ((moveHorizontal != 0 || moveVertical != 0) && !footSound.isPlaying && isGrounded)
        {
            positionalSound.PlayFootSound();
        }
        if (Input.GetKeyDown(ControlsManager.Jump) && isGrounded)
        {
            isGrounded = false;
            anim.SetBool("jump", true);
            if (jumpCoroutine != null)
            {
                StopCoroutine(jumpCoroutine);
            }
            jumpCoroutine = StartCoroutine(FloatingTrue());
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
           
        }
        if (Input.GetKeyDown(ControlsManager.Fireball) && CanCast())
        {
            anim.SetBool("cast", true);

        }
        if (Input.GetKeyDown(ControlsManager.Teleport))
        {
            skills.StartCoroutine(skills.Teleport(marker.transform.position));

        }
        if (Input.GetKeyDown(ControlsManager.Invisible))
        {
            skills.StartCoroutine(skills.Invisible());
        }
        if (Input.GetKeyDown(ControlsManager.ElectricBall) && !isCastingElectricBall&&skills.canCastElectricBall && CanCast())
        {
            anim.SetBool("electricBallCast", true);
        }
        // В методе Update PlayerController добавить:
        if (Input.GetKeyDown(ControlsManager.WaterExplosion) && CanCast()&&skills.canCastWaterExplosion)
        {
            anim.SetBool("waterExplosionCast", true);
        }
    }
    private bool CanCast() 
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(1);
        bool isInBlendTreeState = stateInfo.IsTag("TopBlendTree");
        return isInBlendTreeState;
    }
    
 public   void StartElectricBallCast()
    {
        if (isCastingElectricBall) return;
        isCastingElectricBall = true;
        currentSpeed = 0f; // Блокируем движение
        anim.SetFloat("h", 0);
        anim.SetFloat("v", 0);
        electricBallCoroutine = StartCoroutine(ElectricBallCastRoutine());
    }

    // Добавлено для Electric Ball
    IEnumerator ElectricBallCastRoutine()
    {
        float castTime = 5f;
        float ballInterval = 0.35f;
        float timer = 0f;
        float nextBallTime = 0f;
        while (timer < castTime)
        {
            timer += Time.deltaTime;

            // Выпускаем Electric Ball каждые 0.5 секунды
            if (timer >= nextBallTime)
            {
                skills.CastElectricBall();
                nextBallTime += ballInterval;
            }

            yield return null;
        }
        isCastingElectricBall = false;
        currentSpeed = movementSpeed; // Восстанавливаем скорость
        anim.SetBool("electricBallCast", false);
    }

    IEnumerator FloatingTrue()
    {
        yield return new WaitForSeconds(0.75f);
        if (!isGrounded)
        {
            anim.SetBool("floating", true);
        }
    }

    public void SwitchToMain()
    {
        anim.SetBool("cast", false);
        anim.SetBool("waterExplosionCast", false);
    }

    void FixedUpdate()
    {   
        if (isCastingElectricBall) return;

        rb.MovePosition(rb.position + direction * currentSpeed * Time.deltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        anim.SetBool("jump", false);
        anim.SetBool("floating", false);
    }
}