using System;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [Header("Настройки анимации")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float rotationAmplitude = 15f;
    [SerializeField] private float animationSpeed = 2f;

    private RectTransform rectTransform;
    private Vector3 startRotation;
    private float timeCounter = 0f;
  [SerializeField]  GameObject settingsMenu;
    public static event Action SettingsOpen;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startRotation = rectTransform.localEulerAngles;
        settingsMenu = GameObject.Find("SettingsMenu");
       settingsMenu.SetActive(false);
     
    }

    void Update()
    {
        if (settingsMenu!=null&& settingsMenu.activeSelf)
        {
            timeCounter += Time.deltaTime * animationSpeed;

            // Синусоидальное вращение
            float sinValue = Mathf.Sin(timeCounter);
            float rotation = sinValue * rotationAmplitude;

            // Применяем вращение только по Z оси
            rectTransform.localRotation = Quaternion.Euler(
                startRotation.x,
                startRotation.y,
                startRotation.z + rotation
            );
        }
    }
    public void OpenSettings() 
    {
        if (settingsMenu.activeSelf)
        {
            settingsMenu.SetActive(false);
        }
        else
        {
           
            settingsMenu.SetActive(true);
            SettingsOpen?.Invoke();
        }
    }
}
