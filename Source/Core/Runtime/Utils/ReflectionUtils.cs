// Copyright (c) 2013-2019 Innoactive GmbH
// Licensed under the Apache License, Version 2.0
// Modifications copyright (c) 2021-2025 MindPort GmbH

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VRBuilder.Core.Attributes;

namespace VRBuilder.Core.Utils
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Return <paramref name="type"/> name taking into consideration if it is nested type or not.
        /// </summary>
        public static string GetNameWithNesting(this Type type)
        {
            return type.MemberType == MemberTypes.NestedType ? string.Concat(GetNameWithNesting(type.DeclaringType), "+", type.Name) : type.Name;

        }

        /// <summary>
        /// If the given <paramref name="list"/> implements IList<>, return its generic type argument. Otherwise, return typeof(object).
        /// </summary>
        public static Type GetEntryType(object list)
        {
            Type entryDeclaredType = typeof(object);

            Type genericListType = list.GetType()
                .GetInterfaces()
                .Where(implementedInterface => implementedInterface.IsGenericType)
                .FirstOrDefault(implementedInterface => implementedInterface.GetGenericTypeDefinition() == typeof(IList<>));
            if (genericListType != null)
            {
                entryDeclaredType = genericListType.GetGenericArguments()[0];
            }

            return entryDeclaredType;
        }

        /// <summary>
        /// If the given <paramref name="listType"/> is IList{T}, return its generic type argument. Otherwise, return typeof(object).
        /// </summary>
        public static Type GetEntryType(Type listType)
        {
            Type entryDeclaredType = typeof(object);
            Type genericListType = null;

            if (listType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                genericListType = listType;
            }
            else
            {
                genericListType = listType
                    .GetInterfaces()
                    .Where(implementedInterface => implementedInterface.IsGenericType)
                    .FirstOrDefault(implementedInterface => implementedInterface.GetGenericTypeDefinition() == typeof(IList<>));
            }

            if (genericListType != null)
            {
                entryDeclaredType = genericListType.GetGenericArguments()[0];
            }

            return entryDeclaredType;
        }

        private static Type[] cachedTypes;

        /// <summary>
        /// Returns all existing types of all assemblies.
        /// </summary>
        public static IEnumerable<Type> GetAllTypes()
        {
            if (cachedTypes == null)
            {
                cachedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        return e.Types.Where(type => type != null);
                    }
                }).ToArray();
            }

            return cachedTypes;
        }

        /// <summary>
        /// Returns non-abstract classes that implement or inherit from given type.
        /// </summary>
        public static IEnumerable<Type> GetConcreteImplementationsOf(Type baseType)
        {
            return GetAllTypes()
                .Where(baseType.IsAssignableFrom)
                .Where(type => type.IsClass && type.IsAbstract == false);
        }

        /// <summary>
        /// Determines whether the specified type originates from a non-editor assembly.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <c>true</c> if the type's assembly does not reference UnityEditor or nunit.framework; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNonEditorType(this Type type)
        {
            return type.Assembly.GetReferencedAssemblies()
                .All(a => a.Name != "UnityEditor" && a.Name != "nunit.framework");
        }

        /// <summary>
        /// Retrieves all public, non-abstract classes assignable from the specified base type.
        /// Optionally filters out types from editor assemblies.
        /// </summary>
        /// <param name="baseType">The base type to check against.</param>
        /// <param name="excludeEditor">If set to <c>true</c>, types from editor assemblies are excluded.</param>
        /// <returns>An enumerable of types assignable from <paramref name="baseType"/>.</returns>
        public static IEnumerable<Type> GetConcreteTypesAssignableFrom(Type baseType, bool excludeEditor = true)
        {
            return GetAllTypes()
                .Where(baseType.IsAssignableFrom)
                .Where(t => t.IsConcretePublicClass())
                .Where(t => !excludeEditor || t.IsNonEditorType());
        }

        /// <summary>
        /// Finds the first concrete implementation of the specified base type that has a DefaultImplementationAttribute with a matching ConcreteType.
        /// </summary>
        /// <param name="baseType">The base type for which to find an implementation.</param>
        /// <param name="excludeEditor">If set to <c>true</c>, types from editor assemblies are excluded.</param>
        /// <returns>
        /// The concrete type that matches the DefaultImplementationAttribute; otherwise, <c>null</c> if none is found.
        /// </returns>
        public static Type GetImplementationWithDefaultAttribute(Type baseType, bool excludeEditor = true)
        {
            return GetConcreteTypesAssignableFrom(baseType, excludeEditor)
                .FirstOrDefault(t => t.GetCustomAttribute<DefaultSceneObjectPropertyAttribute>()?.ConcreteType == baseType);
        }

        /// <summary>
        /// Finds the first concrete implementation of the specified base type that does not have a DefaultImplementationAttribute.
        /// </summary>
        /// <param name="baseType">The base type for which to find an implementation.</param>
        /// <param name="excludeEditor">If set to <c>true</c>, types from editor assemblies are excluded.</param>
        /// <returns>
        /// The concrete type without a DefaultImplementationAttribute; otherwise, <c>null</c> if none is found.
        /// </returns>
        public static Type GetImplementationWithoutDefaultAttribute(Type baseType, bool excludeEditor = true)
        {
            return GetConcreteTypesAssignableFrom(baseType, excludeEditor)
                .FirstOrDefault(t => t.GetCustomAttribute<DefaultSceneObjectPropertyAttribute>() == null);
        }

        /// <summary>
        /// Filters all types from <see cref="GetAllTypes"/> to return those that are assignable from the specified <paramref name="propertyType"/>,
        /// are public, and are not pointer or by‑reference types, excluding types from editor assemblies.
        /// </summary>
        /// <param name="propertyType">The type of the property to compare against.</param>
        /// <returns>A list of types matching the criteria.</returns>
        public static List<Type> GetFilteredPropertyTypes(Type propertyType)
        {
            return GetAllTypes()
                .Where(type => type.IsAssignableFrom(propertyType))
                .Where(type => type.IsPublic && !type.IsPointer && !type.IsByRef)
                .Where(type => type.IsNonEditorType())
                .ToList();
        }

        /// <summary>
        /// Filters all types from <see cref="GetAllTypes"/> to return those that implement any of the specified extension interface types,
        /// are concrete (non-abstract) classes, are public, and are not from editor assemblies.
        /// </summary>
        /// <param name="extensionTypes">A collection of extension interface types to match against.</param>
        /// <returns>A list of available concrete extension types matching the criteria.</returns>
        public static List<Type> GetFilteredAvailableExtensions(IEnumerable<Type> extensionTypes)
        {
            return GetAllTypes()
                .Where(type => extensionTypes.Any(ext => ext.IsAssignableFrom(type)))
                .Where(type => type.IsConcretePublicClass())
                .Where(type => type.IsNonEditorType())
                .ToList();
        }


        /// <summary>
        /// Helper method to check if a type is a concrete public class.
        /// </summary>
        private static bool IsConcretePublicClass(this Type type)
        {
            return type.IsClass && !type.IsAbstract && type.IsPublic;
        }

        /// <summary>
        /// Returns <paramref name="types"/> which have attribute <typeparamref name="T"/>.
        /// </summary>
        public static IEnumerable<Type> WhichHaveAttribute<T>(this IEnumerable<Type> types) where T : Attribute
        {
            return types.Where(type => type.GetCustomAttributes(typeof(T), false).Length != 0);
        }

        /// <summary>
        /// Returns first attribute <typeparamref name="T"/> of <paramref name="type"/>
        /// </summary>
        public static T GetAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
        }

        /// <summary>
        /// Returns non-abstract classes that implement or inherit from given type.
        /// </summary>
        public static IEnumerable<Type> GetConcreteImplementationsOf<T>()
        {
            return GetConcreteImplementationsOf(typeof(T));
        }

        /// <summary>
        /// Creates instance of given type using public or protected constructor with no parameters.
        /// </summary>
        public static object CreateInstanceOfType(Type type)
        {
            return Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Array.Empty<object>(), null);
        }

        /// <summary>
        /// Creates instance of given type using public or protected constructor with no parameters.
        /// </summary>
        public static T CreateInstanceOfType<T>()
        {
            return (T)CreateInstanceOfType(typeof(T));
        }

        /// <summary>
        /// Returns default instance of type.
        /// </summary>
        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }

        /// <summary>
        /// Returns generic IDictionary interface which is implemented by the type of <paramref name="dictionaryValue"/>.
        /// If <paramref name="dictionaryValue"/>'s type does not implement it, returns null.
        /// </summary>
        public static Type GetGenericDictionaryInterface(object dictionaryValue)
        {
            if (dictionaryValue == null)
            {
                return null;
            }

            return dictionaryValue.GetType()
                .GetInterfaces()
                .Where(implementedInterface => implementedInterface.IsGenericType)
                .FirstOrDefault(implementedInterface => implementedInterface.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        /// <summary>
        /// Remove an element from <paramref name="list"/> at <paramref name="index"/>. If the list is fixed size, new instance is created.
        /// </summary>
        public static void RemoveFromList(ref IList list, int index)
        {
            if (list.IsFixedSize)
            {
                IList temp = (IList)Activator.CreateInstance(list.GetType(), list.Count - 1);
                for (int j = 0; j < index; j++)
                {
                    temp[j] = list[j];
                }

                for (int j = index + 1; j < list.Count; j++)
                {
                    temp[j - 1] = list[j];
                }

                list = temp;
                return;
            }

            list.RemoveAt(index);
        }

        /// <summary>
        /// Insert a <paramref name="value"/> in <paramref name="list"/> at index <paramref name="index"/>. If the list is fixed size, new instance is created.
        /// </summary>
        public static void InsertIntoList(ref IList list, int index, object value)
        {
            if (list.IsFixedSize)
            {
                IList temp = (IList)Activator.CreateInstance(list.GetType(), list.Count + 1);
                for (int i = 0; i < list.Count; i++)
                {
                    if (i < index)
                    {
                        temp[i] = list[i];
                    }
                    else
                    {
                        temp[i + 1] = list[i];
                    }
                }

                temp[index] = value;
                list = temp;
                return;
            }

            list.Insert(index, value);
        }

        /// <summary>
        /// Replace <paramref name="list"/> with a <paramref name="newList"/>. New instance is created.
        /// </summary>
        public static void ReplaceList<T>(ref IList list, IEnumerable<T> newList)
        {
            // Completely enumerate collection to know its size.
            T[] newArray = newList.ToArray();

            IList result = (IList)Activator.CreateInstance(list.GetType(), newArray.Length);

            for (int i = 0; i < newArray.Length; i++)
            {
                if (result.Count != newArray.Length)
                {
                    result.Add(newArray[i]);
                }
                else
                {
                    result[i] = newArray[i];
                }
            }

            list = result;
        }

        /// <summary>
        /// Get declared type of field/property <paramref name="info"/>.
        /// </summary>
        public static Type GetDeclaredTypeOfPropertyOrField(MemberInfo info)
        {
            if (IsProperty(info))
            {
                return ((PropertyInfo)info).PropertyType;
            }

            return IsField(info) ? ((FieldInfo)info).FieldType : null;

        }

        /// <summary>
        /// Is given <paramref name="memberInfo"/> a PropertyInfo?
        /// </summary>
        public static bool IsProperty(MemberInfo memberInfo)
        {
            return memberInfo is PropertyInfo;
        }

        /// <summary>
        /// Is given <paramref name="memberInfo"/> a FieldInfo?
        /// </summary>
        public static bool IsField(MemberInfo memberInfo)
        {
            return memberInfo is FieldInfo;
        }

        /// <summary>
        /// Set <paramref name="value"/> to property/field <paramref name="member"/> of object <paramref name="owner"/>.
        /// </summary>
        public static void SetValueToPropertyOrField(object owner, MemberInfo member, object value)
        {
            PropertyInfo property = member as PropertyInfo;
            FieldInfo field = member as FieldInfo;

            if (property != null)
            {
                property.SetValue(owner, value, null);
            }

            if (field != null)
            {
                field.SetValue(owner, value);
            }
        }

        /// <summary>
        /// Get value from property/field <paramref name="member"/> of object <paramref name="owner"/>
        /// </summary>
        public static object GetValueFromPropertyOrField(object owner, MemberInfo member)
        {
            PropertyInfo property = member as PropertyInfo;
            FieldInfo field = member as FieldInfo;

            if (property != null)
            {
                return property.GetValue(owner, null);
            }

            return field.GetValue(owner);
        }

        /// <summary>
        /// Returns the type from <paramref name="assemblyQualifiedName"/> or null if not found.
        /// </summary>
        public static Type GetTypeFromAssemblyQualifiedName(string assemblyQualifiedName)
        {
            return string.IsNullOrEmpty(assemblyQualifiedName) ? null : GetAllTypes().FirstOrDefault(type => type.AssemblyQualifiedName == assemblyQualifiedName);

        }

        /// <summary>
        /// Return an IEnumerable of types which inherit <typeparamref name="T"/> and are not inherited by any other type.
        /// It is sorted by priority. <paramref name="lowestPriorityTypes"/> come at the end.
        /// </summary>
        public static IEnumerable<Type> GetFinalImplementationsOf<T>(params Type[] lowestPriorityTypes)
        {
            IEnumerable<Type> types = GetConcreteImplementationsOf<T>()
                .Where(type => type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(constructor => constructor.GetParameters().Length != 0 == false))
                .ToList();

            IEnumerable<Type> typesWithoutInheritors = types.Where(type => types.Any(compareWith => type != compareWith && type.IsAssignableFrom(compareWith)) == false).ToList();

            typesWithoutInheritors = typesWithoutInheritors.OrderBy(lowestPriorityTypes.Contains);

            return typesWithoutInheritors;
        }

        /// <summary>
        /// Check if <paramref name="type"/> inherits from <paramref name="genericDefinition"/>.
        /// </summary>
        public static bool IsSubclassOfGenericDefinition(this Type typeToCheck, Type genericDefinition)
        {
            if (genericDefinition.IsGenericTypeDefinition == false)
            {
                throw new ArgumentException("genericDefinition has to be generic definition.");
            }

            Type type = typeToCheck;

            while (type != null && type != typeof(object))
            {
                Type recursiveType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (genericDefinition == recursiveType)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return typeToCheck.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && genericDefinition == interfaceType.GetGenericTypeDefinition());
        }

        /// <summary>
        /// Determines if the given object is empty.
        /// </summary>
        public static bool IsEmpty(object value)
        {
            switch (value)
            {
                case null:
                case string s when string.IsNullOrEmpty(s):
                    return true;
            }
            return value.GetType().GetInterfaces().Contains(typeof(ICanBeEmpty)) && ((ICanBeEmpty)value).IsEmpty();
        }

        /// <summary>
        /// Checks if the given type is a number.
        /// </summary>
        public static bool IsNumeric(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double
                    or TypeCode.Single => true,
                _ => false
            };
        }
    }
}
