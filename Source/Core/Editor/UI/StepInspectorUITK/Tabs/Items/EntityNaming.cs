// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using VRBuilder.Core.Attributes;

namespace VRBuilder.Core.Editor.UI.StepInspectorUITK.Tabs.Items
{
    /// <summary>
    /// Resolves the visible title for any behavior / condition / transition by following
    /// the same fallback order the legacy inspector used:
    ///   1. dynamic <see cref="INamedData"/>.Name when non-empty (so e.g. an ObjectInRange
    ///      condition reads "Move Cube within 1 unit of Target")
    ///   2. <see cref="DisplayNameAttribute"/> on the Data type (e.g. "Object Nearby")
    ///   3. <see cref="DisplayNameAttribute"/> on the entity type itself
    ///   4. the entity's type name as a last-resort
    /// </summary>
    internal static class EntityNaming
    {
        public static string ResolveTitle(IEntity entity)
        {
            if (entity == null)
            {
                return "Entity";
            }

            object data = entity is IDataOwner owner ? owner.Data : null;

            // 1. Dynamic INamedData.Name takes precedence (shown live in legacy too).
            if (data is INamedData named && string.IsNullOrEmpty(named.Name) == false)
            {
                return named.Name;
            }

            // 2. [DisplayName] on the Data type.
            if (data != null)
            {
                DisplayNameAttribute dataAttr = GetDisplayName(data.GetType());
                if (dataAttr != null && string.IsNullOrEmpty(dataAttr.Name) == false)
                {
                    return dataAttr.Name;
                }
            }

            // 3. [DisplayName] on the entity type itself.
            DisplayNameAttribute entityAttr = GetDisplayName(entity.GetType());
            if (entityAttr != null && string.IsNullOrEmpty(entityAttr.Name) == false)
            {
                return entityAttr.Name;
            }

            // 4. Type name fallback.
            return entity.GetType().Name;
        }

        private static DisplayNameAttribute GetDisplayName(System.Type type)
        {
            object[] attrs = type.GetCustomAttributes(typeof(DisplayNameAttribute), inherit: true);
            return attrs.Length > 0 ? attrs[0] as DisplayNameAttribute : null;
        }
    }
}
