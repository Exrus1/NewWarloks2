using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    [SerializeField] TMP_Text ChatText;
    [SerializeField] TMP_InputField InputText;
    [SerializeField] TMP_Text PlayersText;
    [SerializeField] GameObject startButton;
    [SerializeField] Image blackOut;

    SoundManager musicManager;
    readonly Dictionary<int, string> nicknames = new();

    public override void OnStartNetwork()
    {
        musicManager = FindAnyObjectByType<SoundManager>();
        Cursor.lockState = CursorLockMode.None;
        StartCoroutine(BlackOut(2));

        if (startButton != null)
            startButton.SetActive(IsServerStarted);

        NetworkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        NetworkManager.ClientManager.RegisterBroadcast<NicknameBroadcast>(OnNicknameBroadcast);
        NetworkManager.ClientManager.RegisterBroadcast<NicknameListBroadcast>(OnNicknameListBroadcast);
        NetworkManager.ServerManager.RegisterBroadcast<NicknameBroadcast>(OnServerNicknameBroadcast);
    }

    public override void OnStopNetwork()
    {
        if (NetworkManager == null)
            return;
        NetworkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        NetworkManager.ClientManager.UnregisterBroadcast<NicknameBroadcast>(OnNicknameBroadcast);
        NetworkManager.ClientManager.UnregisterBroadcast<NicknameListBroadcast>(OnNicknameListBroadcast);
        NetworkManager.ServerManager.UnregisterBroadcast<NicknameBroadcast>(OnServerNicknameBroadcast);
    }

    public override void OnStartClient()
    {
        NicknameBroadcast nb = new() { Nickname = PlayerSession.Nickname };
        NetworkManager.ClientManager.Broadcast(nb);
    }

    public override void OnStartServer()
    {
        if (IsClientStarted)
            nicknames[LocalConnection.ClientId] = PlayerSession.Nickname;
        RefreshPlayers();
    }

    void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (!IsServerStarted)
            return;

        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            string name = nicknames.TryGetValue(conn.ClientId, out string n) ? n : ("?????" + conn.ClientId);
            nicknames.Remove(conn.ClientId);
            ObserversLog(name + " ????? ?? ???????");
            RefreshPlayers();
            if (startButton != null)
                startButton.SetActive(true);
        }
        else if (args.ConnectionState == RemoteConnectionState.Started)
        {
            RefreshPlayers();
        }
    }

    void OnServerNicknameBroadcast(NetworkConnection conn, NicknameBroadcast msg, Channel channel)
    {
        string nick = string.IsNullOrWhiteSpace(msg.Nickname) ? ("?????" + conn.ClientId) : msg.Nickname.Trim();
        bool isNew = !nicknames.ContainsKey(conn.ClientId);
        nicknames[conn.ClientId] = nick;
        if (isNew)
            ObserversLog(nick + " ????? ? ???????");
        RefreshPlayers();
    }

    void OnNicknameBroadcast(NicknameBroadcast msg, Channel channel) { }

    void OnNicknameListBroadcast(NicknameListBroadcast msg, Channel channel)
    {
        nicknames.Clear();
        if (msg.ClientIds == null || msg.Nicknames == null)
            return;
        int count = Mathf.Min(msg.ClientIds.Length, msg.Nicknames.Length);
        for (int i = 0; i < count; i++)
            nicknames[msg.ClientIds[i]] = msg.Nicknames[i];
        ShowPlayersLocal();
    }

    public void StartGame()
    {
        if (!IsServerStarted)
            return;
        ObserversBattleMusic();
    }

    [ObserversRpc]
    void ObserversBattleMusic()
    {
        if (musicManager != null)
            musicManager.PlayBattleMusic();
        StartCoroutine(BlackOut(1));
    }

    public void LeaveRoom()
    {
        StartCoroutine(BlackOut(0));
    }

    void ObserversLog(string message)
    {
        ObserversShowMessage(message);
    }

    [ObserversRpc]
    void ObserversShowMessage(string message)
    {
        if (ChatText == null)
            return;
        ChatText.text += "\n";
        ChatText.text += message;
    }

    public void Send()
    {
        if (InputText == null || string.IsNullOrWhiteSpace(InputText.text))
            return;
        if (!Input.GetKeyDown(KeyCode.Return))
            return;

        string line = PlayerSession.Nickname + ": " + InputText.text;
        InputText.text = string.Empty;
        SendChatServerRpc(line);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendChatServerRpc(string message)
    {
        ObserversShowMessage(message);
    }

    void RefreshPlayers()
    {
        if (!IsServerStarted)
            return;

        List<int> ids = new();
        List<string> names = new();
        foreach (KeyValuePair<int, string> pair in nicknames)
        {
            ids.Add(pair.Key);
            names.Add(pair.Value);
        }

        NicknameListBroadcast list = new()
        {
            ClientIds = ids.ToArray(),
            Nicknames = names.ToArray()
        };
        NetworkManager.ServerManager.Broadcast(list);
        ShowPlayersLocal();
    }

    void ShowPlayersLocal()
    {
        if (PlayersText == null)
            return;
        PlayersText.text = "Игроки: ";
        foreach (KeyValuePair<int, string> pair in nicknames)
        {
            PlayersText.text += "\n";
            PlayersText.text += pair.Value;
        }
    }

    IEnumerator BlackOut(int type)
    {
        if (blackOut == null)
            yield break;

        if (type != 2)
        {
            blackOut.raycastTarget = true;
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
                NetworkSceneHelper.DisconnectAndLoadMenu();
                break;
            case 1:
                if (IsServerStarted)
                    NetworkSceneHelper.LoadGlobalReplace("Game");
                break;
            case 2:
                blackOut.raycastTarget = true;
                blackOut.color = new Color(0, 0, 0, 1f);
                while (blackOut.color.a > 0f)
                {
                    blackOut.color -= new Color(0, 0, 0, Time.deltaTime * 0.5f);
                    yield return new WaitForSeconds(0.01f);
                }
                blackOut.raycastTarget = false;
                break;
        }
    }
}
