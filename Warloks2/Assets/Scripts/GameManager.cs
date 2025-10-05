using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public float gravity;
   SoundManager musicManager;
    void Start()
    {
        musicManager = FindAnyObjectByType<SoundManager>();
        Physics.gravity = new Vector3(0, -9.81f * gravity, 0);
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
            new Vector3(Random.Range(-25,25),50, Random.Range(25, 80)),
            Quaternion.identity
        );

      
    }

    public void EndGame()
    {
        musicManager.PlayMusic();
        PhotonNetwork.LoadLevel("Lobby");
    }
}
