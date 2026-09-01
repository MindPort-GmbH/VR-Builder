// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using VRBuilder.Core.Attributes;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    [InitializeOnLoad]
    public static class DrawerLocator
    {
        private static readonly Dictionary<Type, IProcessDrawer> allDrawers;
        private static readonly Dictionary<Type, IProcessDrawer> defaultDrawers;
        private static readonly Dictionary<Type, IProcessDrawer> instantiatorDrawers;
        private static readonly Dictionary<Type, IProcessDrawer> resolvedDrawers;

        static DrawerLocator()
        {
            defaultDrawers = new Dictionary<Type, IProcessDrawer>();
            foreach (Type drawerType in TypeCache.GetTypesDerivedFrom<IProcessDrawer>())
            {
                foreach (DefaultProcessDrawerAttribute attribute in drawerType.GetCustomAttributes<DefaultProcessDrawerAttribute>(true))
                {
                    defaultDrawers[attribute.DrawableType] = (IProcessDrawer) Activator.CreateInstance(drawerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null);
                }
            }

            allDrawers = new Dictionary<Type, IProcessDrawer>();
            foreach (Type drawerType in TypeCache.GetTypesDerivedFrom<IProcessDrawer>())
            {
                allDrawers[drawerType] = (IProcessDrawer) Activator.CreateInstance(drawerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null);
            }

            instantiatorDrawers = new Dictionary<Type, IProcessDrawer>();
            foreach (Type drawerType in TypeCache.GetTypesDerivedFrom<IProcessDrawer>().Where(t => t.GetCustomAttributes<InstantiatorProcessDrawerAttribute>(true).Any()))
            {
                foreach (InstantiatorProcessDrawerAttribute attribute in drawerType.GetCustomAttributes<InstantiatorProcessDrawerAttribute>(true))
                {
                    instantiatorDrawers[attribute.Type] = (IProcessDrawer) Activator.CreateInstance(drawerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null);
                }
            }

            resolvedDrawers = new Dictionary<Type, IProcessDrawer>();
        }

        /// <summary>
        /// Returns required drawer for a given object member.
        /// If the member has a <see cref="UsesSpecificProcessDrawerAttribute"/>, returns the drawer defined in that attribute.
        /// If the member contains non-null value, tries to find drawers from most concrete type definition (which is actual value's type) and up through inheritance tree.
        /// If member's value is null, does the same starting from the property declared type instead.
        /// If drawer is still not found, checks drawers for interfaces.
        /// If everything else fails, returns the drawer for System.Object.
        /// </summary>
        /// <param name="memberInfo">Reflection information about the member for which drawer is needed.</param>
        /// <param name="owner">Object to which this member belongs to.</param>
        /// <returns>Returns suitable Process drawer. Returns null only if the member is not a property or field, or the specified custom drawer is not found.</returns>
        public static IProcessDrawer GetDrawerForMember(MemberInfo memberInfo, object owner)
        {
            if (memberInfo is not PropertyInfo && memberInfo is not FieldInfo)
            {
                return null;
            }

            if (HasCustomDrawer(memberInfo))
            {
                return GetCustomDrawer(memberInfo);
            }

            object actualValue = GetValue(memberInfo, owner);
            return GetDrawerForValue(actualValue, memberInfo is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)memberInfo).FieldType);
        }

        /// <summary>
        /// Returns required drawer for a given value.
        /// If the value is not null, try to find drawers from most concrete type definition (which is actual value's type) and up through inheritance tree.
        /// If the value is null, does the same starting from the member declaring type instead.
        /// If drawer is still not found, checks drawers for interfaces.
        /// If everything else fails, returns the drawer for System.Object.
        /// </summary>
        /// <param name="value">Value to get drawer for.</param>
        /// <param name="declaredType">Declaring type of the class member that contains the value.</param>
        /// <returns>Returns suitable Process drawer.</returns>
        public static IProcessDrawer GetDrawerForValue(object value, Type declaredType)
        {
            if (value == null)
            {
                return GetDrawerForType(declaredType);
            }

            return GetDrawerForType(value.GetType());
        }

        /// <summary>
        /// Get a drawer for a view that creates a new instance of <paramref name="declaredType"/>
        /// </summary>
        public static IProcessDrawer GetInstantiatorDrawer(Type declaredType)
        {
            Type currentType = declaredType;
            // Get drawer for type, checking from the most concrete type definition to a most abstract one.
            while (currentType.IsInterface == false && currentType != typeof(object))
            {
                if (instantiatorDrawers.ContainsKey(currentType))
                {
                    return instantiatorDrawers[currentType];
                }

                currentType = currentType.BaseType;
            }

            if (declaredType.IsInterface && instantiatorDrawers.ContainsKey(declaredType))
            {
                return instantiatorDrawers[declaredType];
            }

            return declaredType.GetInterfaces().Where(i => instantiatorDrawers.ContainsKey(i)).Select(i => instantiatorDrawers[i]).FirstOrDefault(t => t != null);
        }

        private static IProcessDrawer GetDrawerForType(Type type)
        {
            if (resolvedDrawers.TryGetValue(type, out IProcessDrawer cachedDrawer))
            {
                return cachedDrawer;
            }

            Type currentType = type;
            // Get drawer for type, checking from the most concrete type definition to a most abstract one.
            while (currentType.IsInterface == false && currentType != typeof(object))
            {
                IProcessDrawer concreteTypeDrawer = GetTypeDrawer(currentType);
                if (concreteTypeDrawer != null)
                {
                    resolvedDrawers[type] = concreteTypeDrawer;
                    return concreteTypeDrawer;
                }

                currentType = currentType.BaseType;
            }

            IProcessDrawer interfaceDrawer = null;
            if (type.IsInterface)
            {
                interfaceDrawer = GetTypeDrawer(type);
            }

            if (interfaceDrawer == null)
            {
                interfaceDrawer = GetInheritedInterfaceDrawer(type);
            }

            if (interfaceDrawer != null)
            {
                resolvedDrawers[type] = interfaceDrawer;
                return interfaceDrawer;
            }

            IProcessDrawer objectDrawer = GetObjectDrawer();
            resolvedDrawers[type] = objectDrawer;
            return objectDrawer;
        }

        private static object GetValue(MemberInfo memberInfo, object owner)
        {
            return MemberAccessCache.GetValue(owner, memberInfo);
        }

        private static IProcessDrawer GetTypeDrawer(Type type)
        {
            if (defaultDrawers.ContainsKey(type))
            {
                return defaultDrawers[type];
            }

            return null;
        }

        private static IProcessDrawer GetObjectDrawer()
        {
            return GetTypeDrawer(typeof(object));
        }

        private static IProcessDrawer GetInheritedInterfaceDrawer(Type type)
        {
            return type.GetInterfaces().Select(GetTypeDrawer).FirstOrDefault(t => t != null);
        }

        private static bool HasCustomDrawer(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes<UsesSpecificProcessDrawerAttribute>(true).Any();
        }

        private static IProcessDrawer GetCustomDrawer(MemberInfo memberInfo)
        {
            UsesSpecificProcessDrawerAttribute attribute = memberInfo.GetCustomAttributes<UsesSpecificProcessDrawerAttribute>(true).First();

            // ReSharper disable once PossibleNullReferenceException
            string drawerTypeName = attribute.DrawerType;
            string[] splittedName = drawerTypeName.Split('.').Reverse().ToArray();

            // Drawer name can be partially qualified
            Type drawerType = allDrawers.Keys.FirstOrDefault(key =>
            {
                string[] splittedKey = key.FullName.Split('.').Reverse().ToArray();

                if (splittedName.Length > splittedKey.Length)
                {
                    return false;
                }

                for (int i = 0; i < splittedName.Length; i++)
                {
                    if (splittedKey[i] != splittedName[i])
                    {
                        return false;
                    }
                }

                return true;
            });
            if (drawerType == null)
            {
                return null;
            }

            return allDrawers[drawerType];
        }
    }
}
