// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2026 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Provides cached reflection-based access for fields and properties used by editor UI drawers.
    /// </summary>
    /// <remarks>
    /// Reading members through reflection every frame is expensive. This class builds getter/setter delegates once and reuses them.
    /// It also caches type-level lookups, such as metadata members and the member named <c>Data</c>.
    /// A shared lock protects cache access so callers can safely use this from different threads.
    /// </remarks>
    internal static class MemberAccessCache
    {
        private sealed class Accessor
        {
            public Func<object, object> Getter { get; set; }
            public Action<object, object> Setter { get; set; }
            public Type DeclaredType { get; set; }
        }

        private sealed class MetadataMemberAccessor
        {
            public PropertyInfo Property { get; set; }
            public FieldInfo Field { get; set; }
        }

        private static readonly Dictionary<MemberInfo, Accessor> accessorsByMember = new Dictionary<MemberInfo, Accessor>();
        private static readonly Dictionary<Type, MetadataMemberAccessor> metadataMembersByType = new Dictionary<Type, MetadataMemberAccessor>();
        private static readonly Dictionary<Type, MemberInfo> dataMemberByType = new Dictionary<Type, MemberInfo>();
        private static readonly object locker = new object();

        /// <summary>
        /// Reads a field/property value from an object using a cached getter.
        /// </summary>
        /// <param name="owner">Object instance that contains the member.</param>
        /// <param name="memberInfo">Field or property to read.</param>
        /// <returns>Current value stored in the member.</returns>
        public static object GetValue(object owner, MemberInfo memberInfo)
        {
            return GetOrCreateAccessor(memberInfo).Getter(owner);
        }

        /// <summary>
        /// Writes a value to a field/property on an object using a cached setter.
        /// </summary>
        /// <param name="owner">Object instance that contains the member.</param>
        /// <param name="memberInfo">Field or property to write.</param>
        /// <param name="value">Value to assign to the member.</param>
        public static void SetValue(object owner, MemberInfo memberInfo, object value)
        {
            GetOrCreateAccessor(memberInfo).Setter(owner, value);
        }

        /// <summary>
        /// Gets the compile-time type declared by a field/property.
        /// </summary>
        /// <param name="memberInfo">Field or property to inspect.</param>
        /// <returns>Type declared on the member definition.</returns>
        public static Type GetDeclaredType(MemberInfo memberInfo)
        {
            return GetOrCreateAccessor(memberInfo).DeclaredType;
        }

        /// <summary>
        /// Finds members on a type that hold <see cref="Metadata"/> and caches the result.
        /// </summary>
        /// <param name="ownerType">Type to search.</param>
        /// <param name="property">Matching metadata property, if one exists.</param>
        /// <param name="field">Matching metadata field, if one exists.</param>
        /// <returns><c>true</c> if either output contains a member; otherwise <c>false</c>.</returns>
        public static bool TryGetMetadataMember(Type ownerType, out PropertyInfo property, out FieldInfo field)
        {
            MetadataMemberAccessor accessor;

            lock (locker)
            {
                if (metadataMembersByType.TryGetValue(ownerType, out accessor) == false)
                {
                    accessor = new MetadataMemberAccessor
                    {
                        Property = ownerType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .FirstOrDefault(prop => typeof(Metadata).IsAssignableFrom(prop.PropertyType)),
                        Field = ownerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                            .FirstOrDefault(fld => typeof(Metadata).IsAssignableFrom(fld.FieldType))
                    };

                    metadataMembersByType[ownerType] = accessor;
                }
            }

            property = accessor.Property;
            field = accessor.Field;
            return property != null || field != null;
        }

        /// <summary>
        /// Finds the member named <c>Data</c> on the owner's type and caches that lookup per type.
        /// </summary>
        /// <param name="owner">Object whose type should be inspected.</param>
        /// <returns>The <c>Data</c> member, or <c>null</c> when it does not exist.</returns>
        public static MemberInfo GetDataMember(object owner)
        {
            if (owner == null)
            {
                return null;
            }

            Type type = owner.GetType();
            lock (locker)
            {
                if (dataMemberByType.TryGetValue(type, out MemberInfo cachedMember))
                {
                    return cachedMember;
                }
            }

            MemberInfo dataMember = EditorReflectionUtils.GetFieldsAndPropertiesToDraw(owner).FirstOrDefault(member => member.Name == "Data");
            lock (locker)
            {
                dataMemberByType[type] = dataMember;
            }

            return dataMember;
        }

        /// <summary>
        /// Returns a cached accessor pair (getter/setter) for a member, creating it the first time.
        /// </summary>
        /// <param name="memberInfo">Field or property to access.</param>
        /// <returns>Cached accessor information for the requested member.</returns>
        private static Accessor GetOrCreateAccessor(MemberInfo memberInfo)
        {
            lock (locker)
            {
                if (accessorsByMember.TryGetValue(memberInfo, out Accessor accessor))
                {
                    return accessor;
                }

                accessor = CreateAccessor(memberInfo);
                accessorsByMember[memberInfo] = accessor;
                return accessor;
            }
        }

        /// <summary>
        /// Builds getter/setter delegates and declared type info for a field/property.
        /// </summary>
        /// <param name="memberInfo">Field or property to wrap.</param>
        /// <returns>Accessor object containing read/write delegates and declared type.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberInfo"/> is not a field or property.</exception>
        private static Accessor CreateAccessor(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return new Accessor
                {
                    DeclaredType = propertyInfo.PropertyType,
                    Getter = CreatePropertyGetter(propertyInfo),
                    Setter = CreatePropertySetter(propertyInfo)
                };
            }

            if (memberInfo is FieldInfo fieldInfo)
            {
                return new Accessor
                {
                    DeclaredType = fieldInfo.FieldType,
                    Getter = CreateFieldGetter(fieldInfo),
                    Setter = CreateFieldSetter(fieldInfo)
                };
            }

            throw new ArgumentException("Unsupported MemberInfo type.", nameof(memberInfo));
        }

        /// <summary>
        /// Creates a fast getter delegate for a property.
        /// </summary>
        /// <param name="propertyInfo">Property to read from.</param>
        /// <returns>Delegate that returns the property value for a given owner object.</returns>
        private static Func<object, object> CreatePropertyGetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetGetMethod(true) == null || propertyInfo.GetIndexParameters().Length > 0)
            {
                return owner => propertyInfo.GetValue(owner, null);
            }

            try
            {
                ParameterExpression ownerParam = Expression.Parameter(typeof(object), "owner");
                Expression typedOwner = Expression.Convert(ownerParam, propertyInfo.DeclaringType);
                Expression propertyAccess = Expression.Property(typedOwner, propertyInfo);
                UnaryExpression boxedResult = Expression.Convert(propertyAccess, typeof(object));
                return Expression.Lambda<Func<object, object>>(boxedResult, ownerParam).Compile();
            }
            catch
            {
                return owner => propertyInfo.GetValue(owner, null);
            }
        }

        /// <summary>
        /// Creates a fast setter delegate for a property.
        /// </summary>
        /// <param name="propertyInfo">Property to write to.</param>
        /// <returns>Delegate that assigns a value to the property on a given owner object.</returns>
        private static Action<object, object> CreatePropertySetter(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetSetMethod(true) == null
                || propertyInfo.GetIndexParameters().Length > 0
                || propertyInfo.DeclaringType == null
                || propertyInfo.DeclaringType.IsValueType)
            {
                return (owner, value) => propertyInfo.SetValue(owner, value, null);
            }

            try
            {
                ParameterExpression ownerParam = Expression.Parameter(typeof(object), "owner");
                ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");

                Expression typedOwner = Expression.Convert(ownerParam, propertyInfo.DeclaringType);
                Expression typedValue = Expression.Convert(valueParam, propertyInfo.PropertyType);
                BinaryExpression assign = Expression.Assign(Expression.Property(typedOwner, propertyInfo), typedValue);

                return Expression.Lambda<Action<object, object>>(assign, ownerParam, valueParam).Compile();
            }
            catch
            {
                return (owner, value) => propertyInfo.SetValue(owner, value, null);
            }
        }

        /// <summary>
        /// Creates a fast getter delegate for a field.
        /// </summary>
        /// <param name="fieldInfo">Field to read from.</param>
        /// <returns>Delegate that returns the field value for a given owner object.</returns>
        private static Func<object, object> CreateFieldGetter(FieldInfo fieldInfo)
        {
            try
            {
                ParameterExpression ownerParam = Expression.Parameter(typeof(object), "owner");
                Expression typedOwner = Expression.Convert(ownerParam, fieldInfo.DeclaringType);
                MemberExpression fieldAccess = Expression.Field(typedOwner, fieldInfo);
                UnaryExpression boxedResult = Expression.Convert(fieldAccess, typeof(object));
                return Expression.Lambda<Func<object, object>>(boxedResult, ownerParam).Compile();
            }
            catch
            {
                return fieldInfo.GetValue;
            }
        }

        /// <summary>
        /// Creates a fast setter delegate for a field.
        /// </summary>
        /// <param name="fieldInfo">Field to write to.</param>
        /// <returns>Delegate that assigns a value to the field on a given owner object.</returns>
        private static Action<object, object> CreateFieldSetter(FieldInfo fieldInfo)
        {
            if (fieldInfo.DeclaringType == null || fieldInfo.DeclaringType.IsValueType)
            {
                return fieldInfo.SetValue;
            }

            try
            {
                ParameterExpression ownerParam = Expression.Parameter(typeof(object), "owner");
                ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
                Expression typedOwner = Expression.Convert(ownerParam, fieldInfo.DeclaringType);
                Expression typedValue = Expression.Convert(valueParam, fieldInfo.FieldType);
                BinaryExpression assign = Expression.Assign(Expression.Field(typedOwner, fieldInfo), typedValue);

                return Expression.Lambda<Action<object, object>>(assign, ownerParam, valueParam).Compile();
            }
            catch
            {
                return fieldInfo.SetValue;
            }
        }
    }
}
