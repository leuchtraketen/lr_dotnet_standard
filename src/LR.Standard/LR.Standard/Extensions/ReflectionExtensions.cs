using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LR.Standard;

public static class ReflectionExtensions
{
    public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(Type mappedType)
    {
        return mappedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
    }

    public static object? GetMemberValue(object? obj, MemberInfo member)
    {
        if (member is PropertyInfo mp)
        {
            return mp.GetValue(obj, null);
        }

        if (member is FieldInfo mf)
        {
            return mf.GetValue(obj);
        }

        throw new NotSupportedException("GetMemberValue: " + member.Name);
    }

    public static IEnumerable<FieldInfo> GetAllFields(Type t)
    {
        if (t == null) return Enumerable.Empty<FieldInfo>();

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        return t.GetFields(flags).Concat(GetAllFields(t.GetTypeInfo().BaseType));
    }

    public static IEnumerable<Type> GetAllBaseTypes(this Type type)
    {
        // is there any base type?
        if (type == null)
        {
            yield break;
        }

        // return all implemented or inherited interfaces
        foreach (Type i in type.GetInterfaces())
        {
            yield return i;
        }

        // return all inherited types

/* Nicht gemergte Änderung aus Projekt "LR.Standard (netcoreapp3.0)"
Vor:
            var currentBaseType = type.BaseType;
Nach:
            Type? currentBaseType = type.BaseType;
*/
        Type currentBaseType = type.BaseType;
        while (currentBaseType != null)
        {
            yield return currentBaseType;
            currentBaseType = currentBaseType.BaseType;
        }
    }

    public static void CopyProperties(this object source, object destination)
    {
        // If any this null throw an exception
        if (source == null || destination == null)
            throw new Exception("Source or/and Destination Objects are null");
        // Getting the Types of the objects
        Type typeDest = destination.GetType();
        Type typeSrc = source.GetType();

        // Iterate the Properties of the source instance and
        // populate them from their desination counterparts
        PropertyInfo[] srcProps = typeSrc.GetProperties();
        foreach (PropertyInfo srcProp in srcProps)
        {
            if (!srcProp.CanRead)
            {
                continue;
            }

            PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
            if (targetProperty == null)
            {
                continue;
            }

            if (!targetProperty.CanWrite)
            {
                continue;
            }

            if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate)
            {
                continue;
            }

            if (( targetProperty.GetSetMethod().Attributes & MethodAttributes.Static ) != 0)
            {
                continue;
            }

            if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType))
            {
                continue;
            }

            // Passed all tests, lets set the value
            targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
        }
    }
}
