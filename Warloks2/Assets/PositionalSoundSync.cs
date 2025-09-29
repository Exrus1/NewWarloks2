
using Photon.Pun;
using UnityEngine;

public class PositionalSoundSync : MonoBehaviourPunCallbacks
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footSteps;
    public AudioClip[] sfx;//firebal, invision, teleport,teleportCharging

    public void PlaySoundAtPosition(int index,Vector3 position)
    {

        audioSource.transform.position = position;
        audioSource.PlayOneShot(sfx[index]);

        // Синхронизация с другими клиентами
        photonView.RPC("RPC_PlaySoundAtPosition", RpcTarget.Others, index, position);
    }

    [PunRPC]
    void RPC_PlaySoundAtPosition(int index, Vector3 position)
    {
        audioSource.PlayOneShot(sfx[index]);
    }
    public void PlayFootSound()
    {
       
        audioSource.transform.position = transform.position;
        int randIndex = Random.Range(0, footSteps.Length);
        audioSource.clip = footSteps[3];
        audioSource.pitch = Random.Range(1.25f, 1.75f);
        audioSource.Play();

        // Синхронизация с другими клиентами
        photonView.RPC("RPC_PlayFootSound", RpcTarget.Others, 3, transform.position);
    }

    [PunRPC]
    void RPC_PlayFootSound(int soundIndex, Vector3 position)
    {
        audioSource.clip = footSteps[soundIndex];
        audioSource.Play();
    }
}