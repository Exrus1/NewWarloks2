using UnityEngine;

using Photon.Pun;

using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text ChatText;
    [SerializeField] TMP_InputField InputText;
    [SerializeField] TMP_Text PlayersText;

    [SerializeField] GameObject startButton;
    MusicManager musicManager;
    [SerializeField] Image blackOut;
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        StartCoroutine(BlackOut(2));
        musicManager = FindAnyObjectByType<MusicManager>();
        RefreshPlayers();
        if (!PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(false);
        }
    }
    public void StartGame()
    {
        photonView.RPC("BattleMusic", RpcTarget.All);

      
     
    }
    [PunRPC]
    public void BattleMusic()
    {
        musicManager.PlayBattleMusic();
        StartCoroutine(BlackOut(1));

    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Log(otherPlayer.NickName + " left the room");
        RefreshPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Log(newPlayer.NickName + " entered the room");
        RefreshPlayers();
    }
    
 
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        StartCoroutine(BlackOut(0));
    }
    void Log(string message)
    {
        ChatText.text += "\n";
        ChatText.text += message;
    }
   
    public override void OnLeftRoom()
    {
       
    }
    
    [PunRPC]
    public void ShowMessage(string message, PhotonMessageInfo info)
    {
        ChatText.text += "\n";
        ChatText.text += message;
    }
    public void Send()
    {
        if (string.IsNullOrWhiteSpace(InputText.text)) { return; }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            photonView.RPC("ShowMessage", RpcTarget.All, PhotonNetwork.NickName + ": " + InputText.text);
            InputText.text = string.Empty;
        }
    }
    void RefreshPlayers()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ShowPlayers", RpcTarget.All);
        }
    }

    [PunRPC]
    public void ShowPlayers()
    {
        PlayersText.text = "Players: ";
        foreach (Photon.Realtime.Player otherPlayer in PhotonNetwork.PlayerList)
        {
            PlayersText.text += "\n";
            PlayersText.text += otherPlayer.NickName;
        }
    }
    IEnumerator BlackOut(int type)
    {
        if (type!=2)
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
                SceneManager.LoadScene(0);
                break;
            case 1:
                PhotonNetwork.LoadLevel("Game");
                break;
            case 2:
                blackOut.color = new Color(0, 0, 0, 1f);
                while (blackOut.color.a > 0f)
                {
                    blackOut.color -= new Color(0, 0, 0, Time.deltaTime * 0.5f);
                    yield return new WaitForSeconds(0.01f);
                }
                break;
        }
       

    }
}
