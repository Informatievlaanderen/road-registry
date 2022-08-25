namespace RoadRegistry.Framework.Assertions;

using System.Reflection;
using AutoFixture.Idioms;
using AutoFixture.Kernel;

public class ConstructorBasedInequalityOperatorEqualsOtherAssertion : IdiomaticAssertion
{
    public ConstructorBasedInequalityOperatorEqualsOtherAssertion(ISpecimenBuilder builder, ConstructorInfo constructor)
    {
        Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        Constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
    }

    public ISpecimenBuilder Builder { get; }
    public ConstructorInfo Constructor { get; }

    public override void Verify(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));

        var method = type
            .GetMethods()
            .SingleOrDefault(candidate =>
                candidate.Name == "op_Inequality"
                && candidate.GetParameters().Length == 2
                && candidate.GetParameters()[0].ParameterType == type
                && candidate.GetParameters()[1].ParameterType == type);

        if (method == null)
            throw new InequalityOperatorException(type, $"The type {type.Name} does not implement an inequality operator for {type.Name}.");

        var selfParameters = new object[Constructor.GetParameters().Length];
        foreach (var parameter in Constructor.GetParameters()) selfParameters[parameter.Position] = Builder.CreateAnonymous(parameter.ParameterType);

        var otherParameters = new object[Constructor.GetParameters().Length];
        selfParameters.CopyTo(otherParameters, 0);
        var position = (int)Builder.CreateAnonymous(typeof(int)) % otherParameters.Length;
        while (otherParameters[position].Equals(selfParameters[position]))
            otherParameters[position] =
                Builder.CreateAnonymous(Constructor.GetParameters()[position].ParameterType);

        var self = Constructor.Invoke(selfParameters);
        var other = Constructor.Invoke(otherParameters);

        object result;
        try
        {
            result = method.Invoke(null, new[] { self, other });
        }
        catch (Exception exception)
        {
            throw new InequalityOperatorException(type, $"The inequality operator of type {type.Name} threw an exception", exception);
        }

        if (!(bool)result)
            throw new InequalityOperatorException(type);
    }
}
