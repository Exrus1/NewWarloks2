

using UnityEngine;
using UnityEngine.UI;

public class UISound : MonoBehaviour
{
    AudioSource source;
   [SerializeField] AudioClip clip;
    Button button;
    private void Start()
    {
        button = GetComponent<Button>();
        source = gameObject.AddComponent<AudioSource>();
        
        button.onClick.AddListener(Play);
      
    }
    public void Play()
     {
        source.volume = SoundManager.SoundVolume;
        source.clip = clip;
        source.Play();
     } 

}
