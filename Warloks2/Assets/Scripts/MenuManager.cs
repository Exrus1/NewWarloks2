using System.Collections;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TMP_Text logText;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_InputField ipField;
    [SerializeField] Image blackOut;
    [SerializeField] Button serverButton;

    SoundManager musicManager;
    bool ready;
    NetworkManager networkManager;
    bool pendingFadeToLobby;

    void Start()
    {
        StartCoroutine(BlackOut(2));
        if (Time.time < 1f)
        {
            musicManager = FindAnyObjectByType<SoundManager>();
            if (musicManager != null)
            {
                DontDestroyOnLoad(musicManager.gameObject);
                musicManager.PlayMusic();
            }
        }

        EnsureNetworkManager();
        PlayerSession.Nickname = PlayerSession.Nickname;
        if (inputField != null && string.IsNullOrWhiteSpace(inputField.text))
            inputField.text = PlayerSession.Nickname;

        Log("Ваше имя: " + PlayerSession.Nickname);
        Log($"Порт FishNet/Tugboat: {PlayerSession.DefaultPort} (Radmin VPN — укажите IP хоста)");
        ready = true;
    }

    void EnsureNetworkManager()
    {
        networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null)
            networkManager = FindAnyObjectByType<NetworkManager>();

        if (networkManager == null)
        {
            GameObject go = new GameObject("NetworkManager");
            networkManager = go.AddComponent<NetworkManager>();
            go.AddComponent<FishNet.Transporting.Tugboat.Tugboat>();
            DontDestroyOnLoad(go);
            Log("NetworkManager создан в runtime. Добавьте префаб в Menu для продакшена.");
        }

        networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;
        networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
    }

    void OnDestroy()
    {
        if (networkManager == null)
            return;
        networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
        networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
    }

    void OnClientConnectionState(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Log("Клиент подключён");
            if (pendingFadeToLobby)
            {
                // ClientHost / client: DefaultScene usually loads Lobby; ensure fallback.
                if (networkManager.GetComponent<FishNet.Component.Scenes.DefaultScene>() == null && networkManager.IsServerStarted)
                    NetworkSceneHelper.LoadGlobalReplace("Lobby");
                StartCoroutine(BlackOut(1));
            }
        }
        else if (args.ConnectionState == LocalConnectionState.Stopped)
        {
            Log("Клиент отключён");
        }
    }

    void OnServerConnectionState(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == LocalConnectionState.Started)
        {
            Log("Сервер запущен");
            if (pendingFadeToLobby)
            {
                if (networkManager.GetComponent<FishNet.Component.Scenes.DefaultScene>() == null)
                    NetworkSceneHelper.LoadGlobalReplace("Lobby");
                if (!networkManager.IsClientStarted)
                    StartCoroutine(BlackOut(1));
            }
        }
    }

    void Log(string message)
    {
        if (logText == null)
            return;
        logText.text += "\n";
        logText.text += message;
    }

    /// <summary>Host = listen server + local client. Wired from former Create lobby button.</summary>
    public void CreateRoom() => StartHost();

    public void StartHost()
    {
        if (!ready || networkManager == null)
            return;

        ApplyNickname();
        pendingFadeToLobby = true;
        EnsurePort();
        if (!networkManager.IsServerStarted)
            networkManager.ServerManager.StartConnection();
        if (!networkManager.IsClientStarted)
            networkManager.ClientManager.StartConnection();
        StartCoroutine(BlackOut(0));
    }

    /// <summary>Dedicated server (no local player).</summary>
    public void StartDedicatedServer()
    {
        if (!ready || networkManager == null)
            return;

        ApplyNickname();
        pendingFadeToLobby = true;
        EnsurePort();
        if (!networkManager.IsServerStarted)
            networkManager.ServerManager.StartConnection();
        StartCoroutine(BlackOut(0));
    }

    /// <summary>Client join by Radmin IP. Wired from former Join lobby button.</summary>
    public void JoinRoom() => StartClient();

    public void StartClient()
    {
        if (!ready || networkManager == null)
            return;

        string ip = ipField != null ? ipField.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(ip))
        {
            Log("Введите IP хоста (Radmin)");
            return;
        }

        ApplyNickname();
        pendingFadeToLobby = true;
        EnsurePort();
        networkManager.TransportManager.Transport.SetClientAddress(ip);
        Log("Подключение к " + ip + ":" + PlayerSession.DefaultPort);
        if (!networkManager.IsClientStarted)
            networkManager.ClientManager.StartConnection();
        StartCoroutine(BlackOut(0));
    }

    void EnsurePort()
    {
        networkManager.TransportManager.Transport.SetPort(PlayerSession.DefaultPort);
    }

    void ApplyNickname()
    {
        if (inputField != null && !string.IsNullOrWhiteSpace(inputField.text))
            PlayerSession.Nickname = inputField.text.Trim();
    }

    public void ChangeName()
    {
        if (inputField == null || string.IsNullOrWhiteSpace(inputField.text))
            return;
        PlayerSession.Nickname = inputField.text.Trim();
        Log("Ваше новое имя: " + PlayerSession.Nickname);
    }

    IEnumerator BlackOut(int type)
    {
        if (!ready && type != 2)
            yield break;

        if (blackOut == null)
            yield break;

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
                // Fade only; DefaultScene / Lobby load happens via NetworkManager connection.
                break;
            case 1:
                break;
            case 2:
                blackOut.color = new Color(0, 0, 0, 1f);
                while (blackOut.color.a > 0f)
                {
                    blackOut.color -= new Color(0, 0, 0, Time.deltaTime);
                    yield return new WaitForSeconds(0.01f);
                }
                blackOut.raycastTarget = false;
                break;
        }
    }
}

