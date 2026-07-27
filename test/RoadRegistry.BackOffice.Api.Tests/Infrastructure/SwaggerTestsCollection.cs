namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using Xunit;

// The Swagger test composes the real API DI (ConfigureDefaultForApi), which mutates FluentValidation's global static
// resolvers for the duration of the build. That would race with the (parallel) validator tests in this assembly, so the
// whole assembly is run without collection parallelization. It is snappy enough to keep as a plain unit test.
[CollectionDefinition("Swagger", DisableParallelization = true)]
public sealed class SwaggerTestsCollection;
