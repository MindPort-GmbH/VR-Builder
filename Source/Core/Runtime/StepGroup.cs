using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.EntityOwners;
using VRBuilder.Core.EntityOwners.FoldedEntityCollection;
using VRBuilder.Core.Exceptions;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.Utils;
using VRBuilder.Core.Utils.Logging;

namespace VRBuilder.Core
{
    [DataContract(IsReference = true)]
    public class StepGroup : Entity<StepGroup.EntityData>, IChapter, IStep
    {
        [DataMember]
        public StepMetadata StepMetadata { get; set; }

        IStepData IDataOwner<IStepData>.Data => Data;

        [DataMember]
        public ChapterMetadata ChapterMetadata { get; set; }

        IChapterData IDataOwner<IChapterData>.Data => Data;

        public class EntityData : EntityCollectionData<IStep>, IStepData, IChapterData, ILockableStepData
        {
            ///<inheritdoc />
            [DataMember]
            [DrawingPriority(0)]
            [HideInProcessInspector]
            public string Name { get; set; }

            ///<inheritdoc />
            [DataMember]
            [DrawingPriority(1)]
            public string Description { get; set; }

            ///<inheritdoc />
            [DataMember]
            [HideInProcessInspector]
            public IBehaviorCollection Behaviors { get; set; }

            ///<inheritdoc />
            [DataMember]
            [HideInProcessInspector]
            public ITransitionCollection Transitions { get; set; }

            public override IEnumerable<IStep> GetChildren()
            {
                return Steps.ToArray();
            }

            IEnumerable<IStepChild> IEntityCollectionData<IStepChild>.GetChildren()
            {
                return new List<IStepChild>
                    {
                        Behaviors,
                        Transitions,
                    };
            }

            ///<inheritdoc />
            [IgnoreDataMember]
            public IStepChild Current { get; set; }

            [IgnoreDataMember]
            public Dictionary<ITransition, ITransition> LinkedTransitions = new Dictionary<ITransition, ITransition>();

            ///<inheritdoc />
            public IMode Mode { get; set; }

            ///<inheritdoc />
            [HideInProcessInspector]
            public IEnumerable<LockablePropertyReference> ToUnlock { get; set; } = new List<LockablePropertyReference>();

            ///<inheritdoc />
            [HideInProcessInspector]
            public IDictionary<Guid, IEnumerable<Type>> TagsToUnlock { get; set; } = new Dictionary<Guid, IEnumerable<Type>>();

            /// <summary>
            /// The first step of the chapter.
            /// </summary>
            [DataMember]
            public IStep FirstStep { get; set; }

            /// <summary>
            /// All steps of the chapter.
            /// </summary>
            [DataMember]
            public IList<IStep> Steps { get; set; }

            [IgnoreDataMember]
            IStep IEntitySequenceData<IStep>.Current { get; set; }

            public EntityData()
            {
            }
        }

        private class ActivatingProcess : EntityIteratingProcess<IEntitySequenceDataWithMode<IStep>, IStep>
        {
            private readonly IStep firstStep;

            public ActivatingProcess(IChapterData data) : base(data)
            {
                firstStep = data.FirstStep;
            }

            private IEnumerator<IStep> enumerator;

            private IEnumerator<IStep> GetChildren()
            {
                IStep current = firstStep;

                while (current != null)
                {
                    yield return current;

                    current = current.Data.Transitions.Data.Transitions.First(transition => transition.IsCompleted).Data.TargetStep;
                }
            }

            /// <inheritdoc />
            public override void Start()
            {
                //((EntityData)Data).Transitions.Data.Transitions.First().Autocomplete();
                //Debug.Log(((EntityData)Data).Transitions.Data.Transitions.First().IsCompleted);

                enumerator = GetChildren();
                base.Start();
            }

            /// <inheritdoc />
            protected override bool ShouldActivateCurrent()
            {
                return true;
            }

            /// <inheritdoc />
            protected override bool ShouldDeactivateCurrent()
            {
                return Data.Current.Data.Transitions.Data.Transitions.Any(transition => transition.IsCompleted);
            }

            /// <inheritdoc />
            public override void End()
            {
                enumerator = null;

                //((EntityData)Data).Transitions.Data.Transitions.First().Autocomplete();

                //Debug.Log(((EntityData)Data).Transitions.Data.Transitions.First().IsCompleted);

                //foreach (ITransition transition in ((EntityData)Data).LinkedTransitions.Keys)
                //{
                //    if (transition.IsCompleted)
                //    {
                //        ((EntityData)Data).LinkedTransitions[transition].Autocomplete();
                //    }
                //}

                base.End();
            }

            /// <inheritdoc />
            protected override bool TryNext(out IStep entity)
            {
                if (enumerator != null && enumerator.MoveNext())
                {
                    entity = enumerator.Current;
                    return true;
                }
                else
                {
                    entity = null;
                    return false;
                }
            }

