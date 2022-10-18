namespace RoadRegistry.Tests.Framework.Assertions;

using System.Reflection;
using AutoFixture.Idioms;
using AutoFixture.Kernel;

public class ConstructorBasedEqualityOperatorEqualsSelfAssertion : IdiomaticAssertion
{
    public ConstructorBasedEqualityOperatorEqualsSelfAssertion(ISpecimenBuilder builder, ConstructorInfo constructor)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        Constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
    }

    public ISpecimenBuilder Builder { get; }
    public ConstructorInfo Constructor { get; }

    public override void Verify(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var method = type
            .GetMethods()
            .SingleOrDefault(candidate =>
                candidate.Name == "op_Equality"
                && candidate.GetParameters().Length == 2
                && candidate.GetParameters()[0].ParameterType == type
                && candidate.GetParameters()[1].ParameterType == type);

        if (method == null)
            throw new EqualityOperatorException(type, $"The type {type.Name} does not implement an equality operator for {type.Name}.");

        var selfParameters = new object[Constructor.GetParameters().Length];
        foreach (var parameter in Constructor.GetParameters()) selfParameters[parameter.Position] = Builder.CreateAnonymous(parameter.ParameterType);

        var self = Constructor.Invoke(selfParameters);

        object result;
        try
        {
            result = method.Invoke(null, new[] { self, self });
        }
        catch (Exception exception)
        {
            throw new EqualityOperatorException(type, $"The equality operator of type {type.Name} threw an exception", exception);
        }

        if (!(bool)result) throw new EqualityOperatorException(type);
    }
}