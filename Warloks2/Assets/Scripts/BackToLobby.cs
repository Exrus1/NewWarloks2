using Photon.Pun;
using UnityEngine;

public class BackToLobby : MonoBehaviourPunCallbacks
{
   
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            gameObject.SetActive(false);
        }
    }

    public void ChangeScene() 
    {
        PhotonNetwork.LoadLevel("Lobby");
    }
}
