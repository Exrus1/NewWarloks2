using UnityEngine;

public class Settings : MonoBehaviour
{
    [Header("��������� ��������")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float rotationAmplitude = 15f;
    [SerializeField] private float animationSpeed = 2f;

    private RectTransform rectTransform;
    private Vector3 startRotation;
    private float timeCounter = 0f;
   [SerializeField]GameObject settingsMenu;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startRotation = rectTransform.localEulerAngles;
    }

    void Update()
    {
        if (settingsMenu.activeSelf)
        {
            timeCounter += Time.deltaTime * animationSpeed;

            // �������������� ��������
            float sinValue = Mathf.Sin(timeCounter);
            float rotation = sinValue * rotationAmplitude;

            // ��������� �������� ������ �� Z ���
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
        }
    }
}
