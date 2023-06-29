using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class ExecuteChaptersBehaviorTests : BehaviorTests
    {
        private List<GameObject> createdObjects = new List<GameObject>();

        private BooleanDataProperty CreateBooleanPropertyObject(string name = "Boolean")
        {
            GameObject boolean = new GameObject(name);
            createdObjects.Add(boolean);
            return boolean.AddComponent<BooleanDataProperty>();
        }

        protected override IBehavior CreateDefaultBehavior()
        {
            return new ExecuteChaptersBehavior(EntityFactory.CreateChapter("Step Group"));
        }

        [TearDown]
        public void DestroyBooleanObject()
        {
            foreach(GameObject obj in createdObjects)
            {
                GameObject.DestroyImmediate(obj);
            }

            createdObjects.Clear();
        }

        [UnityTest]
        public IEnumerator DoesNotActivateIfThreadIncomplete()
        {
            // Given a execute chapters behavior with one thread that will not complete immediately,            
            IStep step1 = EntityFactory.CreateStep("CompletingStep");
            IChapter chapter1 = EntityFactory.CreateChapter("Thread 1");
            chapter1.Data.Steps.Add(step1);
            chapter1.Data.FirstStep = step1;

            IStep step2 = EntityFactory.CreateStep("BlockingStep");
            step2.Data.Transitions.Data.Transitions.First().Data.Conditions.Add(new TimeoutCondition(10f));
            IChapter chapter2 = EntityFactory.CreateChapter("Thread 2");
            chapter2.Data.Steps.Add(step2);
            chapter2.Data.FirstStep = step2;

            ExecuteChaptersBehavior behavior = new ExecuteChaptersBehavior(new IChapter[] { chapter1, chapter2 });
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate it and wait for the first thread to complete,
            behavior.LifeCycle.Activate();

            while (Stage.Active != chapter1.LifeCycle.Stage)
            {
                yield return null;
                behavior.Update();
            }

            // Then the behavior does not complete because a thread is still running.
            Assert.AreEqual(Stage.Active, chapter1.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, chapter2.LifeCycle.Stage);
            Assert.AreEqual(Stage.Activating, behavior.LifeCycle.Stage);
        }

        [UnityTest]
        public IEnumerator ThreadsAreExecutedAtTheSameTime()
        {
            // Given a execute chapters behavior,            
            BooleanDataProperty chapterExecuted1 = CreateBooleanPropertyObject("1");
            BooleanDataProperty chapterExecuted2 = CreateBooleanPropertyObject("2");

            IStep step1 = EntityFactory.CreateStep("Set boolean 1");
            step1.Data.Behaviors.Data.Behaviors.Add(new SetValueBehavior<bool>(chapterExecuted1, true));
            IChapter chapter1 = EntityFactory.CreateChapter("Thread 1");
            chapter1.Data.Steps.Add(step1);
            chapter1.Data.FirstStep = step1;

            IStep step2 = EntityFactory.CreateStep("Set boolean 2");
            step2.Data.Behaviors.Data.Behaviors.Add(new SetValueBehavior<bool>(chapterExecuted2, true));
            IChapter chapter2 = EntityFactory.CreateChapter("Thread 2");
            chapter2.Data.Steps.Add(step2);
            chapter2.Data.FirstStep = step2;

            ExecuteChaptersBehavior behavior = new ExecuteChaptersBehavior(new IChapter[] { chapter1, chapter2 });
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate it,
            behavior.LifeCycle.Activate();

            while (Stage.Active != chapter1.LifeCycle.Stage)
            {
                yield return null;
                behavior.Update();
            }

            // Then the steps are executed.
            Assert.AreEqual(Stage.Active, chapter2.LifeCycle.Stage);
            Assert.IsTrue(chapterExecuted1.GetValue());
            Assert.IsTrue(chapterExecuted2.GetValue());
        }

        [UnityTest]
        public IEnumerator StepsAreExecuted()
        {
            // Given a execute chapters behavior,            
            BooleanDataProperty chapterExecuted = CreateBooleanPropertyObject();
            IStep step = EntityFactory.CreateStep("Set boolean");
            step.Data.Behaviors.Data.Behaviors.Add(new SetValueBehavior<bool>(chapterExecuted, true));
            IChapter chapter = EntityFactory.CreateChapter("Step Group");
            chapter.Data.Steps.Add(step);
            chapter.Data.FirstStep = step;
            ExecuteChapterBehavior behavior = new ExecuteChapterBehavior(chapter);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we activate it,
            behavior.LifeCycle.Activate();

            while (Stage.Active != behavior.LifeCycle.Stage)
            {
                yield return null;
                behavior.Update();
            }

            // Then the steps are executed.
            Assert.AreEqual(Stage.Active, chapter.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(chapterExecuted.GetValue());
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndActivateIt()
        {
            // Given a execute chapters behavior,            
            BooleanDataProperty chapterExecuted = CreateBooleanPropertyObject();
            IStep step = EntityFactory.CreateStep("Set boolean");
            step.Data.Behaviors.Data.Behaviors.Add(new SetValueBehavior<bool>(chapterExecuted, true));
            IChapter chapter = EntityFactory.CreateChapter("Step Group");
            chapter.Data.Steps.Add(step);
            chapter.Data.FirstStep = step;
            ExecuteChapterBehavior behavior = new ExecuteChapterBehavior(chapter);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we mark it to fast-forward and activate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.AreEqual(Stage.Active, behavior.Data.Chapter.LifeCycle.Stage);
            Assert.IsTrue(chapterExecuted.GetValue());
            yield break;
        }

        [UnityTest]
        public IEnumerator FastForwardInactiveBehaviorAndDeactivateIt()
        {
            // Given a execute chapters behavior,
            BooleanDataProperty chapterExecuted = CreateBooleanPropertyObject();
            IStep step = EntityFactory.CreateStep("Set boolean");
            step.Data.Behaviors.Data.Behaviors.Add(new SetValueBehavior<bool>(chapterExecuted, true));
            IChapter chapter = EntityFactory.CreateChapter("Step Group");
            chapter.Data.Steps.Add(step);
            chapter.Data.FirstStep = step;
            ExecuteChapterBehavior behavior = new ExecuteChapterBehavior(chapter);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            // When we mark it to fast-forward, activate and immediately deactivate it,
            behavior.LifeCycle.MarkToFastForward();
            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.IsTrue(chapterExecuted.GetValue());
        }

        [UnityTest]
        public IEnumerator FastForwardActivatingBehavior()
        {
            // Given a execute chapters behavior,
            BooleanDataProperty chapterExecuted = CreateBooleanPropertyObject();
            IStep step = EntityFactory.CreateStep("Set boolean");
            step.Data.Behaviors.Data.Behaviors.Add(new SetValueBehavior<bool>(chapterExecuted, true));
            IChapter chapter = EntityFactory.CreateChapter("Step Group");
            chapter.Data.Steps.Add(step);
            chapter.Data.FirstStep = step;
            ExecuteChapterBehavior behavior = new ExecuteChapterBehavior(chapter);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Activating)
            {
                yield return null;
                behavior.Update();
            }

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Active, behavior.LifeCycle.Stage);
            Assert.IsTrue(chapterExecuted.GetValue());
        }

        [UnityTest]
        public IEnumerator FastForwardDeactivatingBehavior()
        {
            // Given a execute chapters behavior,
            BooleanDataProperty chapterExecuted = CreateBooleanPropertyObject();
            IStep step = EntityFactory.CreateStep("Set boolean");
            step.Data.Behaviors.Data.Behaviors.Add(new SetValueBehavior<bool>(chapterExecuted, true));
            IChapter chapter = EntityFactory.CreateChapter("Step Group");
            chapter.Data.Steps.Add(step);
            chapter.Data.FirstStep = step;
            ExecuteChapterBehavior behavior = new ExecuteChapterBehavior(chapter);
            behavior.Configure(RuntimeConfigurator.Configuration.Modes.CurrentMode);

            behavior.LifeCycle.Activate();

            while (behavior.LifeCycle.Stage != Stage.Active)
            {
                yield return null;
                behavior.Update();
            }

            behavior.LifeCycle.Deactivate();

            while (behavior.LifeCycle.Stage != Stage.Deactivating)
            {
                yield return null;
                behavior.Update();
            }

            // When we mark it to fast-forward,
            behavior.LifeCycle.MarkToFastForward();

            // Then it autocompletes immediately.
            Assert.AreEqual(Stage.Inactive, behavior.LifeCycle.Stage);
            Assert.IsTrue(chapterExecuted.GetValue());
        }
    }
}