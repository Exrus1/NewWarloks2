using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerSkills : MonoBehaviourPunCallbacks
{
    [SerializeField] float teleportDelay;
    [SerializeField] float teleportCooldown;
    [SerializeField] bool teleportIsReady=true;
    [SerializeField] GameObject teleportParticle;
    PhotonDestroy photonDestroy;
    private void Start()
    {
        photonDestroy = GetComponent<PhotonDestroy>();
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

}
