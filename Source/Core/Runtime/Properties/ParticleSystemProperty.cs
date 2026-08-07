using System;
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

        private new ParticleSystem particleSystem;

        private Action<ParticleSystemPropertyEventArgs> startedEmissionAction;
        private Action<ParticleSystemPropertyEventArgs> stoppedEmissionAction;

        /// <inheritdoc/>
        event Action<ParticleSystemPropertyEventArgs> IParticleSystemProperty.StartedEmission
        {
            add => startedEmissionAction += value;
            remove => startedEmissionAction -= value;
        }

        /// <inheritdoc/>
        event Action<ParticleSystemPropertyEventArgs> IParticleSystemProperty.StoppedEmission
        {
            add => stoppedEmissionAction += value;
            remove => stoppedEmissionAction -= value;
        }

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


        protected override void OnEnable()
        {
            base.OnEnable();

            startedEmission.AddListener(OnStartedEmission);
            stoppedEmission.AddListener(OnStoppedEmission);

        }

        private void OnStartedEmission(ParticleSystemPropertyEventArgs args)
        {
            startedEmissionAction.Invoke(args);
        }

        private void OnStoppedEmission(ParticleSystemPropertyEventArgs args)
        {
            stoppedEmissionAction.Invoke(args);
        }

        /// <inheritdoc/>
        public void StartEmission()
        {
            ParticleSystem.Play();
            startedEmissionAction?.Invoke(new ParticleSystemPropertyEventArgs());
        }

        /// <inheritdoc/>
        public void StopEmission()
        {
            ParticleSystem.Stop();
            stoppedEmissionAction?.Invoke(new ParticleSystemPropertyEventArgs());
        }
    }
}
