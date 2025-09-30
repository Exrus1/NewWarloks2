
using UnityEngine;

using TMPro;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text logText;
    [SerializeField] TMP_InputField inputField;
    MusicManager musicManager;

  [SerializeField]  Image blackOut;
    void Start()
    {
        StartCoroutine(BlackOut(2));
        if (Time.time < 1f)
        {
            musicManager = FindAnyObjectByType<MusicManager>();
            DontDestroyOnLoad(musicManager.gameObject);
            musicManager.PlayMusic();
        }
       
        PhotonNetwork.NickName = "Player" + Random.Range(1, 9999); 
        Log("Player Name: " + PhotonNetwork.NickName);
        PhotonNetwork.AutomaticallySyncScene = true; 
        PhotonNetwork.GameVersion = "1"; 
        PhotonNetwork.ConnectUsingSettings();
    }
    void Log(string message)
    {
        logText.text += "\n";
        logText.text += message;
    }
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 15 });
        StartCoroutine(BlackOut(0));
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnConnectedToMaster()
    {
        Log("Connected to the server");
    }
    public override void OnJoinedRoom()
    {
        Log("Joined the lobby");
        StartCoroutine(BlackOut(1));
    }
    public void ChangeName()
    {
        PhotonNetwork.NickName = inputField.text;
        Log("New Player name: " + PhotonNetwork.NickName);
    }
    IEnumerator BlackOut(int type) 
    {
        if (type != 2)
        {
            blackOut.color = new Color(0, 0, 0, 0.25f);
            while (blackOut.color.a < 1f)
            {
                blackOut.color += new Color(0, 0, 0, Time.deltaTime * 0.5f);
                yield return new WaitForSeconds(0.01f);
            }
        }
        switch (type)
        {
            case 0:
                PhotonNetwork.LoadLevel("Lobby");
             
                break;
            case 1:

                PhotonNetwork.JoinRandomRoom();
                break;
            case 2:
                blackOut.color = new Color(0, 0, 0, 1f);
                while (blackOut.color.a > 0f)
                {
                    blackOut.color -= new Color(0, 0, 0, Time.deltaTime*0.5f);
                    yield return new WaitForSeconds(0.01f);
                }
                break;
        }
    }
}
