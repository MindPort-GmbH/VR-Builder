// Copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using VRBuilder.Core.Attributes;

namespace VRBuilder.Core.Editor.UI.NewStepInspector.Drawers
{
    /// <summary>
    /// Static registry that discovers and resolves UIToolkit element drawers.
    /// Mirrors <see cref="VRBuilder.Core.Editor.UI.Drawers.DrawerLocator"/> exactly.
    /// </summary>
    [InitializeOnLoad]
    public static class ElementDrawerLocator
    {
        private static readonly Dictionary<Type, IElementDrawer> allDrawers;
        private static readonly Dictionary<Type, IElementDrawer> defaultDrawers;
        private static readonly Dictionary<Type, IElementDrawer> instantiatorDrawers;
        private static readonly Dictionary<Type, IElementDrawer> resolvedDrawers;

        static ElementDrawerLocator()
        {
            defaultDrawers = new Dictionary<Type, IElementDrawer>();
            foreach (Type drawerType in TypeCache.GetTypesDerivedFrom<IElementDrawer>())
            {
                foreach (DefaultProcessElementDrawerAttribute attribute in drawerType.GetCustomAttributes<DefaultProcessElementDrawerAttribute>(true))
                {
                    defaultDrawers[attribute.DrawableType] = (IElementDrawer)Activator.CreateInstance(drawerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null);
                }
            }

            allDrawers = new Dictionary<Type, IElementDrawer>();
            foreach (Type drawerType in TypeCache.GetTypesDerivedFrom<IElementDrawer>())
            {
                allDrawers[drawerType] = (IElementDrawer)Activator.CreateInstance(drawerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null);
            }

            instantiatorDrawers = new Dictionary<Type, IElementDrawer>();
            foreach (Type drawerType in TypeCache.GetTypesDerivedFrom<IElementDrawer>()
                .Where(t => t.GetCustomAttributes<InstantiatorProcessElementDrawerAttribute>(true).Any()))
            {
                foreach (InstantiatorProcessElementDrawerAttribute attribute in drawerType.GetCustomAttributes<InstantiatorProcessElementDrawerAttribute>(true))
                {
                    instantiatorDrawers[attribute.Type] = (IElementDrawer)Activator.CreateInstance(drawerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null);
                }
            }

            resolvedDrawers = new Dictionary<Type, IElementDrawer>();
        }

        /// <summary>
        /// Returns the appropriate drawer for a given object member.
        /// Checks UsesSpecificProcessDrawerAttribute first, then resolves by type.
        /// </summary>
        public static IElementDrawer GetDrawerForMember(MemberInfo memberInfo, object owner)
        {
            if (memberInfo is not PropertyInfo && memberInfo is not FieldInfo)
            {
                return null;
            }

            if (HasCustomDrawer(memberInfo))
            {
                IElementDrawer customDrawer = GetCustomDrawer(memberInfo);
                if (customDrawer != null)
                {
                    return customDrawer;
                }
            }

            object actualValue = UI.Drawers.MemberAccessCache.GetValue(owner, memberInfo);
            return GetDrawerForValue(actualValue, memberInfo is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)memberInfo).FieldType);
        }

        /// <summary>
        /// Returns the appropriate drawer for a given value.
        /// </summary>
        public static IElementDrawer GetDrawerForValue(object value, Type declaredType)
        {
            if (value == null)
            {
                return GetDrawerForType(declaredType);
            }

            return GetDrawerForType(value.GetType());
        }

        /// <summary>
        /// Get a drawer for a view that creates a new instance of the given type.
        /// </summary>
        public static IElementDrawer GetInstantiatorDrawer(Type declaredType)
        {
            Type currentType = declaredType;

            while (currentType != null && currentType.IsInterface == false && currentType != typeof(object))
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

            return declaredType.GetInterfaces()
                .Where(i => instantiatorDrawers.ContainsKey(i))
                .Select(i => instantiatorDrawers[i])
                .FirstOrDefault(t => t != null);
        }

        private static IElementDrawer GetDrawerForType(Type type)
        {
            if (resolvedDrawers.TryGetValue(type, out IElementDrawer cachedDrawer))
            {
                return cachedDrawer;
            }

            Type currentType = type;

            while (currentType != null && currentType.IsInterface == false && currentType != typeof(object))
            {
                IElementDrawer concreteTypeDrawer = GetTypeDrawer(currentType);
                if (concreteTypeDrawer != null)
                {
                    resolvedDrawers[type] = concreteTypeDrawer;
                    return concreteTypeDrawer;
                }

                currentType = currentType.BaseType;
            }

            IElementDrawer interfaceDrawer = null;
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

            IElementDrawer objectDrawer = GetTypeDrawer(typeof(object));
            resolvedDrawers[type] = objectDrawer;
            return objectDrawer;
        }

        private static IElementDrawer GetTypeDrawer(Type type)
        {
            if (defaultDrawers.ContainsKey(type))
            {
                return defaultDrawers[type];
            }

            return null;
        }

        private static IElementDrawer GetInheritedInterfaceDrawer(Type type)
        {
            return type.GetInterfaces().Select(GetTypeDrawer).FirstOrDefault(t => t != null);
        }

        private static bool HasCustomDrawer(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes<UsesSpecificProcessDrawerAttribute>(true).Any();
        }

        private static IElementDrawer GetCustomDrawer(MemberInfo memberInfo)
        {
            UsesSpecificProcessDrawerAttribute attribute = memberInfo.GetCustomAttributes<UsesSpecificProcessDrawerAttribute>(true).First();
            string drawerTypeName = attribute.DrawerType;
            string[] splittedName = drawerTypeName.Split('.').Reverse().ToArray();

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
