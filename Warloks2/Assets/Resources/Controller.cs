using UnityEngine;

using TMPro;

public class Controller : MonoBehaviour
{
    [SerializeField] float movementSpeed = 5f;

    float currentSpeed;

    [SerializeField] Rigidbody rb;
    Vector3 direction;

    [SerializeField] float shiftSpeed = 10f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float stamina = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    
        currentSpeed = movementSpeed;
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        direction = new Vector3(moveHorizontal, 0.0f, moveVertical);
        direction = transform.TransformDirection(direction);
     
   
      
       
      
    }


    void FixedUpdate()
    {
        rb.MovePosition(transform.position + direction * currentSpeed * Time.deltaTime);
    }
  
}