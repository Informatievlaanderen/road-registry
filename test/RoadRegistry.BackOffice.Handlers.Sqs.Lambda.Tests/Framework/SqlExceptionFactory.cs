namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;

using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.Data.SqlClient;

public static class SqlExceptionFactory
{
    public static SqlException Create(int number, string message = "Mocked SQL exception")
    {
        var tSqlError           = typeof(SqlError);
        var tSqlErrorCollection = typeof(SqlErrorCollection);

        // 1) Create SqlError without calling its ctor
#pragma warning disable SYSLIB0050
        var error = FormatterServices.GetUninitializedObject(tSqlError);
#pragma warning restore SYSLIB0050

        // 2) Set fields via reflection
        void Set(string fieldName, object? value)
        {
            var f = tSqlError.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f == null) throw new MissingFieldException(tSqlError.FullName, fieldName);
            f.SetValue(error, value);
        }

        Set("_number", number);
        Set("_message", message);
        Set("_server", "server");
        Set("_procedure", "proc");
        Set("_lineNumber", 0);
        Set("_state", (byte)0);

        // 3) Create SqlErrorCollection and add error
        var errorCollection = Activator.CreateInstance(tSqlErrorCollection, nonPublic: true)!;
        var addMethod = tSqlErrorCollection.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)!;
        addMethod.Invoke(errorCollection, [error]);

        // 4) Call internal static SqlException.CreateException(errors, serverVersion, Guid)
        var createMethod = typeof(SqlException)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "CreateException" &&
                        m.GetParameters().Length >= 2 &&
                        m.GetParameters()[0].ParameterType == tSqlErrorCollection);

        var ex = createMethod.Invoke(null, [errorCollection, "11.0.0"]);

        return (SqlException)ex!;
    }
}
