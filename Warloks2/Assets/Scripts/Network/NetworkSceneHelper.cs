using FishNet;
using FishNet.Managing.Scened;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public static class NetworkSceneHelper
{
    public static void LoadGlobalReplace(string sceneName)
    {
        if (InstanceFinder.NetworkManager == null || !InstanceFinder.IsServerStarted)
        {
            UnitySceneManager.LoadScene(sceneName);
            return;
        }

        SceneLoadData sld = new(sceneName)
        {
            ReplaceScenes = ReplaceOption.All
        };
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    public static void DisconnectAndLoadMenu()
    {
        if (InstanceFinder.NetworkManager != null)
        {
            if (InstanceFinder.IsClientStarted)
                InstanceFinder.ClientManager.StopConnection();
            if (InstanceFinder.IsServerStarted)
                InstanceFinder.ServerManager.StopConnection(true);
        }

        UnitySceneManager.LoadScene("Menu");
    }
}
