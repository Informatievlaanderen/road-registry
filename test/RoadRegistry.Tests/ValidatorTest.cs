namespace RoadRegistry.Tests;

using System.Linq.Expressions;
using AutoFixture;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.TestHelper;

public abstract class ValidatorTest<TModel, TValidator>
    where TModel : class
    where TValidator : AbstractValidator<TModel>, new()
{
    protected ValidatorTest()
    {
        Fixture = FixtureFactory.Create();
        Validator = new TValidator();
    }

    public Fixture Fixture { get; }
    public TModel Model { get; init; }
    public TValidator Validator { get; }

    private void AssignPropertyValue<TProperty>(Expression<Func<TModel, TProperty>> memberAccessor, object value)
    {
        var modelType = typeof(TModel);
        var propertyName = ValidatorOptions.Global.PropertyNameResolver(modelType, memberAccessor.GetMember(), memberAccessor);
        var property = modelType.GetProperty(propertyName);
        property.SetValue(Model, value);
    }

    public ITestValidationWith ShouldHaveValidationErrorFor<TProperty>(Expression<Func<TModel, TProperty>> memberAccessor, object value)
    {
        AssignPropertyValue(memberAccessor, value);
        return Validator.TestValidate(Model).ShouldHaveValidationErrorFor(memberAccessor);
    }

    public void ShouldNotHaveValidationErrorFor<TProperty>(Expression<Func<TModel, TProperty>> memberAccessor, object value)
    {
        AssignPropertyValue(memberAccessor, value);
        Validator.TestValidate(Model).ShouldNotHaveValidationErrorFor(memberAccessor);
    }

    [Fact]
    public virtual void VerifyValid()
    {
        Validator.ValidateAndThrow(Model);
    }
}
