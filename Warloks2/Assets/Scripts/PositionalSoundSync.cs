using FishNet.Object;
using UnityEngine;

public class PositionalSoundSync : NetworkBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footSteps;
    public AudioClip[] sfx;

    public override void OnStartClient()
    {
        SoundManager.SFXEvent += ChangeVolume;
    }

    void OnDestroy()
    {
        SoundManager.SFXEvent -= ChangeVolume;
    }

    void ChangeVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;
    }

    public void PlaySoundAtPosition(int index, Vector3 position)
    {
        if (audioSource == null || sfx == null || index < 0 || index >= sfx.Length)
            return;

        audioSource.transform.position = position;
        audioSource.PlayOneShot(sfx[index]);
        PlaySoundObserversRpc(index, position);
    }

    [ObserversRpc(ExcludeOwner = true)]
    void PlaySoundObserversRpc(int index, Vector3 position)
    {
        if (audioSource == null || sfx == null || index < 0 || index >= sfx.Length)
            return;
        audioSource.PlayOneShot(sfx[index]);
    }

    public void PlayFootSound()
    {
        if (audioSource == null || footSteps == null || footSteps.Length == 0)
            return;

        audioSource.transform.position = transform.position;
        int soundIndex = Mathf.Min(3, footSteps.Length - 1);
        audioSource.clip = footSteps[soundIndex];
        audioSource.pitch = Random.Range(1.25f, 1.75f);
        audioSource.Play();
        PlayFootObserversRpc(soundIndex, transform.position);
    }

    [ObserversRpc(ExcludeOwner = true)]
    void PlayFootObserversRpc(int soundIndex, Vector3 position)
    {
        if (audioSource == null || footSteps == null || soundIndex < 0 || soundIndex >= footSteps.Length)
            return;
        audioSource.clip = footSteps[soundIndex];
        audioSource.Play();
    }
}