            /// <inheritdoc />
            public override void FastForward()
            {
                if (Data.Current == null)
                {
                    return;
                }

                if (Data.Current.FindPathInGraph(step => step.Data.Transitions.Data.Transitions.Select(transition => transition.Data.TargetStep), null, out IList<IStep> pathToChapterEnd) == false)
                {
                    throw new InvalidStateException("The end of the group is not reachable from the current step.");
                }

                foreach (IStep step in pathToChapterEnd)
                {
                    if (Data.Current.LifeCycle.Stage == Stage.Inactive)
                    {
                        Data.Current.LifeCycle.Activate();
                    }

                    Data.Current.LifeCycle.MarkToFastForward();

                    ITransition toAutocomplete = Data.Current.Data.Transitions.Data.Transitions.First(transition => transition.Data.TargetStep == step);
                    if (toAutocomplete.IsCompleted == false)
                    {
                        toAutocomplete.Autocomplete();
                    }

                    Data.Current.LifeCycle.Deactivate();

                    Data.Current = step;
                }
            }
        }

        private class ActiveProcess : StageProcess<EntityData>
        {
            private readonly IEnumerable<LockablePropertyData> toUnlock;

            public ActiveProcess(EntityData data) : base(data)
            {
            }

            ///<inheritdoc />
            public override void Start()
            {
            }

            ///<inheritdoc />
            public override IEnumerator Update()
            {
                while (Data.Transitions.Data.Transitions.Any(transition => transition.IsCompleted) == false)
                {
                    if( Data.Steps.SelectMany(step => step.Data.Transitions.Data.Transitions).Where(transition => transition.Data.TargetStep == null).Any(transition => transition.IsCompleted))
                        {
                        ITransition transition = Data.Transitions.Data.Transitions.First();

                        if (transition.LifeCycle.Stage == Stage.Inactive)
                        {
                            Data.Transitions.Data.Transitions.First().LifeCycle.Activate();
                        }
                    }

                    //foreach (Transition transition in ((EntityData)Data).LinkedTransitions.Keys)
                    //{
                    //    Debug.Log($"Checking {transition}. Completed: {transition.IsCompleted}");

                    //    if (transition.IsCompleted)
                    //    {
                    //        Debug.Log("Transition completed");
                    //        ((EntityData)Data).LinkedTransitions[transition].Autocomplete();
                    //    }
                    //}

                    yield return null;
                }
            }

            ///<inheritdoc />
            public override void End()
            {
            }

            ///<inheritdoc />
            public override void FastForward()
            {
            }
        }

        public override IStageProcess GetActivatingProcess()
        {
            return new CompositeProcess(new ActivatingProcess(Data), new FoldedActivatingProcess<IStepChild>(Data));
            return new ActivatingProcess(Data);
        }

        public override IStageProcess GetActiveProcess()
        {
            return new CompositeProcess(new FoldedActiveProcess<IStepChild>(Data), new ActiveProcess(Data));
        }

        /// <inheritdoc />
        public override IStageProcess GetDeactivatingProcess()
        {
            return new CompositeProcess(new FoldedDeactivatingProcess<IStepChild>(Data), new StopEntityIteratingProcess<IStep>(Data));
            return new StopEntityIteratingProcess<IStep>(Data);
        }

        /// <inheritdoc />
        protected override IConfigurator GetConfigurator()
        {
            return new SequenceConfigurator<IStep>(Data);
        }

        public StepGroup(string name, IStep firstStep)
        {
            Data.Name = name;   
            StepMetadata = new StepMetadata();
            ChapterMetadata = new ChapterMetadata();
            Data.Transitions = new TransitionCollection();
            Data.Behaviors = new BehaviorCollection();

            ChapterMetadata.Guid = Guid.NewGuid();

            Data.FirstStep = firstStep;
            Data.Steps = new ObservableCollection<IStep>();
            ((ObservableCollection<IStep>)Data.Steps).CollectionChanged += OnStepsChanged;

            if (firstStep != null)
            {
                Data.Steps.Add(firstStep);
            }

            if (LifeCycleLoggingConfig.Instance.LogChapters)
            {
                LifeCycle.StageChanged += (sender, args) =>
                {
                    Debug.LogFormat("<b>StepGroup</b> <i>'{0}'</i> is <b>{1}</b>.\n", Data.Name, LifeCycle.Stage.ToString());
                };
            }
        }

        private void OnStepsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IEnumerable<ITransition> emptyTransitions = Data.Steps.SelectMany(step => step.Data.Transitions.Data.Transitions).Where(transition => transition.Data.TargetStep == null);

            foreach (ITransition transition in emptyTransitions)
            {
                if(Data.LinkedTransitions.Keys.Contains(transition) == false)
                {
                    ITransition outgoingTransition = EntityFactory.CreateTransition();
                    Data.Transitions.Data.Transitions.Add(outgoingTransition);
                    Data.LinkedTransitions.Add(transition, outgoingTransition);
                }
            }

            IEnumerable<ITransition> keys = new List<ITransition>(Data.LinkedTransitions.Keys);
            foreach(ITransition transition in keys)
            {
                if (emptyTransitions.Contains(transition) == false)
                {
                    Data.Transitions.Data.Transitions.Remove(Data.LinkedTransitions[transition]);
                    Data.LinkedTransitions.Remove(transition);
                }
            }
        }
    }
}
