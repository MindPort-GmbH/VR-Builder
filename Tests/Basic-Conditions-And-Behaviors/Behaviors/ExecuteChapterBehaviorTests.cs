using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using VRBuilder.Core.Behaviors;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Tests.Utils;

namespace VRBuilder.Core.Tests.Behaviors
{
    public class ExecuteChapterBehaviorTests : BehaviorTests
    {
        GameObject boolean;

        private BooleanDataProperty CreateBooleanPropertyObject()
        {
            boolean = new GameObject("Boolean");
            return boolean.AddComponent<BooleanDataProperty>();
        }

        protected override IBehavior CreateDefaultBehavior()
        {
            return new ExecuteChapterBehavior(EntityFactory.CreateChapter("Step Group"));
        }

        [TearDown]
        public void DestroyBooleanObject()
        {
            GameObject.DestroyImmediate(boolean);
        }

        [UnityTest]
        public IEnumerator StepsAreExecuted()
        {
            // Given a execute chapter behavior,            
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
            // Given a execute chapter behavior,            
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
            // Given a execute chapter behavior,
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
            // Given a execute chapter behavior,
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
            // Given a execute chapter behavior,
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