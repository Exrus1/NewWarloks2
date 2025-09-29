using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public float gravity;
   MusicManager musicManager;
    void Start()
    {
        musicManager = FindAnyObjectByType<MusicManager>();
        Physics.gravity = new Vector3(0, Physics.gravity.y * gravity, 0);
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
    }

    [Header("Spawn Settings")]
    public GameObject playerPrefab;
  
  
    public override void OnJoinedRoom()
    {
    
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            new Vector3(Random.value*10, Random.value * 10, Random.value * 10),
            Quaternion.identity
        );

      
    }

    public void EndGame()
    {
        photonView.RPC("PlayMenuMusic", RpcTarget.All);
        PhotonNetwork.LoadLevel("Lobby");
    }
    public void PlayMenuMusic()
    {
        musicManager.PlayMusic();
       
    }

}
