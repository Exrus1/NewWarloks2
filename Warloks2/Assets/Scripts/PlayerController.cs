using System.Collections;
using FishNet.Object;
using UnityEngine;

public class PlayerController : NetworkBehaviour
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

    bool isCastingElectricBall;
    Coroutine electricBallCoroutine;

    PositionalSoundSync positionalSound;
    AudioSource footSound;
    CapsuleCollider capsuleCollider;
    Coroutine jumpCoroutine;

    void ShootRayFromCenter()
    {
        if (cam == null || marker == null)
            return;
        Vector3 screenCenter = new(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = cam.ScreenPointToRay(screenCenter);
        LayerMask layerMask = ~(1 << 2);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            marker.transform.position = hit.point;
    }

    public override void OnStartClient()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        Transform camTransform = transform.Find("Main Camera");
        if (camTransform != null)
            cam = camTransform.GetComponent<Camera>();

        if (IsOwner)
            skills = GetComponent<PlayerSkills>();
        else
        {
            if (marker != null)
            {
                MeshRenderer mr = marker.GetComponent<MeshRenderer>();
                if (mr != null)
                    mr.enabled = false;
            }
            enabled = false;
            return;
        }

        currentSpeed = movementSpeed;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        footSound = GetComponent<AudioSource>();
        positionalSound = GetComponent<PositionalSoundSync>();
    }

    void Update()
    {
        if (!IsOwner)
            return;

        ShootRayFromCenter();
        if (isCastingElectricBall)
            return;

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        direction = new Vector3(moveHorizontal, 0f, moveVertical);
        direction = transform.TransformDirection(direction);
        if (anim != null)
        {
            anim.SetFloat("h", moveHorizontal);
            anim.SetFloat("v", moveVertical);
        }

        if ((moveHorizontal != 0 || moveVertical != 0) && footSound != null && !footSound.isPlaying && isGrounded && positionalSound != null)
            positionalSound.PlayFootSound();

        if (Input.GetKeyDown(ControlsManager.Jump) && isGrounded && rb != null)
        {
            isGrounded = false;
            if (anim != null)
                anim.SetBool("jump", true);
            if (jumpCoroutine != null)
                StopCoroutine(jumpCoroutine);
            jumpCoroutine = StartCoroutine(FloatingTrue());
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }

        if (Input.GetKeyDown(ControlsManager.Fireball) && CanCast() && anim != null)
            anim.SetBool("cast", true);

        if (Input.GetKeyDown(ControlsManager.Teleport) && skills != null && marker != null)
            skills.StartCoroutine(skills.Teleport(marker.transform.position));

        if (Input.GetKeyDown(ControlsManager.Invisible) && skills != null)
            skills.StartCoroutine(skills.Invisible());

        if (Input.GetKeyDown(ControlsManager.ElectricBall) && !isCastingElectricBall && skills != null && skills.canCastElectricBall && CanCast() && anim != null)
            anim.SetBool("electricBallCast", true);

        if (Input.GetKeyDown(ControlsManager.WaterExplosion) && CanCast() && skills != null && skills.canCastWaterExplosion && anim != null)
            anim.SetBool("waterExplosionCast", true);
    }

    bool CanCast()
    {
        if (anim == null)
            return false;
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(1);
        return stateInfo.IsTag("TopBlendTree");
    }

    public void StartElectricBallCast()
    {
        if (!IsOwner || isCastingElectricBall)
            return;
        isCastingElectricBall = true;
        currentSpeed = 0f;
        if (anim != null)
        {
            anim.SetFloat("h", 0);
            anim.SetFloat("v", 0);
        }
        electricBallCoroutine = StartCoroutine(ElectricBallCastRoutine());
    }

    IEnumerator ElectricBallCastRoutine()
    {
        float castTime = 5f;
        float ballInterval = 0.35f;
        float timer = 0f;
        float nextBallTime = 0f;
        while (timer < castTime)
        {
            timer += Time.deltaTime;
            if (timer >= nextBallTime)
            {
                if (skills != null)
                    skills.CastElectricBall();
                nextBallTime += ballInterval;
            }
            yield return null;
        }
        isCastingElectricBall = false;
        currentSpeed = movementSpeed;
        if (anim != null)
            anim.SetBool("electricBallCast", false);
    }

    IEnumerator FloatingTrue()
    {
        yield return new WaitForSeconds(0.75f);
        if (!isGrounded && anim != null)
            anim.SetBool("floating", true);
    }

    public void SwitchToMain()
    {
        if (anim == null)
            return;
        anim.SetBool("cast", false);
        anim.SetBool("waterExplosionCast", false);
    }

    void FixedUpdate()
    {
        if (!IsOwner || isCastingElectricBall || rb == null)
            return;
        rb.MovePosition(rb.position + direction * currentSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner)
            return;
        isGrounded = true;
        if (anim != null)
        {
            anim.SetBool("jump", false);
            anim.SetBool("floating", false);
        }
    }
}
