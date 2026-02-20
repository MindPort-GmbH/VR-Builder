// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using VRBuilder.Core;
using VRBuilder.Core.Editor;

namespace VRBuilder.Core.Editor.UI.Drawers
{
    /// <summary>
    /// Caches member accessors for hot editor draw paths to reduce repeated reflection overhead.
    /// </summary>
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

        public static object GetValue(object owner, MemberInfo memberInfo)
        {
            return GetOrCreateAccessor(memberInfo).Getter(owner);
        }

        public static void SetValue(object owner, MemberInfo memberInfo, object value)
        {
            GetOrCreateAccessor(memberInfo).Setter(owner, value);
        }

        public static Type GetDeclaredType(MemberInfo memberInfo)
        {
            return GetOrCreateAccessor(memberInfo).DeclaredType;
        }

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
                return owner => fieldInfo.GetValue(owner);
            }
        }

        private static Action<object, object> CreateFieldSetter(FieldInfo fieldInfo)
        {
            if (fieldInfo.DeclaringType == null || fieldInfo.DeclaringType.IsValueType)
            {
                return (owner, value) => fieldInfo.SetValue(owner, value);
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
                return (owner, value) => fieldInfo.SetValue(owner, value);
            }
        }
    }
}
