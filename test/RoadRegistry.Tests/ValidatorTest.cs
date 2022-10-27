namespace RoadRegistry.Tests;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.TestHelper;
using RoadRegistry.Tests.BackOffice;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

public abstract class ValidatorTest<TModel, TValidator>
    where TModel : class
    where TValidator : AbstractValidator<TModel>, new()
{
    protected ValidatorTest()
    {
        Fixture = new Fixture();
        Validator = new TValidator();
    }

    public Fixture Fixture { get; }

    public TModel Model { get; init; }

    public TValidator Validator { get; }

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

    private void AssignPropertyValue<TProperty>(Expression<Func<TModel, TProperty>> memberAccessor, object value)
    {
        var modelType = typeof(TModel);
        var propertyName = ValidatorOptions.Global.PropertyNameResolver(modelType, memberAccessor.GetMember(), memberAccessor);
        var property = modelType.GetProperty(propertyName);
        property.SetValue(Model, value);
    }

    [Fact]
    public virtual void VerifyValid()
    {
        Validator.ValidateAndThrow(Model);
    }
}
