
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] GameObject soundMenu, controlsMenu;
    public void OpenSoundMenu()
    {
       
        if (soundMenu.activeSelf)
        {
            soundMenu.SetActive(false);
        }
        else
        {
           soundMenu.SetActive(true);
            controlsMenu.SetActive(false);
        }
    }
    public void OpenControlsMenu()
    {

        if (controlsMenu.activeSelf)
        {
            controlsMenu.SetActive(false);
        }
        else
        {
            controlsMenu.SetActive(true);
            soundMenu.SetActive(false);
        }
    }
}
