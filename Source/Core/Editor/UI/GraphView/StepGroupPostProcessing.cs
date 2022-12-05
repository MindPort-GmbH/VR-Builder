using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRBuilder.Core.Behaviors;

namespace VRBuilder.Core
{
    public class StepGroupPostProcessing : EntityPostProcessing<IStep>
    {
        public override void Execute(IStep entity)
        {
            if (entity.StepMetadata.StepType == "stepGroup")
            {
                Debug.Log("Postprocessing group");

                StepGroup stepGroup = (StepGroup)entity;

                IStep step1 = EntityFactory.CreateStep("step 1");
                step1.Data.Behaviors.Data.Behaviors.Add(new DelayBehavior(2));

                IStep step2 = EntityFactory.CreateStep("step 2");
                step2.Data.Behaviors.Data.Behaviors.Add(new DelayBehavior(2));

                step1.Data.Transitions.Data.Transitions[0].Data.TargetStep = step2;

                stepGroup.Data.Steps.Add(step1);
                stepGroup.Data.Steps.Add(step2);

                stepGroup.Data.FirstStep = step1;
            }
        }
    }
}
