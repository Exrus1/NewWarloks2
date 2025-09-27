using UnityEngine;


public class MusicManager : MonoBehaviour
{
  [SerializeField]  AudioClip[] music;
    AudioSource source;

    private void Awake()
    {
            source = GetComponent<AudioSource>();
       
    }
    public void PlayMusic()
    {
        source.Stop();
        source.clip = music[0];
        source.Play();
    }
    public void PlayBattleMusic()
    {
        source.Stop();
        source.clip = music[1];
        source.Play();
    }
   
}
