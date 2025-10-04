using System.Collections;
using Photon.Pun;
using UnityEngine;

public  class PhotonDestroy : MonoBehaviourPunCallbacks
{
    public  void PunDestroy(GameObject go,float timeToKill) 
    {

        StartCoroutine(Destr(go, timeToKill));
    }
    IEnumerator Destr(GameObject go, float timeToKill)
    {
        yield return new WaitForSeconds(timeToKill);
        PhotonNetwork.Destroy(go);
    }
}
