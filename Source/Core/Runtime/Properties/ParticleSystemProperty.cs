using UnityEngine;
using UnityEngine.Events;

namespace VRBuilder.Core.Properties
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemProperty : ProcessSceneObjectProperty, IParticleSystemProperty
    {     
        [Header("Events")]
        [SerializeField]
        private UnityEvent<ParticleSystemPropertyEventArgs> startedEmission = new UnityEvent<ParticleSystemPropertyEventArgs>();

        [SerializeField]
        private UnityEvent<ParticleSystemPropertyEventArgs> stoppedEmission = new UnityEvent<ParticleSystemPropertyEventArgs>();

        /// <inheritdoc/>
        public UnityEvent<ParticleSystemPropertyEventArgs> StartedEmission => startedEmission;

        /// <inheritdoc/>
        public UnityEvent<ParticleSystemPropertyEventArgs> StoppedEmission => stoppedEmission;

        private new ParticleSystem particleSystem;

        /// <summary>
        /// The particle system associated with this property.
        /// </summary>
        public ParticleSystem ParticleSystem
        {
            get
            {
                if (particleSystem == null)
                {
                    particleSystem = GetComponent<ParticleSystem>();
                }

                return particleSystem;
            }
        }

        /// <inheritdoc/>
        public bool IsEmitting => ParticleSystem.isEmitting;

        /// <inheritdoc/>
        public void StartEmission()
        {
            ParticleSystem.Play();
            StartedEmission?.Invoke(new ParticleSystemPropertyEventArgs());
        }

        /// <inheritdoc/>
        public void StopEmission()
        {
            ParticleSystem.Stop();
            StoppedEmission?.Invoke(new ParticleSystemPropertyEventArgs());
        }
    }
}