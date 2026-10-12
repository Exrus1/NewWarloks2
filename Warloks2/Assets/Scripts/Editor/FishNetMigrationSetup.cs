#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using FishNet.Component.Scenes;
using FishNet.Component.Transforming;
using FishNet.Managing;
using FishNet.Managing.Object;
using FishNet.Managing.Observing;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting.Tugboat;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// One-shot / menu setup for Photon → FishNet migration (prefabs, NetworkManager, scenes).
/// </summary>
public static class FishNetMigrationSetup
{
    const string PrefabFolder = "Assets/Prefabs";
    const string NetworkManagerPath = PrefabFolder + "/NetworkManager.prefab";
    const string DefaultPrefabsPath = "Assets/DefaultPrefabObjects.asset";

    static readonly string[] SpawnablePaths =
    {
        "Assets/Resources/Player.prefab",
        "Assets/Resources/Fireball.prefab",
        "Assets/Resources/ElectricBall.prefab",
        "Assets/Resources/WaterExplosion.prefab",
        "Assets/Resources/TeleportEffect.prefab"
    };

    [MenuItem("Warloks/FishNet/Run Migration Setup")]
    public static void RunSetup()
    {
        EnsureFolder(PrefabFolder);
        try { ConvertSpawnablePrefabs(); }
        catch (System.Exception e) { Debug.LogError("[FishNetMigrationSetup] Prefabs: " + e); }

        GameObject nmPrefab = null;
        try { nmPrefab = CreateNetworkManagerPrefab(); }
        catch (System.Exception e) { Debug.LogError("[FishNetMigrationSetup] NetworkManager: " + e); }

        try { RegisterDefaultPrefabs(); }
        catch (System.Exception e) { Debug.LogError("[FishNetMigrationSetup] DefaultPrefabs: " + e); }

        try { if (nmPrefab != null) SetupMenuScene(nmPrefab); }
        catch (System.Exception e) { Debug.LogError("[FishNetMigrationSetup] Menu: " + e); }

        try { SetupLobbyScene(); }
        catch (System.Exception e) { Debug.LogError("[FishNetMigrationSetup] Lobby: " + e); }

        try { SetupGameScene(); }
        catch (System.Exception e) { Debug.LogError("[FishNetMigrationSetup] Game: " + e); }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[FishNetMigrationSetup] Completed. Menu: Host / Server / Client + IP. Port 7770.");
    }

    [InitializeOnLoadMethod]
    static void AutoRunOnce()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            string absoluteNm = Path.Combine(Application.dataPath, "Prefabs", "NetworkManager.prefab");
            if (File.Exists(absoluteNm))
            {
                SessionState.SetBool("WarloksFishNetMigrationDone", true);
                return;
            }

            if (SessionState.GetBool("WarloksFishNetMigrationAttempted", false))
                return;
            SessionState.SetBool("WarloksFishNetMigrationAttempted", true);

