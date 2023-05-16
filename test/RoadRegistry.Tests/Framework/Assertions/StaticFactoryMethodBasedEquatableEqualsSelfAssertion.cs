namespace RoadRegistry.Tests.Framework.Assertions;

using System.Reflection;
using AutoFixture.Idioms;
using AutoFixture.Kernel;

public class StaticFactoryMethodBasedEquatableEqualsSelfAssertion : IdiomaticAssertion
{
    public StaticFactoryMethodBasedEquatableEqualsSelfAssertion(ISpecimenBuilder builder, MethodInfo constructor)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        Constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
    }

    public ISpecimenBuilder Builder { get; }
    public MethodInfo Constructor { get; }

    public override void Verify(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var equatableType = typeof(IEquatable<>).MakeGenericType(type);
        if (!equatableType.IsAssignableFrom(type))
        {
            throw new EquatableEqualsException(type, $"The type {type.Name} does not implement IEquatable<{type.Name}>.");
        }

        var method = equatableType.GetMethods().Single();
        var selfParameters = new object[Constructor.GetParameters().Length];
        foreach (var parameter in Constructor.GetParameters())
        {
            selfParameters[parameter.Position] = Builder.CreateAnonymous(parameter.ParameterType);
        }

        var self = Constructor.Invoke(null, selfParameters);

        object result;
        try
        {
            result = method.Invoke(self, new[] { self });
        }
        catch (Exception exception)
        {
            throw new EquatableEqualsException(type, $"The IEquatable<{type.Name}>.Equals method of type {type.Name} threw an exception: {exception}", exception);
        }

        if (!(bool)result)
        {
            throw new EquatableEqualsException(type);
        }
    }
}