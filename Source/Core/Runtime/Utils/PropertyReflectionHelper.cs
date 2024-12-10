// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2024 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRBuilder.Core.Conditions;
using VRBuilder.Core.Configuration;
using VRBuilder.Core.Properties;
using VRBuilder.Core.RestrictiveEnvironment;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Unity;

namespace VRBuilder.Core.Utils
{
    /// <summary>
    /// Helper class which provides methods to extract <see cref="LockablePropertyData"/> from different process entities.
    /// </summary>
    internal static class PropertyReflectionHelper
    {
        public static List<LockablePropertyData> ExtractLockablePropertiesFromStep(IStepData data)
        {

            List<LockablePropertyData> result = new List<LockablePropertyData>();

            if (data == null)
            {
                return result;
            }

            foreach (ITransition transition in data.Transitions.Data.Transitions)
            {
                foreach (ICondition condition in transition.Data.Conditions)
                {
                    if (condition is ILockablePropertiesProvider lockablePropertiesProvider)
                    {
                        result.AddRange(lockablePropertiesProvider.GetLockableProperties());
                    }
                    else
                    {
                        result.AddRange(ExtractLockablePropertiesFromCondition(condition.Data));
                    }
                }
            }

            return result;
        }

        public static List<LockablePropertyData> ExtractLockablePropertiesFromCondition(IConditionData data, bool checkRequiredComponentsToo = true)
        {
            List<LockablePropertyData> result = new List<LockablePropertyData>();

            List<MemberInfo> memberInfo = GetAllPropertiesInGroupsFromCondition(data);
            memberInfo.ForEach(info =>
            {
                ProcessSceneReferenceBase reference = ReflectionUtils.GetValueFromPropertyOrField(data, info) as ProcessSceneReferenceBase;

                if (reference == null || reference.IsEmpty())
                {
                    return;
                }

                IEnumerable<ISceneObject> sceneObjects = new List<ISceneObject>();

                foreach (Guid guid in reference.Guids)
                {
                    sceneObjects = sceneObjects.Concat(RuntimeConfigurator.Configuration.SceneObjectRegistry.GetObjects(guid));

                }

                if (sceneObjects.Count() == 0)
                {
                    return;
                }

                IEnumerable<Type> refs = ExtractFittingPropertyType<LockableProperty>(reference.GetReferenceType());

                foreach (ISceneObject sceneObject in sceneObjects)
                {
                    Type refType = refs.Where(type => sceneObject.Properties.Select(property => property.GetType()).Contains(type)).FirstOrDefault();
                    if (refType != null)
                    {
                        IEnumerable<Type> types = new[] { refType };
                        if (checkRequiredComponentsToo)
                        {
                            types = GetDependenciesFrom<LockableProperty>(refType);
                        }

                        foreach (Type type in types)
                        {
                            LockableProperty property = sceneObject.Properties.FirstOrDefault(property => property.GetType() == type) as LockableProperty;
                            if (property != null)
                            {
                                result.Add(new LockablePropertyData(property));
                            }
                        }
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Returns all concrete runtime property types derived from T.
        /// </summary>
        public static IEnumerable<Type> ExtractFittingPropertyType<T>(Type referenceType) where T : ISceneObjectProperty
        {
            IEnumerable<Type> refs = ReflectionUtils.GetConcreteImplementationsOf(referenceType);
            refs = refs.Where(typeof(T).IsAssignableFrom);

            if (UnitTestChecker.IsUnitTesting == false)
            {
                refs = refs.Where(type => type.Assembly.GetReferencedAssemblies().All(name => name.Name != "nunit.framework"));
                if (Application.isEditor == false)
                {
                    refs = refs.Where(type => type.Assembly.GetReferencedAssemblies().All(name => name.Name != "UnityEditor"));
                }
            }

            return refs;
        }

        private static List<MemberInfo> GetAllPropertiesInGroupsFromCondition(IConditionData conditionData)
        {
            List<MemberInfo> memberInfo = conditionData.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info =>
                    info.PropertyType.IsConstructedGenericType && info.PropertyType.GetGenericTypeDefinition() ==
                    typeof(SingleScenePropertyReference<>))
                .Cast<MemberInfo>()
                .ToList();

            memberInfo.AddRange(conditionData.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(info =>
                    info.PropertyType.IsConstructedGenericType && info.PropertyType.GetGenericTypeDefinition() ==
                    typeof(MultipleScenePropertyReference<>)));

            memberInfo.AddRange(conditionData.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(info =>
                    info.FieldType.IsConstructedGenericType && info.FieldType.GetGenericTypeDefinition() ==
                    typeof(SingleScenePropertyReference<>)));

            memberInfo.AddRange(conditionData.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(info =>
                    info.FieldType.IsConstructedGenericType && info.FieldType.GetGenericTypeDefinition() ==
                    typeof(MultipleScenePropertyReference<>)));

            return memberInfo;
        }

        /// <summary>
        /// Get process scene properties which the given type dependence on, which has to be a subclass of <T>
        /// </summary>
        private static IEnumerable<Type> GetDependenciesFrom<T>(Type processProperty) where T : ISceneObjectProperty
        {
            List<Type> dependencies = new List<Type>();
            IEnumerable<Type> requiredComponents = processProperty.GetCustomAttributes(typeof(RequireComponent), false)
                .Cast<RequireComponent>()
                .SelectMany(rq => new[] { rq.m_Type0, rq.m_Type1, rq.m_Type2 });

            foreach (Type requiredComponent in requiredComponents)
            {
                if (requiredComponent != null && requiredComponent.IsSubclassOf(typeof(T)))
                {
                    dependencies.AddRange(GetDependenciesFrom<T>(requiredComponent));
                }
            }

            dependencies.Add(processProperty);
            return new HashSet<Type>(dependencies);
        }
    }
}
