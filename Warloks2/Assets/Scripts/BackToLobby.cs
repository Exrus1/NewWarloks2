using FishNet.Object;
using UnityEngine;

public class BackToLobby : NetworkBehaviour
{
    public override void OnStartClient()
    {
        if (!IsServerStarted)
            gameObject.SetActive(false);
    }

    public void ChangeScene()
    {
        if (!IsServerStarted)
            return;
        NetworkSceneHelper.LoadGlobalReplace("Lobby");
    }
}
