
using UnityEngine;

using TMPro;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;



public class MenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text logText;
    [SerializeField] TMP_InputField inputField;
    SoundManager musicManager;
    bool connect=false;
  [SerializeField]  Image blackOut;
    void Start()
    {
        StartCoroutine(BlackOut(2));
        if (Time.time < 1f)
        {
            musicManager = FindAnyObjectByType<SoundManager>();
            DontDestroyOnLoad(musicManager.gameObject);
            musicManager.PlayMusic();
        }
       
        PhotonNetwork.NickName = "Игрок" + Random.Range(1, 9999); 
        Log("Ваше имя: " + PhotonNetwork.NickName);
        PhotonNetwork.AutomaticallySyncScene = true; 
        PhotonNetwork.GameVersion = "1";
        
            PhotonNetwork.ConnectUsingSettings();
        
    }
  
        //if (!PhotonNetwork.IsConnected)
        //{
        //    PhotonNetwork.ConnectUsingSettings();
        //    print("a");
        //}
   
    void Log(string message)
    {
        logText.text += "\n";
        logText.text += message;
    }
    public void CreateRoom()
    {
        if (connect)
        {
            PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 15 });
            StartCoroutine(BlackOut(0));
        }
    }
    public void JoinRoom()
    {
        if (connect)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }
    public override void OnConnectedToMaster()
    {
        connect = true;
        Log("Подключен к серверу");
    }
    public override void OnJoinedRoom()
    {
        Log("Вход в лобби");
        StartCoroutine(BlackOut(1));
    }
    public void ChangeName()
    {
        PhotonNetwork.NickName = inputField.text;
        Log("Ваше новое имя: " + PhotonNetwork.NickName);
    }
    IEnumerator BlackOut(int type) 
    {if (connect)
        {
            blackOut.raycastTarget = true;
            if (type != 2)
            {
                blackOut.color = new Color(0, 0, 0, 0.25f);
                while (blackOut.color.a < 1f)
                {
                    blackOut.color += new Color(0, 0, 0, Time.deltaTime);
                    yield return new WaitForSeconds(0.01f);
                }
            }
            switch (type)
            {
                case 0:
                    PhotonNetwork.LoadLevel("Lobby");

                    break;
                case 1:

                    //PhotonNetwork.JoinRandomRoom();
                    break;
                case 2:
                    blackOut.color = new Color(0, 0, 0, 1f);
                    while (blackOut.color.a > 0f)
                    {
                        blackOut.color -= new Color(0, 0, 0, Time.deltaTime);
                        yield return new WaitForSeconds(0.01f);
                    }
                    break;
            }
           
        }
    }
}
