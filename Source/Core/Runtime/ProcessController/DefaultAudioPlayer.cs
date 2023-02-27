using UnityEngine;
using VRBuilder.Core.Configuration;

public class DefaultAudioPlayer : IProcessAudioPlayer
{
    private AudioSource audioSource;

    public DefaultAudioPlayer()
    {
        GameObject user = RuntimeConfigurator.Configuration.User.gameObject;

        audioSource = user.GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = user.AddComponent<AudioSource>();
        }
    }

    public AudioSource FallbackAudioSource => audioSource;

    public void PlayAudioClip(AudioClip audioClip, float volume = 1, float pitch = 1)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }
}
