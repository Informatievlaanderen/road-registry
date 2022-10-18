namespace RoadRegistry.Tests.BackOffice.Messages;

using System.Text;
using RoadRegistry.BackOffice.Messages;
using Xunit;
using Xunit.Abstractions;

public class SchemaDump
{
    private readonly ITestOutputHelper _output;

    public SchemaDump(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact(Skip = "Useful to run when you want to dump the event schema as markdown")]
    public void DumpMarkdownSchema()
    {
        var primitiveTypes = new HashSet<Type>(
            new[]
            {
                typeof(bool),
                typeof(int),
                typeof(long),
                typeof(double),
                typeof(decimal),
                typeof(string),
                typeof(DateTime),
                typeof(DateTimeOffset)
            });
        var builder = new StringBuilder();
        var allDataTypes = new HashSet<Type>();
        builder.AppendLine("# Events");
        builder.AppendLine();
        foreach (var @event in RoadNetworkEvents.All)
        {
            builder.AppendFormat("## {0}", @event.Name);
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine("| Property | Data Type |");
            builder.AppendLine("| ---- | ---- |");
            foreach (var property in @event.GetProperties())
                if (property.PropertyType.IsArray)
                {
                    var elementType = property.PropertyType.GetElementType();
                    if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var nullableElementType = Nullable.GetUnderlyingType(elementType);
                        if (primitiveTypes.Contains(nullableElementType))
                        {
                            builder.AppendFormat("| {0} | {1}? array |", property.Name, nullableElementType.Name);
                        }
                        else
                        {
                            builder.AppendFormat("| {0} | [{1}](#{2})? array |", property.Name, nullableElementType.Name, nullableElementType.Name.ToLowerInvariant());
                            allDataTypes.Add(nullableElementType);
                        }
                    }
                    else if (primitiveTypes.Contains(elementType))
                    {
                        builder.AppendFormat("| {0} | {1} array |", property.Name, elementType.Name);
                    }
                    else
                    {
                        builder.AppendFormat("| {0} | [{1}](#{2}) array |", property.Name, elementType.Name, elementType.Name.ToLowerInvariant());
                        allDataTypes.Add(elementType);
                    }

                    builder.AppendLine();
                }
                else
                {
                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var nullablePropertyType = Nullable.GetUnderlyingType(property.PropertyType);
                        if (primitiveTypes.Contains(nullablePropertyType))
                        {
                            builder.AppendFormat("| {0} | {1}? |", property.Name, nullablePropertyType.Name);
                        }
                        else
                        {
                            builder.AppendFormat("| {0} | [{1}](#{2})? |", property.Name, nullablePropertyType.Name, nullablePropertyType.Name.ToLowerInvariant());
                            allDataTypes.Add(nullablePropertyType);
                        }
                    }
                    else if (primitiveTypes.Contains(property.PropertyType))
                    {
                        builder.AppendFormat("| {0} | {1} |", property.Name, property.PropertyType.Name);
                    }
                    else
                    {
                        builder.AppendFormat("| {0} | [{1}](#{2}) |", property.Name, property.PropertyType.Name, property.PropertyType.Name.ToLowerInvariant());
                        allDataTypes.Add(property.PropertyType);
                    }

                    builder.AppendLine();
                }

