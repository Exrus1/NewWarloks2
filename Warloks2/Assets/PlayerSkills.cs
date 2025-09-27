using System.Collections;
using Photon.Pun;
using UnityEngine;


public class PlayerSkills : MonoBehaviourPunCallbacks
{
    //“≈À≈œŒ–“
    [SerializeField] float teleportDelay;
    [SerializeField] float teleportCooldown;
    bool teleportIsReady=true;
    [SerializeField] GameObject teleportParticle;
    //»Õ¬»«
    [SerializeField] float invisibleDuration;
    [SerializeField] float invisibleCooldown;
     bool  invisibleIsReady = true;
    [SerializeField] Material invisibleMaterial;
    [SerializeField] LayerMask defaultMask;
    [SerializeField] LayerMask invisibleMask;
    [SerializeField] SkinnedMeshRenderer mesh;
    Material defaultMaterial;


    PhotonDestroy photonDestroy;
    PlayerController playerController;

    private void Start()
    {
        defaultMaterial = mesh.material;

        photonDestroy = GetComponent<PhotonDestroy>();
        playerController = GetComponent<PlayerController>();
    }
    public IEnumerator Teleport(Vector3 pos) 
    {if (teleportIsReady)
        {
            teleportIsReady = false;
         
            yield return new WaitForSeconds(teleportDelay);
            GameObject tp = PhotonNetwork.Instantiate(
          teleportParticle.name,
          transform.position,
          transform.rotation
      );
            photonDestroy.PunDestroy(tp, 3);

            transform.position = pos;
           
            GameObject tp2 = PhotonNetwork.Instantiate(
          teleportParticle.name,
          transform.position,
          transform.rotation
      );
            photonDestroy.PunDestroy(tp2, 3);
            yield return new WaitForSeconds(teleportCooldown);
            
            teleportIsReady = true;
        }
    }
    public IEnumerator Invisible()
    {
        if (invisibleIsReady&&photonView.IsMine)
        {
            photonView.RPC("SetInvision", RpcTarget.All);
            
            yield return new WaitForSeconds(invisibleDuration);
            photonView.RPC("ReSetInvision", RpcTarget.All);
            yield return new WaitForSeconds(invisibleCooldown);
            invisibleIsReady = true;
        }
    }
    [PunRPC]
     void SetInvision()
    {
        gameObject.layer = 6;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = 6;
        }
        Material defaultMaterial = mesh.material;
        invisibleIsReady = false;
        playerController.cam.cullingMask = invisibleMask;
        mesh.material = invisibleMaterial;
    }
    [PunRPC]
    void ReSetInvision()
    {
        gameObject.layer = 0;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = 0;
        }
        playerController.cam.cullingMask = defaultMask;
        mesh.material = defaultMaterial;
    }


}
