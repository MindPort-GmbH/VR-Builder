using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using VRBuilder.Core.Attributes;
using VRBuilder.Core.Editor.UI.Drawers;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Drawers
{
    /// <summary>
    /// Resolves <see cref="IElementDrawer"/> instances by type. UITK counterpart of
    /// <see cref="VRBuilder.Core.Editor.UI.Drawers.DrawerLocator"/>.
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
                foreach (DefaultProcessElementDrawerAttribute attribute in
                    drawerType.GetCustomAttributes<DefaultProcessElementDrawerAttribute>(true))
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
                foreach (InstantiatorProcessElementDrawerAttribute attribute in
                    drawerType.GetCustomAttributes<InstantiatorProcessElementDrawerAttribute>(true))
                {
                    instantiatorDrawers[attribute.Type] = (IElementDrawer)Activator.CreateInstance(drawerType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null);
                }
            }

            resolvedDrawers = new Dictionary<Type, IElementDrawer>();
        }

        public static IElementDrawer GetDrawerForMember(MemberInfo memberInfo, object owner)
        {
            if (memberInfo is not PropertyInfo && memberInfo is not FieldInfo)
            {
                return null;
            }

            if (HasCustomDrawer(memberInfo))
            {
                return GetCustomDrawer(memberInfo);
            }

            object actualValue = MemberAccessCache.GetValue(owner, memberInfo);
            return GetDrawerForValue(actualValue, memberInfo is PropertyInfo prop ? prop.PropertyType : ((FieldInfo)memberInfo).FieldType);
        }

        public static IElementDrawer GetDrawerForValue(object value, Type declaredType)
        {
            if (value == null)
            {
                return GetDrawerForType(declaredType);
            }

            return GetDrawerForType(value.GetType());
        }

        public static IElementDrawer GetInstantiatorDrawer(Type declaredType)
        {
            Type currentType = declaredType;
            while (currentType != null && currentType.IsInterface == false && currentType != typeof(object))
            {
                if (instantiatorDrawers.TryGetValue(currentType, out IElementDrawer drawer))
                {
                    return drawer;
                }

                currentType = currentType.BaseType;
            }

            if (declaredType.IsInterface && instantiatorDrawers.TryGetValue(declaredType, out IElementDrawer ifaceDrawer))
            {
                return ifaceDrawer;
            }

            return declaredType
                .GetInterfaces()
                .Where(i => instantiatorDrawers.ContainsKey(i))
                .Select(i => instantiatorDrawers[i])
                .FirstOrDefault();
        }

        private static IElementDrawer GetDrawerForType(Type type)
        {
            if (type == null)
            {
                return GetObjectDrawer();
            }

            if (resolvedDrawers.TryGetValue(type, out IElementDrawer cachedDrawer))
            {
                return cachedDrawer;
            }

            Type currentType = type;
            while (currentType != null && currentType.IsInterface == false && currentType != typeof(object))
            {
                IElementDrawer concreteDrawer = GetTypeDrawer(currentType);
                if (concreteDrawer != null)
                {
                    resolvedDrawers[type] = concreteDrawer;
                    return concreteDrawer;
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

            IElementDrawer objectDrawer = GetObjectDrawer();
            if (objectDrawer != null)
            {
                resolvedDrawers[type] = objectDrawer;
            }
            return objectDrawer;
        }

        private static IElementDrawer GetTypeDrawer(Type type)
        {
            return defaultDrawers.TryGetValue(type, out IElementDrawer drawer) ? drawer : null;
        }

        // May return null until ObjectElementDrawer is registered (Phase 2).
        private static IElementDrawer GetObjectDrawer()
        {
            return GetTypeDrawer(typeof(object));
        }

        private static IElementDrawer GetInheritedInterfaceDrawer(Type type)
        {
            return type.GetInterfaces().Select(GetTypeDrawer).FirstOrDefault(d => d != null);
        }

        private static bool HasCustomDrawer(MemberInfo memberInfo)
        {
            return memberInfo.GetCustomAttributes<UsesSpecificProcessDrawerAttribute>(true).Any();
        }

        private static IElementDrawer GetCustomDrawer(MemberInfo memberInfo)
        {
            UsesSpecificProcessDrawerAttribute attribute =
                memberInfo.GetCustomAttributes<UsesSpecificProcessDrawerAttribute>(true).First();

            string drawerTypeName = attribute.DrawerType;
            string[] splittedName = drawerTypeName.Split('.').Reverse().ToArray();

            // Drawer name in the attribute can be partially qualified.
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

            return drawerType == null ? null : allDrawers[drawerType];
        }
    }
}