            try
            {
                Debug.Log("[FishNetMigrationSetup] Auto-running migration setup...");
                RunSetup();
                SessionState.SetBool("WarloksFishNetMigrationDone", true);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[FishNetMigrationSetup] Auto setup failed:\n" + e);
            }
        };
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && parent != "Assets")
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }
    }

    static void ConvertSpawnablePrefabs()
    {
        foreach (string path in SpawnablePaths)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            if (prefabRoot == null)
            {
                Debug.LogWarning("Missing prefab: " + path);
                continue;
            }

            RemovePhotonComponents(prefabRoot);
            NetworkObject nob = prefabRoot.GetComponent<NetworkObject>();
            if (nob == null)
                nob = prefabRoot.AddComponent<NetworkObject>();

            if (prefabRoot.GetComponent<NetworkTransform>() == null)
                prefabRoot.AddComponent<NetworkTransform>();

            if (path.EndsWith("Player.prefab"))
            {
                if (prefabRoot.GetComponent<SimpleNetworkAnimator>() == null)
                    prefabRoot.AddComponent<SimpleNetworkAnimator>();
                if (prefabRoot.GetComponent<NetworkDespawn>() == null)
                    prefabRoot.AddComponent<NetworkDespawn>();
                if (prefabRoot.GetComponent<PhotonDestroy>() == null)
                    prefabRoot.AddComponent<PhotonDestroy>();

                WirePlayerSkillPrefabs(prefabRoot);
            }
            else
            {
                if (prefabRoot.GetComponent<NetworkDespawn>() == null)
                    prefabRoot.AddComponent<NetworkDespawn>();
                if (prefabRoot.GetComponent<PhotonDestroy>() == null)
                    prefabRoot.AddComponent<PhotonDestroy>();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            Debug.Log("Converted " + path);
        }
    }

    static void WirePlayerSkillPrefabs(GameObject player)
    {
        PlayerSkills skills = player.GetComponent<PlayerSkills>();
        if (skills == null)
            return;

        SerializedObject so = new(skills);
        AssignNob(so, "fireballPrefab", "Assets/Resources/Fireball.prefab");
        AssignNob(so, "electricBallPrefab", "Assets/Resources/ElectricBall.prefab");
        AssignNob(so, "waterExplosionPrefab", "Assets/Resources/WaterExplosion.prefab");
        AssignNob(so, "teleportParticle", "Assets/Resources/TeleportEffect.prefab");
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AssignNob(SerializedObject so, string prop, string prefabPath)
    {
        SerializedProperty sp = so.FindProperty(prop);
        if (sp == null)
            return;
        NetworkObject nob = AssetDatabase.LoadAssetAtPath<NetworkObject>(prefabPath);
        if (nob != null)
            sp.objectReferenceValue = nob;
    }

    static void RemovePhotonComponents(GameObject root)
    {
        // Remove by type name to avoid hard compile dependency on Photon during cleanup order.
        MonoBehaviour[] behaviours = root.GetComponents<MonoBehaviour>();
        List<Object> toDestroy = new();
        foreach (MonoBehaviour mb in behaviours)
        {
            if (mb == null)
                continue;
            string full = mb.GetType().FullName ?? string.Empty;
            if (full.StartsWith("Photon.Pun") || full.Contains("PhotonTransformView") ||
                full.Contains("PhotonRigidbodyView") || full.Contains("PhotonAnimatorView") ||
                full.Contains("PhotonView"))
                toDestroy.Add(mb);
        }

        foreach (Object o in toDestroy)
            Object.DestroyImmediate(o, true);
    }

    static GameObject CreateNetworkManagerPrefab()
    {
        GameObject go = new("NetworkManager");
        NetworkManager nm = go.AddComponent<NetworkManager>();
        go.AddComponent<Tugboat>();
        go.AddComponent<ObserverManager>();

        DefaultScene defaultScene = go.AddComponent<DefaultScene>();
        SerializedObject ds = new(defaultScene);
        SerializedProperty offline = ds.FindProperty("_offlineScene");
        SerializedProperty online = ds.FindProperty("_onlineScene");
        if (offline != null)
            offline.stringValue = "Assets/Scenes/Menu.unity";
        if (online != null)
            online.stringValue = "Assets/Scenes/Lobby.unity";
        SerializedProperty replace = ds.FindProperty("_replaceScenes");
        if (replace != null)
            replace.enumValueIndex = (int)ReplaceOption.All;
        ds.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject nmSo = new(nm);
        SerializedProperty spawnable = nmSo.FindProperty("_spawnablePrefabs");
        DefaultPrefabObjects dpo = AssetDatabase.LoadAssetAtPath<DefaultPrefabObjects>(DefaultPrefabsPath);
        if (spawnable != null && dpo != null)
            spawnable.objectReferenceValue = dpo;
        SerializedProperty ddol = nmSo.FindProperty("_dontDestroyOnLoad");
        if (ddol != null)
            ddol.boolValue = true;
        nmSo.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, NetworkManagerPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    static void RegisterDefaultPrefabs()
    {
        DefaultPrefabObjects dpo = AssetDatabase.LoadAssetAtPath<DefaultPrefabObjects>(DefaultPrefabsPath);
        if (dpo == null)
        {
            Debug.LogError("DefaultPrefabObjects missing at " + DefaultPrefabsPath);
            return;
        }

        SerializedObject so = new(dpo);
        SerializedProperty prefabs = so.FindProperty("_prefabs");
        if (prefabs == null)
            return;

        foreach (string path in SpawnablePaths)
        {
            NetworkObject nob = AssetDatabase.LoadAssetAtPath<NetworkObject>(path);
            if (nob == null)
                continue;
            bool found = false;
            for (int i = 0; i < prefabs.arraySize; i++)
            {
                if (prefabs.GetArrayElementAtIndex(i).objectReferenceValue == nob)
                {
                    found = true;
                    break;
                }
            }
            if (found)
                continue;
            prefabs.InsertArrayElementAtIndex(prefabs.arraySize);
            prefabs.GetArrayElementAtIndex(prefabs.arraySize - 1).objectReferenceValue = nob;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(dpo);
    }

    static void SetupMenuScene(GameObject nmPrefab)
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Single);
        bool dirty = false;

        if (Object.FindAnyObjectByType<NetworkManager>() == null && nmPrefab != null)
        {
            PrefabUtility.InstantiatePrefab(nmPrefab);
            dirty = true;
        }

        MenuManager menu = Object.FindAnyObjectByType<MenuManager>();
        if (menu != null)
        {
            SerializedObject so = new(menu);
            EnsureIpField(menu, so);
            EnsureServerButton(menu, so);
            so.ApplyModifiedPropertiesWithoutUndo();
            dirty = true;

            // Relabel existing buttons if found by text.
            foreach (TMP_Text tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                string t = tmp.text ?? string.Empty;
                if (t.Contains("Создать"))
                {
                    tmp.text = "Хост";
                    dirty = true;
                }
                else if (t.Contains("Войти"))
                {
                    tmp.text = "Клиент";
                    dirty = true;
                }
            }
        }

        if (dirty)
            EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void EnsureIpField(MenuManager menu, SerializedObject so)
    {
        SerializedProperty ipProp = so.FindProperty("ipField");
        if (ipProp == null)
            return;
        if (ipProp.objectReferenceValue != null)
            return;

        TMP_InputField nameField = so.FindProperty("inputField")?.objectReferenceValue as TMP_InputField;
        if (nameField == null)
            return;

        GameObject clone = Object.Instantiate(nameField.gameObject, nameField.transform.parent);
        clone.name = "IPField";
        RectTransform rt = clone.GetComponent<RectTransform>();
        if (rt != null)
            rt.anchoredPosition = nameField.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, -70f);

        TMP_InputField ip = clone.GetComponent<TMP_InputField>();
        ip.text = string.Empty;
        ip.placeholder.TryGetComponent(out TMP_Text ph);
        if (ph != null)
            ph.text = "IP хоста (Radmin)";

        ipProp.objectReferenceValue = ip;
    }

    static void EnsureServerButton(MenuManager menu, SerializedObject so)
    {
        SerializedProperty btnProp = so.FindProperty("serverButton");
        if (btnProp == null)
            return;
        if (btnProp.objectReferenceValue != null)
            return;

        // Find Join button and clone as Server.
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        Button template = null;
        foreach (Button b in buttons)
        {
            TMP_Text label = b.GetComponentInChildren<TMP_Text>();
            if (label != null && (label.text.Contains("Клиент") || label.text.Contains("Войти") || label.text.Contains("Join")))
            {
                template = b;
                break;
            }
        }
        if (template == null && buttons.Length > 0)
            template = buttons[0];
        if (template == null)
            return;

        GameObject clone = Object.Instantiate(template.gameObject, template.transform.parent);
        clone.name = "ServerButton";
        RectTransform rt = clone.GetComponent<RectTransform>();
        if (rt != null)
            rt.anchoredPosition = template.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, -80f);

        TMP_Text txt = clone.GetComponentInChildren<TMP_Text>();
        if (txt != null)
            txt.text = "Сервер";

        Button serverBtn = clone.GetComponent<Button>();
        serverBtn.onClick = new Button.ButtonClickedEvent();
        serverBtn.onClick.AddListener(menu.StartDedicatedServer);

        btnProp.objectReferenceValue = serverBtn;
        try
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(serverBtn.onClick, menu.StartDedicatedServer);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[FishNetMigrationSetup] Wire Server button manually to MenuManager.StartDedicatedServer: " + e.Message);
        }
    }

    static void SetupLobbyScene()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Lobby.unity", OpenSceneMode.Single);
        LobbyManager lobby = Object.FindAnyObjectByType<LobbyManager>();
        if (lobby != null)
        {
            if (lobby.GetComponent<NetworkObject>() == null)
                lobby.gameObject.AddComponent<NetworkObject>();
            EditorSceneManager.MarkSceneDirty(scene);
        }

        foreach (TMP_Text tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
        {
            // keep visuals; no forced renames in lobby
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void SetupGameScene()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Single);
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            if (gm.GetComponent<NetworkObject>() == null)
                gm.gameObject.AddComponent<NetworkObject>();

            SerializedObject so = new(gm);
            SerializedProperty prefabProp = so.FindProperty("playerPrefab");
            NetworkObject player = AssetDatabase.LoadAssetAtPath<NetworkObject>("Assets/Resources/Player.prefab");
            if (prefabProp != null && player != null)
                prefabProp.objectReferenceValue = player;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
        }
        EditorSceneManager.SaveScene(scene);

        // Return to menu for convenience.
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity", OpenSceneMode.Single);
    }
}
#endif
