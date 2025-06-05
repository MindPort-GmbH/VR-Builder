// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Configuration.Modes;
using VRBuilder.Core.Properties;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils;
using VRBuilder.Unity;

namespace VRBuilder.Core.RestrictiveEnvironment
{
    /// <summary>
    /// Restricts interaction with scene objects by using LockableProperties, which are extracted from the <see cref="IStepData"/>.
    /// </summary>
    public class DefaultStepLockHandling : StepLockHandlingStrategy
    {
        private bool lockOnProcessStart = true;
        private bool lockOnProcessFinished = true;

        /// <inheritdoc />
        public override void Unlock(IStepData data, IEnumerable<LockablePropertyData> manualUnlocked)
        {
            IEnumerable<LockablePropertyData> unlockList = PropertyReflectionHelper.ExtractLockablePropertiesFromStep(data);
            unlockList = unlockList.Union(manualUnlocked);

            foreach (LockablePropertyData lockable in unlockList)
            {
                if (!lockable.Property.IsAlwaysUnlocked)
                {
                    lockable.Property.RequestLocked(false, data);
                }
            }
        }

        /// <inheritdoc />
        public override void Lock(IStepData data, IEnumerable<LockablePropertyData> manualUnlocked)
        {
            // All properties which should be locked
            IEnumerable<LockablePropertyData> lockList = PropertyReflectionHelper.ExtractLockablePropertiesFromStep(data);
            lockList = lockList.Union(manualUnlocked);

            ITransition completedTransition = data.Transitions.Data.Transitions.FirstOrDefault(transition => transition.IsCompleted);
            if (completedTransition != null)
            {
                IStepData nextStepData = GetNextStep(completedTransition);
                IEnumerable<LockablePropertyData> nextStepProperties = PropertyReflectionHelper.ExtractLockablePropertiesFromStep(nextStepData);

                if (nextStepData is ILockableStepData lockableStepData)
                {
                    IEnumerable<LockablePropertyData> toUnlock = lockableStepData.ToUnlock.Select(reference => new LockablePropertyData(reference.GetProperty()));

                    foreach (Guid tag in lockableStepData.GroupsToUnlock.Keys)
                    {
                        foreach (ISceneObject sceneObject in RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(tag))
                        {
                            toUnlock = toUnlock.Union(sceneObject.Properties.Where(property => lockableStepData.GroupsToUnlock[tag].Contains(property.GetType())).Select(property => new LockablePropertyData(property as LockableProperty))).ToList();
                        }
                    }

                    nextStepProperties = nextStepProperties.Union(toUnlock);
                }

                LockablePropertyData[] nextStepPropertyArray = nextStepProperties as LockablePropertyData[] ?? nextStepProperties.ToArray();
                if (completedTransition is ILockablePropertiesProvider completedLockableTransition)
                {
                    IEnumerable<LockablePropertyData> transitionLockList = completedLockableTransition.GetLockableProperties();
                    LockablePropertyData[] transitionLockListArray = transitionLockList as LockablePropertyData[] ?? transitionLockList.ToArray();

                    foreach (LockablePropertyData lockable in transitionLockListArray)
                    {
                        if (!lockable.Property.IsAlwaysUnlocked)
                        {
                            lockable.Property.RequestLocked(lockable.EndStepLocked && nextStepPropertyArray.Contains(lockable) == false, data);
                            lockable.Property.RemoveUnlocker(data);
                        }
                    }

                    // Remove all lockable from completed transition
                    lockList = lockList.Except(transitionLockListArray);
                }

                // Whether we lock the property or not, we remove the current step from the unlockers so it can be locked again in the future
                LockablePropertyData[] lockablePropertyArray = lockList as LockablePropertyData[] ?? lockList.ToArray();
                foreach (LockablePropertyData lockable in lockablePropertyArray)
                {
                    lockable.Property.RemoveUnlocker(data);
                }

                // Remove properties that stay unlocked from the list.
                lockList = lockablePropertyArray.Except(nextStepPropertyArray);
            }

            foreach (LockablePropertyData lockable in lockList)
            {
                // Fallback check if the property has the IsAlwaysUnlocked flag
                if (!lockable.Property.IsAlwaysUnlocked)
                {
                    lockable.Property.RequestLocked(true, data);
                }
            }
        }

        private IStepData GetNextStep(ITransition completedTransition)
        {
            if (completedTransition.Data.TargetStep != null)
            {
                return completedTransition.Data.TargetStep.Data;
            }

            if (ProcessRunner.IsRunning == false)
            {
                return null;
            }

            IProcessData process = ProcessRunner.Current.Data;
            // Test all chapters, but the last.
            for (int i = 0; i < process.Chapters.Count - 1; i++)
            {
                if (process.Chapters[i] == process.Current)
                {
                    if (process.Chapters[i + 1].Data.FirstStep != null)
                    {
                        return process.Chapters[i + 1].Data.FirstStep.Data;
                    }
                    break;
                }
            }
            // No next step found, seems to be the last.
            return null;
        }

        /// <inheritdoc />
        public override void Configure(IMode mode)
        {
            if (mode.ContainsParameter<bool>("LockOnProcessStart"))
            {
                lockOnProcessStart = mode.GetParameter<bool>("LockOnProcessStart");
            }

            if (mode.ContainsParameter<bool>("LockOnProcessFinished"))
            {
                lockOnProcessFinished = mode.GetParameter<bool>("LockOnProcessFinished");
            }
        }

        /// <inheritdoc />
        public override void OnProcessStarted(IProcess process)
        {
            if (lockOnProcessStart)
            {
                foreach (LockableProperty prop in SceneUtils.GetActiveAndInactiveComponents<LockableProperty>())
                {
                    if(prop.InheritSceneObjectLockState && !prop.IsAlwaysUnlocked)
                    {
                        prop.SetLocked(true);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void OnProcessFinished(IProcess process)
        {
            if (lockOnProcessFinished)
            {
                foreach (LockableProperty prop in SceneUtils.GetActiveAndInactiveComponents<LockableProperty>())
                {
                    if(prop.InheritSceneObjectLockState && !prop.IsAlwaysUnlocked)
                    {
                        prop.SetLocked(true);
                    }
                }
            }
        }
    }
}
