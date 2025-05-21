using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Scripting;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Utils.ParticleMachines;
using Object = UnityEngine.Object;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// This behavior causes confetti to rain.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ConfettiBehavior : Behavior<ConfettiBehavior.EntityData>
    {
        [DisplayName("Spawn Confetti")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData, IBehaviorExecutionStages
        {
            /// <summary>
            /// Bool to check whether the confetti machine should spawn above the user or at the position of the position provider.
            /// </summary>
            [DataMember]
            [DisplayName("Spawn Above User")]
            public bool IsAboveUser { get; set; }

            /// <summary>
            /// Name of the process object where to spawn the confetti machine.
            /// Only needed if "Spawn Above User" is not checked.
            /// </summary>
            [DataMember]
            [DisplayName("Position Provider")]
            public SingleSceneObjectReference ConfettiPosition { get; set; }

            /// <summary>
            /// Path to the desired confetti machine prefab.
            /// </summary>
            [DataMember]
            [DisplayName("Confetti Machine Path")]
            public string ConfettiMachinePrefabPath { get; set; }

            /// <summary>
            /// Radius of the spawning area.
            /// </summary>
            [DataMember]
            [DisplayName("Area Radius")]
            public float AreaRadius { get; set; }

            /// <summary>
            /// Duration of the animation in seconds.
            /// </summary>
            [DataMember]
            [DisplayName("Duration")]
            public float Duration { get; set; }

            /// <inheritdoc />
            [DataMember]
            public BehaviorExecutionStages ExecutionStages { get; set; }

            public GameObject ConfettiMachine { get; set; }

            public Metadata Metadata { get; set; }

            [IgnoreDataMember]
            public string Name
            {
                get
                {
                    string positionProvider = "user";
                    if (IsAboveUser == false)
                    {
                        positionProvider = ConfettiPosition.HasValue() ? ConfettiPosition.Value.GameObject.name : "[NULL]";
                    }

                    return $"Spawn confetti on {positionProvider}";
                }
            }
        }

        private const float defaultDuration = 15f;
        private const float defaultRadius = 1f;
        private const float distanceAboveUser = 3f;

        [JsonConstructor, Preserve]
        public ConfettiBehavior() : this(true, Guid.Empty, "", defaultRadius, defaultDuration, BehaviorExecutionStages.Activation)
        {
        }

        public ConfettiBehavior(bool isAboveUser, ISceneObject positionProvider, string confettiMachinePrefabPath, float radius, float duration, BehaviorExecutionStages executionStages)
            : this(isAboveUser, ProcessReferenceUtils.GetUniqueIdFrom(positionProvider), confettiMachinePrefabPath, radius, duration, executionStages)
        {
        }

        public ConfettiBehavior(bool isAboveUser, Guid positionProviderId, string confettiMachinePrefabPath, float radius, float duration, BehaviorExecutionStages executionStages)
        {
            Data.IsAboveUser = isAboveUser;
            Data.ConfettiPosition = new SingleSceneObjectReference(positionProviderId);
            Data.ConfettiMachinePrefabPath = confettiMachinePrefabPath;
            Data.AreaRadius = radius;
            Data.Duration = duration;
            Data.ExecutionStages = executionStages;

            if (string.IsNullOrEmpty(Data.ConfettiMachinePrefabPath) && RuntimeConfigurator.Exists)
            {
                Data.ConfettiMachinePrefabPath = RuntimeConfigurator.Configuration.SceneConfiguration.DefaultConfettiPrefab;
            }
        }

        private class EmitConfettiProcess : StageProcess<EntityData>
        {
            private readonly BehaviorExecutionStages stages;
            private float timeStarted;
            private GameObject confettiPrefab;
            private List<GameObject> confettiMachines = new List<GameObject>();

            public EmitConfettiProcess(EntityData data, BehaviorExecutionStages stages) : base(data)
            {
                this.stages = stages;
            }

            /// <inheritdoc />
            public override void Start()
            {
                if (ShouldExecuteCurrentStage(Data) == false)
                {
                    return;
                }

                // Load the given prefab and stop the coroutine if not possible.
                confettiPrefab = Resources.Load<GameObject>(Data.ConfettiMachinePrefabPath);

                if (confettiPrefab == null)
                {
                    Debug.LogWarning("No valid prefab path provided.");
                    return;
                }

                if (Data.IsAboveUser)
                {
                    foreach (IXRRigTransform user in RuntimeConfigurator.Configuration.UserTransforms)
                    {
                        Vector3 spawnPosition;
                        spawnPosition = user.Head.position;
                        spawnPosition.y += distanceAboveUser;

                        CreateConfettiMachine(spawnPosition);
                    }
                }
                else
                {
                    CreateConfettiMachine(Data.ConfettiPosition.Value.GameObject.transform.position);
                }

                if (Data.Duration > 0f)
                {
                    timeStarted = Time.time;
                }
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                if (ShouldExecuteCurrentStage(Data) == false)
                {
                    yield break;
                }

                if (confettiMachines.Count == 0)
                {
                    yield break;
                }

                if (Data.Duration > 0)
                {
                    while (Time.time - timeStarted < Data.Duration)
                    {
                        yield return null;
                    }
                }
            }

            /// <inheritdoc />
            public override void End()
            {
                if (ShouldExecuteCurrentStage(Data))
                {
                    foreach (GameObject confettiMachine in confettiMachines)
                    {
                        Object.Destroy(confettiMachine);
                    }

                    confettiMachines.Clear();
                }
            }

            /// <inheritdoc />
            public override void FastForward() { }

            private bool ShouldExecuteCurrentStage(EntityData data)
            {
                return (data.ExecutionStages & stages) > 0;
            }

            private void CreateConfettiMachine(Vector3 spawnPosition)
            {
                RuntimeConfigurator.Configuration.SceneObjectManager.InstantiatePrefab(confettiPrefab, spawnPosition, Quaternion.Euler(90, 0, 0), OnConfettiMachineCreated);
            }

            private void OnConfettiMachineCreated(GameObject confettiMachine)
            {
                if (confettiMachine == null)
                {
                    Debug.LogWarning("The provided prefab is missing.");
                    return;
                }

                if (confettiMachine.GetComponent(typeof(IParticleMachine)) == null)
                {
                    Debug.LogWarning("The provided prefab does not have any component of type \"IParticleMachine\".");
                    return;
                }

                confettiMachines.Add(confettiMachine);

                // Change the settings and activate the machine
                IParticleMachine particleMachine = confettiMachine.GetComponent<IParticleMachine>();
                particleMachine.Activate(Data.AreaRadius, Data.Duration);
            }
        }


        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new EmitConfettiProcess(Data, BehaviorExecutionStages.Activation);
        }

        /// <inheritdoc />
        public override IStageProcess GetDeactivatingProcess()
        {
            return new EmitConfettiProcess(Data, BehaviorExecutionStages.Deactivation);
        }
    }
}
