using Newtonsoft.Json;
using System.Collections;
using System.Runtime.Serialization;
using VRBuilder.Core.Attributes;
using UnityEngine.Scripting;
using System;
using System.Linq;

namespace VRBuilder.Core.Behaviors
{
    /// <summary>
    /// This behavior changes the parent of a game object in the scene hierarchy. It can accept a null parent, in which case the object will be unparented.
    /// </summary>
    [DataContract(IsReference = true)]
    public class GoToChapterBehavior : Behavior<GoToChapterBehavior.EntityData>
    {
        [DisplayName("Start Chapter")]
        [DataContract(IsReference = true)]
        public class EntityData : IBehaviorData
        {
            [DataMember]
            public Guid ChapterGuid { get; set; }

            public Metadata Metadata { get; set; }
            public string Name { get; set; }
        }

        [JsonConstructor, Preserve]
        public GoToChapterBehavior()
        {
        }

        //public StartChapterBehavior(ISceneObject target, ISceneObject parent, bool snapToParentTransform = false) : this(ProcessReferenceUtils.GetNameFrom(target), ProcessReferenceUtils.GetNameFrom(parent), snapToParentTransform)
        //{
        //}

        //public StartChapterBehavior(string target, string parent, bool snapToParentTransform = false)
        //{
        //    Data.Target = new SceneObjectReference(target);
        //    Data.Parent = new SceneObjectReference(parent);
        //    Data.SnapToParentTransform = snapToParentTransform;
        //}

        private class ActivatingProcess : StageProcess<EntityData>
        {
            public ActivatingProcess(EntityData data) : base(data)
            {
            }

            /// <inheritdoc />
            public override void Start()
            {
            }

            /// <inheritdoc />
            public override IEnumerator Update()
            {
                yield return null;
            }

            /// <inheritdoc />
            public override void End()
            {
                if(Data.ChapterGuid == null || Data.ChapterGuid == Guid.Empty)
                {
                    return;
                }

                IChapter chapter = ProcessRunner.Current.Data.Chapters.FirstOrDefault(chapter => chapter.ChapterMetadata.Guid == Data.ChapterGuid);

                if (chapter != null)
                {
                    ProcessRunner.SetNextChapter(chapter);
                    ProcessRunner.SkipCurrentChapter();
                }
            }

            /// <inheritdoc />
            public override void FastForward()
            {
            }
        }

        /// <inheritdoc />
        public override IStageProcess GetActivatingProcess()
        {
            return new ActivatingProcess(Data);
        }
    }
}
