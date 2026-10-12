#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Places Host/Client/Server/IP controls into Menu.unity (no runtime Instantiate).
/// </summary>
public static class BakeMenuConnectUi
{
    const string MenuScenePath = "Assets/Scenes/Menu.unity";

    [MenuItem("Warloks/FishNet/Bake Menu Connect UI")]
    public static void Bake()
    {
        Scene scene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        MenuManager menu = Object.FindAnyObjectByType<MenuManager>();
        if (menu == null)
        {
            Debug.LogError("[BakeMenuConnectUi] MenuManager not found.");
            return;
        }

        SerializedObject so = new(menu);
        SerializedProperty inputProp = so.FindProperty("inputField");
        SerializedProperty ipProp = so.FindProperty("ipField");
        SerializedProperty serverProp = so.FindProperty("serverButton");

        TMP_InputField nameField = inputProp?.objectReferenceValue as TMP_InputField;
        if (nameField == null)
            nameField = Object.FindAnyObjectByType<TMP_InputField>();

        // Labels on existing buttons
        foreach (TMP_Text tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (string.IsNullOrEmpty(tmp.text))
                continue;
            if (tmp.text.Contains("Создать"))
                tmp.text = "Хост";
            else if (tmp.text.Contains("Войти") || tmp.text.Contains("войти"))
                tmp.text = "Клиент";
        }

        // IP field
        TMP_InputField ipField = ipProp?.objectReferenceValue as TMP_InputField;
        if (ipField == null)
            ipField = FindNamed<TMP_InputField>("IPField");

        if (ipField == null && nameField != null)
        {
            GameObject clone = (GameObject)PrefabUtility.InstantiatePrefab(nameField.gameObject);
            if (clone == null)
                clone = Object.Instantiate(nameField.gameObject, nameField.transform.parent);

            clone.name = "IPField";
            clone.transform.SetParent(nameField.transform.parent, false);
            RectTransform rt = clone.GetComponent<RectTransform>();
            RectTransform src = nameField.GetComponent<RectTransform>();
            if (rt != null && src != null)
            {
                rt.anchorMin = src.anchorMin;
                rt.anchorMax = src.anchorMax;
                rt.pivot = src.pivot;
                rt.sizeDelta = src.sizeDelta;
                rt.anchoredPosition = src.anchoredPosition + new Vector2(0f, -90f);
            }

            ipField = clone.GetComponent<TMP_InputField>();
            ipField.text = string.Empty;
            ipField.onEndEdit.RemoveAllListeners();
            if (ipField.placeholder != null && ipField.placeholder.TryGetComponent(out TMP_Text ph))
                ph.text = "IP хоста (Radmin)";

            Undo.RegisterCreatedObjectUndo(clone, "Bake IPField");
        }

        if (ipProp != null && ipField != null)
            ipProp.objectReferenceValue = ipField;

        // Server button (clone Client/Join)
        Button serverBtn = serverProp?.objectReferenceValue as Button;
        if (serverBtn == null)
        {
            Transform existing = null;
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (t.name == "ServerButton" || t.name == "Server")
                {
                    existing = t;
                    break;
                }
            }
            if (existing != null)
                serverBtn = existing.GetComponent<Button>();
        }

        if (serverBtn == null)
        {
            Button template = FindButtonByLabel("Клиент");
            if (template == null)
                template = FindButtonByName("Join");
            if (template == null)
            {
                Button[] all = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                if (all.Length > 0)
                    template = all[0];
            }

            if (template != null)
            {
                GameObject clone = Object.Instantiate(template.gameObject, template.transform.parent);
                clone.name = "ServerButton";
                RectTransform rt = clone.GetComponent<RectTransform>();
                RectTransform src = template.GetComponent<RectTransform>();
                if (rt != null && src != null)
                {
                    rt.anchorMin = src.anchorMin;
                    rt.anchorMax = src.anchorMax;
                    rt.pivot = src.pivot;
                    rt.sizeDelta = src.sizeDelta;
                    rt.anchoredPosition = src.anchoredPosition + new Vector2(0f, -100f);
                }

                TMP_Text label = clone.GetComponentInChildren<TMP_Text>();
                if (label != null)
                    label.text = "Сервер";

                serverBtn = clone.GetComponent<Button>();
                serverBtn.onClick = new Button.ButtonClickedEvent();
                UnityEventTools.AddPersistentListener(serverBtn.onClick, menu.StartDedicatedServer);
                Undo.RegisterCreatedObjectUndo(clone, "Bake ServerButton");
            }
        }
        else
        {
            // Ensure persistent listener
            bool has = false;
            for (int i = 0; i < serverBtn.onClick.GetPersistentEventCount(); i++)
            {
                if (serverBtn.onClick.GetPersistentMethodName(i) == nameof(MenuManager.StartDedicatedServer))
                {
                    has = true;
                    break;
                }
            }
            if (!has)
                UnityEventTools.AddPersistentListener(serverBtn.onClick, menu.StartDedicatedServer);
        }

        if (serverProp != null && serverBtn != null)
            serverProp.objectReferenceValue = serverBtn;

        // Rename Create/Join gameobjects for clarity
        RenameIfExists("Create", "HostButton");
        RenameIfExists("Join", "ClientButton");

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(menu);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[BakeMenuConnectUi] Menu connect UI baked into scene.");
    }

    static T FindNamed<T>(string name) where T : Component
    {
        foreach (T c in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.gameObject.name == name)
                return c;
        }
        return null;
    }

    static Button FindButtonByLabel(string contains)
    {
        foreach (Button b in Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            TMP_Text t = b.GetComponentInChildren<TMP_Text>();
            if (t != null && t.text != null && t.text.Contains(contains))
                return b;
        }
        return null;
    }

    static Button FindButtonByName(string name)
    {
        foreach (Button b in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (b.gameObject.name == name)
                return b;
        }
        return null;
    }

    static void RenameIfExists(string from, string to)
    {
        foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name == from)
            {
                t.name = to;
                EditorUtility.SetDirty(t.gameObject);
                break;
            }
        }
    }
}
#endif
