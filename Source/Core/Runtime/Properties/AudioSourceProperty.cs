using UnityEngine;

namespace VRBuilder.Core.Properties
{
    /// <summary>
    /// Implementation of the <see cref="IAudioSourceProperty"/> class.
    /// </summary>
    public class AudioSourceProperty: ProcessSceneObjectProperty, IAudioSourceProperty
    {
        /// <inheritdoc/>
        public AudioSource AudioPlayer
        {
            get
            {
                return audioSource ??= GetComponent<AudioSource>();
            }
        }
        
        [Header("Settings")]
        [SerializeField]
        private AudioSource audioSource;
    }
}