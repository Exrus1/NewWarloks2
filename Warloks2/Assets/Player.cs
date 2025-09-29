


using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;



public class Player : MonoBehaviourPunCallbacks
{

    float health = 1000;
    bool isDead;
    [SerializeField] TMP_Text hpText;
    Rigidbody rb;
    Animator anim;
    PlayerController controller;
    ThirdPersonCamera third;
    [SerializeField]  Rigidbody[] childrenRb;
    [SerializeField] Collider[] colliders;
  
    void Start()
    {
      
        anim = GetComponent<Animator>();
        hpText.text = health.ToString();
        controller = GetComponent<PlayerController>();
        third = GetComponent<ThirdPersonCamera>();
        anim = GetComponent<Animator>();
       rb = GetComponent<Rigidbody>();  
        foreach (Rigidbody rb in childrenRb)
        {
            rb.isKinematic = true;
            
        }
        foreach (Collider col in colliders)
        {
            col.enabled = false;
           
        }
        if (!photonView.IsMine)
        {
            //������� ������ � �������� ������ � ��������� �
            transform.Find("Main Camera").gameObject.SetActive(false);
            transform.Find("Canvas").gameObject.SetActive(false);
            //��������� ������ PlayerController
            this.enabled = false;
        }
    }
  public  IEnumerator GradualDamage(float duration,float damage) 
    {
        while (duration>0) 
        { 
            duration-=0.25f;
            TakeDamage(damage);
            yield return new WaitForSeconds(0.25f);
        }
    }



    public void TakeDamage(float damage)
    {
       
        if (isDead||photonView.IsMine ) return;

        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
    }
    [PunRPC]
    void RPC_TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        hpText.text = health.ToString("f0");

        if (health <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        isDead = true;
       

            foreach (Rigidbody rb in childrenRb)
            {
                rb.isKinematic = false;
                rb.mass = 0.1f;
                
            }
            foreach (Collider col in colliders)
            {
                col.enabled = true;


            }
            GetComponent<CapsuleCollider>().radius = 0.01f;
        GetComponent<CapsuleCollider>().height = 0.01f;

        //rb.AddForce(transform.TransformDirection(new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f, 1f))) * 1f, ForceMode.Impulse);
        anim.enabled = false;

        if (photonView.IsMine)
        {
            controller.enabled = false;
            third.enabled = false;
            enabled = false;
        }
           
           
        
    }
   
 
  
}
