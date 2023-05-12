namespace RoadRegistry.Tests.Framework.Assertions;

using AutoFixture.Idioms;
using AutoFixture.Kernel;

public class EqualityOperatorEqualsOtherAssertion : IdiomaticAssertion
{
    public EqualityOperatorEqualsOtherAssertion(ISpecimenBuilder builder)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
    }

    public ISpecimenBuilder Builder { get; }

    public override void Verify(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        var method = type
            .GetMethods()
            .SingleOrDefault(candidate =>
                candidate.Name == "op_Equality"
                && candidate.GetParameters().Length == 2
                && candidate.GetParameters()[0].ParameterType == type
                && candidate.GetParameters()[1].ParameterType == type);

        if (method == null)
        {
            throw new EqualityOperatorException(type, $"The type {type.Name} does not implement an equality operator for {type.Name}.");
        }

        var self = Builder.CreateAnonymous(type);
        var other = Builder.CreateAnonymous(type);

        object result;
        try
        {
            result = method.Invoke(null, new[] { self, other });
        }
        catch (Exception exception)
        {
            throw new EqualityOperatorException(type, $"The equality operator of type {type.Name} threw an exception", exception);
        }

        if ((bool)result)
        {
            throw new EqualityOperatorException(type);
        }
    }
}