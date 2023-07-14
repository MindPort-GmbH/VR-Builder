// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2023 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Serialization.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using VRBuilder.Core.EntityOwners;
using VRBuilder.Core.Behaviors;
using UnityEngine;

namespace VRBuilder.Core.Serialization
{
    /// <summary>
    /// Improved version of the NewtonsoftJsonProcessSerializer, which now allows to serialize very long chapters.
    /// </summary>
    public class NewtonsoftJsonProcessSerializerV4 : NewtonsoftJsonProcessSerializer
    {
        /// <inheritdoc/>
        public override string Name { get; } = "Newtonsoft Json Importer v4";

        protected override int Version { get; } = 4;

        /// <inheritdoc/>
        public override IProcess ProcessFromByteArray(byte[] data)
        {
            string stringData = new UTF8Encoding().GetString(data);
            JObject dataObject = JsonConvert.DeserializeObject<JObject>(stringData, ProcessSerializerSettings);

            // Check if process was serialized with version 1
            int version = dataObject.GetValue("$serializerVersion").ToObject<int>();
            if (version == 1)
            {
                return base.ProcessFromByteArray(data);
            }
            if(version == 2)
            {
                return new ImprovedNewtonsoftJsonProcessSerializer().ProcessFromByteArray(data);
            }
            if(version == 3)
            {
                return new NewtonsoftJsonProcessSerializerV3().ProcessFromByteArray(data);
            }

            ProcessWrapper wrapper = Deserialize<ProcessWrapper>(data, ProcessSerializerSettings);
            return wrapper.GetProcess();
        }

        /// <inheritdoc/>
        public override byte[] ProcessToByteArray(IProcess process)
        {
            ProcessWrapper wrapper = new ProcessWrapper(process);
            JObject jObject = JObject.FromObject(wrapper, JsonSerializer.Create(ProcessSerializerSettings));
            jObject.Add("$serializerVersion", Version);
            // This line is required to undo the changes applied to the process.
            wrapper.GetProcess();

            return new UTF8Encoding().GetBytes(jObject.ToString());
        }

        [Serializable]
        private class ProcessWrapper
        {
            [DataMember]
            public List<IStep> Steps = new List<IStep>();

            [DataMember]
            public IProcess Process;

            public ProcessWrapper()
            {

            }

            public ProcessWrapper(IProcess process)
            {
                foreach (IChapter chapter in process.Data.Chapters)
                {
                    Steps.AddRange(GetSteps(chapter));
                }

                foreach (IStep step in Steps)
                {
                    foreach (ITransition transition in step.Data.Transitions.Data.Transitions)
                    {
                        if (transition.Data.TargetStep != null)
                        {
                            transition.Data.TargetStep = new StepRef() { StepMetadata = new StepMetadata() { Guid = transition.Data.TargetStep.StepMetadata.Guid } };
                        }
                    }
                }
                Process = process;                
            }

            public IProcess GetProcess()
            {
                foreach (IStep step in Steps)
                {
                    foreach (ITransition transition in step.Data.Transitions.Data.Transitions)
                    {
                        if (transition.Data.TargetStep == null)
                        {
                            continue;
                        }

                        StepRef stepRef = (StepRef) transition.Data.TargetStep;
                        transition.Data.TargetStep = Steps.FirstOrDefault(step => step.StepMetadata.Guid == stepRef.StepMetadata.Guid);
                    }
                }

                return Process;
            }

            private IEnumerable<IStep> GetSteps(IChapter chapter)
            {
                List<IStep> steps = new List<IStep>();

                steps.AddRange(chapter.Data.Steps);

                IEnumerable<IChapter> subChapters = chapter.Data.Steps.SelectMany(step => step.Data.Behaviors.Data.Behaviors.Where(behavior => behavior.Data is IEntityCollectionData<IChapter>))
                    .Select(behavior => behavior.Data)
                    .Cast<IEntityCollectionData<IChapter>>()
                    .SelectMany(behavior => behavior.GetChildren());

                foreach(IChapter subChapter in subChapters)
                {
                    steps.AddRange(GetSteps(subChapter)); 
                }

                return steps;
            }

            private IEnumerable<IChapter> GetSubChapters(IChapter chapter)
            {
                List<IChapter> subChapters = new List<IChapter>();

                foreach (IStep step in chapter.Data.Steps) 
                {
                    foreach (IBehavior behavior in step.Data.Behaviors.Data.Behaviors)
                    {
                        if(behavior.Data is IEntityCollectionData<IChapter> data)
                        {
                            subChapters.AddRange(data.GetChildren());

                            foreach(IChapter subChapter in data.GetChildren())
                            {
                                subChapters.AddRange(GetSubChapters(subChapter));
                            }
                        }
                    }
                }

                return subChapters;
            }

            [Serializable]
            public class StepRef : IStep
            {
                IData IDataOwner.Data { get; } = null;

                IStepData IDataOwner<IStepData>.Data { get; } = null;

                public ILifeCycle LifeCycle { get; } = null;

                public IStageProcess GetActivatingProcess()
                {
                    throw new NotImplementedException();
                }

                public IStageProcess GetActiveProcess()
                {
                    throw new NotImplementedException();
                }

                public IStageProcess GetDeactivatingProcess()
                {
                    throw new NotImplementedException();
                }

                public void Configure(IMode mode)
                {
                    throw new NotImplementedException();
                }

                public void Update()
                {
                    throw new NotImplementedException();
                }

                public IStep Clone()
                {
                    throw new NotImplementedException();
                }

                public StepMetadata StepMetadata { get; set; }
                public IEntity Parent { get; set; }
            }
        }
    }
}
