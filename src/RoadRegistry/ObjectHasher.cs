namespace RoadRegistry.BackOffice;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using NetTopologySuite.Geometries;

public static class ObjectHasher
{
    public static IEnumerable<string> GetHashFields(object item)
    {
        var hashFields = GetHashFields(item, null);
        return hashFields;
    }

    private static void AddValueToFields(object item, object value, List<string> fields)
    {
        if (value == null)
        {
            fields.Add(string.Empty);
            return;
        }

        switch (value)
        {
            case string:
            {
                fields.AddRange(GetHashFields(value, item));
                break;
            }
            case IEnumerable enumerable:
            {
                foreach (var enumerableItem in enumerable)
                {
                    fields.AddRange(GetHashFields(enumerableItem, item));
                }

                break;
            }
            default:
            {
                fields.AddRange(GetHashFields(value, item));
                break;
            }
        }
    }

    private static IEnumerable<string> GetHashFields(object item, object parentItem)
    {
        if (item == null)
        {
            return Array.Empty<string>();
        }

        var type = item.GetType();
        var fields = new List<string>();

        if (UseToStringOnValue(type))
        {
            fields.Add(item.ToString());
            return fields;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead).ToArray();
        if (!properties.Any())
        {
            fields.Add(item.ToString());
            return fields;
        }

        if (parentItem != null && item is IHaveHashFields haveHashFields)
        {
            fields.AddRange(haveHashFields.GetHashFields());
            return fields;
        }

        if (item is Geometry geometry)
        {
            fields.Add(geometry.ToText());
            return fields;
        }

        foreach (var pi in properties)
        {
            if (pi.GetIndexParameters().Any())
            {
                continue;
            }

            var value = pi.GetValue(item);
            AddValueToFields(item, value, fields);
        }

        return fields;
    }

    private static bool UseToStringOnValue(Type type)
    {
        if (type == typeof(string) || (type.IsPrimitive && !type.IsClass) || type.IsValueType)
        {
            return true;
        }

        return false;
    }
}
