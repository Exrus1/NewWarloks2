using System;
using System.Collections;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class ControlsManager : MonoBehaviour
{
    public static KeyCode Jump = KeyCode.Space;
    public static KeyCode Fireball = KeyCode.Mouse0;
    public static KeyCode Invisible = KeyCode.Q;
    public static KeyCode Teleport = KeyCode.T;
    public static KeyCode ElectricBall = KeyCode.E;
    public static KeyCode WaterExplosion = KeyCode.V; // Добавлено для Water Explosion

    KeyCode currentControl;
    public static event Action openMenuEvent;
    [SerializeField] GameObject menu;

    [SerializeField] TMP_Text[] keys_text;

    // Ключи для сохранения в PlayerPrefs
    private const string JUMP_KEY = "JumpKey";
    private const string FIREBALL_KEY = "FireballKey";
    private const string INVISIBLE_KEY = "InvisibleKey";
    private const string TELEPORT_KEY = "TeleportKey";
    private const string ELECTRIC_BALL_KEY = "ElectricBallKey";
    private const string WATER_EXPLOSION_KEY = "WaterExplosionKey"; // Добавлено для Water Explosion
    [SerializeField] Image[] buttons;
    void TextUpdate(int i, string str)
    {
        keys_text[i].text = str;
    }

    private void Start()
    {
        LoadControls();
        UpdateAllTexts();
    }

    public void ChangeControl(int i)
    {
        StopAllCoroutines();

        buttons[i].color = Color.red;

        StartCoroutine(Cor(i));
    }

    IEnumerator Cor(int i)
    {
        // Ждем любое нажатие
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        // Ищем конкретную нажатую клавишу
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode) && keyCode != KeyCode.None)
            {
                switch (i)
                {
                    case 0:
                        Jump = keyCode;
                        SaveControl(JUMP_KEY, keyCode);
                        break;
                    case 1:
                        Fireball = keyCode;
                        SaveControl(FIREBALL_KEY, keyCode);
                        break;
                    case 2:
                        Invisible = keyCode;
                        SaveControl(INVISIBLE_KEY, keyCode);
                        break;
                    case 3:
                        Teleport = keyCode;
                        SaveControl(TELEPORT_KEY, keyCode);
                        break;
                    case 4:
                        ElectricBall = keyCode;
                        SaveControl(ELECTRIC_BALL_KEY, keyCode);
                        break;
                    case 5: // Добавлено для Water Explosion
                        WaterExplosion = keyCode;
                        SaveControl(WATER_EXPLOSION_KEY, keyCode);
                        break;
                }
                TextUpdate(i, GetKeyDisplayName(keyCode));
                buttons[i].color = Color.white;
                yield break;
            }
        }

    }

    // Сохранение управления
    private void SaveControl(string key, KeyCode value)
    {
        PlayerPrefs.SetInt(key, (int)value);
        PlayerPrefs.Save(); // Важно вызывать Save для немедленного сохранения
    }

    // Загрузка управления
    private void LoadControls()
    {
        if (PlayerPrefs.HasKey(JUMP_KEY))
            Jump = (KeyCode)PlayerPrefs.GetInt(JUMP_KEY);
        else
            SaveControl(JUMP_KEY, Jump); // Сохраняем значение по умолчанию

        if (PlayerPrefs.HasKey(FIREBALL_KEY))
            Fireball = (KeyCode)PlayerPrefs.GetInt(FIREBALL_KEY);
        else
            SaveControl(FIREBALL_KEY, Fireball);

        if (PlayerPrefs.HasKey(INVISIBLE_KEY))
            Invisible = (KeyCode)PlayerPrefs.GetInt(INVISIBLE_KEY);
        else
            SaveControl(INVISIBLE_KEY, Invisible);

        if (PlayerPrefs.HasKey(TELEPORT_KEY))
            Teleport = (KeyCode)PlayerPrefs.GetInt(TELEPORT_KEY);
        else
            SaveControl(TELEPORT_KEY, Teleport);

        if (PlayerPrefs.HasKey(ELECTRIC_BALL_KEY))
            ElectricBall = (KeyCode)PlayerPrefs.GetInt(ELECTRIC_BALL_KEY);
        else
            SaveControl(ELECTRIC_BALL_KEY, ElectricBall);

        if (PlayerPrefs.HasKey(WATER_EXPLOSION_KEY)) // Добавлено для Water Explosion
            WaterExplosion = (KeyCode)PlayerPrefs.GetInt(WATER_EXPLOSION_KEY);
        else
            SaveControl(WATER_EXPLOSION_KEY, WaterExplosion);
    }

    // Обновление всех текстовых полей
    private void UpdateAllTexts()
    {
        TextUpdate(0, GetKeyDisplayName(Jump));
        TextUpdate(1, GetKeyDisplayName(Fireball));
        TextUpdate(2, GetKeyDisplayName(Invisible));
        TextUpdate(3, GetKeyDisplayName(Teleport));
        TextUpdate(4, GetKeyDisplayName(ElectricBall));
        TextUpdate(5, GetKeyDisplayName(WaterExplosion)); // Добавлено для Water Explosion
    }

    // Сброс к настройкам по умолчанию
    public void ResetToDefaults()
    {
        Jump = KeyCode.Space;
        Fireball = KeyCode.Mouse0;
        Invisible = KeyCode.Q;
        Teleport = KeyCode.T;
        ElectricBall = KeyCode.E;
        WaterExplosion = KeyCode.R; // Добавлено для Water Explosion

        SaveControl(JUMP_KEY, Jump);
        SaveControl(FIREBALL_KEY, Fireball);
        SaveControl(INVISIBLE_KEY, Invisible);
        SaveControl(TELEPORT_KEY, Teleport);
        SaveControl(ELECTRIC_BALL_KEY, ElectricBall);
        SaveControl(WATER_EXPLOSION_KEY, WaterExplosion); // Добавлено для Water Explosion

        UpdateAllTexts();
    }

    // Удаление всех сохраненных настроек (для отладки)
    public void ClearAllSaves()
    {
        PlayerPrefs.DeleteKey(JUMP_KEY);
        PlayerPrefs.DeleteKey(FIREBALL_KEY);
        PlayerPrefs.DeleteKey(INVISIBLE_KEY);
        PlayerPrefs.DeleteKey(TELEPORT_KEY);
        PlayerPrefs.DeleteKey(ELECTRIC_BALL_KEY);
        PlayerPrefs.DeleteKey(WATER_EXPLOSION_KEY); // Добавлено для Water Explosion
        PlayerPrefs.Save();

        ResetToDefaults();
    }

    private string GetKeyDisplayName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Mouse0: return "Левая кнопка мыши";
            case KeyCode.Mouse1: return "Правая кнопка мыши";
            case KeyCode.Mouse2: return "Колесико";
            case KeyCode.LeftControl: return "Левый Ctrl";
            case KeyCode.RightControl: return "Правый Ctrl";
            case KeyCode.LeftShift: return "Левый Shift";
            case KeyCode.RightShift: return "Правый Shift";
            case KeyCode.LeftAlt: return "Левый Alt";
            case KeyCode.RightAlt: return "Правый Alt";
            case KeyCode.UpArrow: return "↑";
            case KeyCode.DownArrow: return "↓";
            case KeyCode.LeftArrow: return "←";
            case KeyCode.RightArrow: return "→";
            case KeyCode.Space: return "Пробел";
            default: return key.ToString();
        }
    }
}