            builder.AppendLine();
        }

        var dataTypes = new Stack<Type>(allDataTypes);
        builder.AppendLine("# Data Types");
        builder.AppendLine();
        while (dataTypes.Count != 0)
        {
            var dataType = dataTypes.Pop();
            builder.AppendFormat("## {0}", dataType.Name);
            builder.AppendLine();
            builder.AppendLine();
            if (dataType.IsEnum)
            {
                builder.AppendLine("| Value |");
                builder.AppendLine("| ---- |");
                foreach (var value in Enum.GetNames(dataType))
                {
                    builder.AppendFormat("| {0} |", value);
                    builder.AppendLine();
                }
            }
            else
            {
                builder.AppendLine("| Property | Data Type |");
                builder.AppendLine("| ---- | ---- |");
                foreach (var property in dataType.GetProperties())
                    if (property.PropertyType.IsArray)
                    {
                        var elementType = property.PropertyType.GetElementType();
                        if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var nullableElementType = Nullable.GetUnderlyingType(elementType);
                            if (primitiveTypes.Contains(nullableElementType))
                            {
                                builder.AppendFormat("| {0} | {1}? array |", property.Name, nullableElementType.Name);
                            }
                            else
                            {
                                builder.AppendFormat("| {0} | [{1}](#{2})? array |", property.Name,
                                    nullableElementType.Name, nullableElementType.Name.ToLowerInvariant());
                                if (!allDataTypes.Contains(nullableElementType))
                                {
                                    dataTypes.Push(nullableElementType);
                                    allDataTypes.Add(nullableElementType);
                                }
                            }
                        }
                        else if (primitiveTypes.Contains(elementType))
                        {
                            builder.AppendFormat("| {0} | {1} array |", property.Name, elementType.Name);
                        }
                        else
                        {
                            builder.AppendFormat("| {0} | [{1}](#{2}) array |", property.Name, elementType.Name,
                                elementType.Name.ToLowerInvariant());
                            if (!allDataTypes.Contains(elementType))
                            {
                                dataTypes.Push(elementType);
                                allDataTypes.Add(elementType);
                            }
                        }

                        builder.AppendLine();
                    }
                    else
                    {
                        if (property.PropertyType.IsGenericType &&
                            property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var nullablePropertyType = Nullable.GetUnderlyingType(property.PropertyType);
                            if (primitiveTypes.Contains(nullablePropertyType))
                            {
                                builder.AppendFormat("| {0} | {1}? |", property.Name, nullablePropertyType.Name);
                            }
                            else
                            {
                                builder.AppendFormat("| {0} | [{1}](#{2})? |", property.Name, nullablePropertyType.Name,
                                    nullablePropertyType.Name.ToLowerInvariant());
                                if (!allDataTypes.Contains(nullablePropertyType))
                                {
                                    dataTypes.Push(nullablePropertyType);
                                    allDataTypes.Add(nullablePropertyType);
                                }
                            }
                        }
                        else if (primitiveTypes.Contains(property.PropertyType))
                        {
                            builder.AppendFormat("| {0} | {1} |", property.Name, property.PropertyType.Name);
                        }
                        else
                        {
                            builder.AppendFormat("| {0} | [{1}](#{2}) |", property.Name, property.PropertyType.Name,
                                property.PropertyType.Name.ToLowerInvariant());
                            if (!allDataTypes.Contains(property.PropertyType))
                            {
                                dataTypes.Push(property.PropertyType);
                                allDataTypes.Add(property.PropertyType);
                            }
                        }

                        builder.AppendLine();
                    }
            }

            builder.AppendLine();
        }

        _output.WriteLine(builder.ToString());
    }

    [Fact(Skip = "Useful to run when you want to review message names")]
    public void DumpMessageNames()
    {
        var message = string.Join(Environment.NewLine, typeof(RoadNetworkEvents).Assembly.GetTypes()
            .Where(type => type.Namespace != null && type.Namespace.StartsWith(typeof(RoadNetworkEvents).Namespace) && !type.IsNested)
            .Select(type => type.Name)
            .OrderBy(name => name));
        _output.WriteLine("Message Names:");
        _output.WriteLine(message);
    }

    [Fact(Skip = "Useful to run when you want to review message property names")]
    public void DumpMessageProperties()
    {
        var message = string.Join(Environment.NewLine, typeof(RoadNetworkEvents).Assembly.GetTypes()
            .Where(type => type.Namespace != null && type.Namespace.StartsWith(typeof(RoadNetworkEvents).Namespace) && !type.IsNested)
            .SelectMany(type => type.GetProperties())
            .Select(property => property.Name)
            .Distinct()
            .OrderBy(name => name));
        _output.WriteLine("Propery Names:");
        _output.WriteLine(message);
    }
}