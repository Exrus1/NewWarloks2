using System;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
  [SerializeField]  AudioClip[] music;
    AudioSource musicSource;
    public static float MusicVolume = 0.5f;
    public static float SoundVolume = 0.5f;
    public static event Action<float> SFXEvent;
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
        musicSource.volume = MusicVolume;
        SFXEvent?.Invoke(SoundVolume);
    }
    public void ChangeSoundVolume(float value) 
    {
        SoundVolume = value;
      
        PlayerPrefs.SetFloat("SoundVolume", SoundVolume);
    }
    public void ChangeMusicVolume(float value)
    {
        MusicVolume = value;
        musicSource.volume = MusicVolume;
        PlayerPrefs.SetFloat("MusicVolume", SoundVolume);
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
