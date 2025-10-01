
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
  public  Camera cam;
    public LayerMask playerLayer;
    public GameObject marker;
    [SerializeField] Transform spine;
    PlayerSkills skills;
    

    public KeyCode Jump;
    public KeyCode Fireball;
    public KeyCode Invisible;
    public KeyCode Teleport;

    PositionalSoundSync positionalSound;
    AudioSource footSound;

 CapsuleCollider capsuleCollider;
    Coroutine jumpCoroutine;
    void ShootRayFromCenter()
    {
       
            // Получаем центр экрана
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
            Ray ray = cam.ScreenPointToRay(screenCenter);

           
            LayerMask layerMask =  ~(1 << 2);

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
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");


       

        direction = new Vector3(moveHorizontal, 0.0f, moveVertical);
        direction = transform.TransformDirection(direction);





        anim.SetFloat("h", moveHorizontal);
            anim.SetFloat("v", moveVertical);
        if ((moveHorizontal!=0||moveVertical!=0)&&!footSound.isPlaying&&isGrounded) 
        {
            positionalSound.PlayFootSound();
        }
            if (Input.GetKeyDown(Jump) && isGrounded)
            {

                isGrounded = false;
                anim.SetBool("jump", true);
            if (jumpCoroutine!= null) 
            {
                StopCoroutine(jumpCoroutine);
            }
             
              jumpCoroutine =  StartCoroutine(FloatingTrue());


            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                //dash
            }
            if (Input.GetKeyDown(Fireball) && !anim.GetBool("cast"))
            {
                anim.SetBool("cast", true);
             
            }
            if (Input.GetKeyDown(Teleport)) 
             {
               skills.StartCoroutine(skills.Teleport(marker.transform.position));
            
             }
        if (Input.GetKeyDown(Invisible))
        {
            skills.StartCoroutine(skills.Invisible());
        }

    }
    IEnumerator FloatingTrue()
    {
        yield return new WaitForSeconds(0.75f);
        if (!isGrounded) 
        {
            anim.SetBool("floating", true);
        }
    }
 
  
    //public void AddForceJump()
    //{
    //    rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
    //}
    public void SwitchToMain()
    {
        anim.SetBool("cast", false);


    }

    //public void FloatingTrue()
    //{
    //    anim.SetBool("floating", true);
    //}
    void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * currentSpeed * Time.deltaTime);
    }
    void OnCollisionEnter(Collision collision)
    {
       
        isGrounded = true;
        anim.SetBool("jump", false);
        anim.SetBool("floating", false);
    }

}
