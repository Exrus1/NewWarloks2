using UnityEngine;

public class SFX : MonoBehaviour
{
    AudioSource source;
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = SoundManager.SoundVolume;
        SoundManager.SFXEvent += ChangeVolume;
    }
    void ChangeVolume(float value) 
    { 
        source.volume = value;
        print(source.volume);
    }
   
}
