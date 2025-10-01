using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class SoundManager : MonoBehaviour
{
  [SerializeField]  AudioClip[] music;
    AudioSource musicSource;
    public static float MusicVolume;
    public static float SoundVolume;
    public static event Action<float> SFXEvent;
 [SerializeField]   Slider musicSlider;
 [SerializeField]   Slider soundSlider;
    private void Awake()
    {
            musicSource = GetComponent<AudioSource>();
        if (PlayerPrefs.HasKey("SoundVolume")) 
        {
            SoundVolume = PlayerPrefs.GetFloat("SoundVolume");
        }
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        }
        ChangeSoundVolume(SoundVolume);
        ChangeMusicVolume(MusicVolume);
        
        Settings.SettingsOpen += FindSlidersAndChangeSlidersValues;
        //SceneManager.sceneLoaded += SoundUpdateOnSceneLoaded;
    }

     void FindSlidersAndChangeSlidersValues() 
    {
        musicSlider = GameObject.Find("MusicBar").GetComponent<Slider>();
        soundSlider = GameObject.Find("SoundsBar").GetComponent<Slider>();
        ChangeMusicVolume(MusicVolume);
        ChangeSoundVolume(SoundVolume);
        musicSlider.value = MusicVolume;
        soundSlider.value = SoundVolume;
        musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        soundSlider.onValueChanged.AddListener(ChangeSoundVolume);

    }

    //void SoundUpdateOnSceneLoaded(Scene scene, LoadSceneMode mode) 
    //{
    //    SFXEvent?.Invoke(SoundVolume);
    //}
    public void ChangeSoundVolume(float value)
    {
        SoundVolume = value;
        SFXEvent?.Invoke(SoundVolume);
        PlayerPrefs.SetFloat("SoundVolume", SoundVolume);
    }
    public void ChangeMusicVolume(float value)
    {
        MusicVolume = value;
        musicSource.volume = MusicVolume;
        PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
    }
    public void PlayMusic()
    {
        musicSource.Stop();
        musicSource.clip = music[0];
        musicSource.Play();
    }
    public void PlayBattleMusic()
    {
        musicSource.Stop();
        musicSource.clip = music[1];
        musicSource.Play();
    }
   
}
