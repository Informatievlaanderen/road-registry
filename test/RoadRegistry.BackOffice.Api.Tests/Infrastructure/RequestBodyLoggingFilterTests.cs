namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using RoadRegistry.BackOffice.Api.Infrastructure.Configuration;

// The point of this filter is what it does *not* do: reading a request body that nobody is going to log costs a
// blocked thread for as long as the client takes to send it, and enough of those at once is what makes Kestrel give
// up on a slow upload.
public class RequestBodyLoggingFilterTests
{
    private static (RequestBodyLoggingFilter Filter, Mock<ILogger<RequestBodyLoggingFilter>> Logger) CreateFilter(bool debugEnabled)
    {
        var logger = new Mock<ILogger<RequestBodyLoggingFilter>>();
        logger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(debugEnabled);

        return (new RequestBodyLoggingFilter(logger.Object), logger);
    }

    private static (ActionExecutingContext Context, CountingStream Body) CreateContext(string method, string body)
    {
        var stream = new CountingStream(Encoding.UTF8.GetBytes(body));

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Body = stream;

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        return (new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), controller: null!), stream);
    }

    private static ActionExecutionDelegate NextIs(Action onCalled)
    {
        return () =>
        {
            onCalled();
            return Task.FromResult<ActionExecutedContext>(null!);
        };
    }

    [Fact]
    public async Task WhenDebugLoggingIsOff_ThenTheBodyIsNotRead()
    {
        var (filter, _) = CreateFilter(debugEnabled: false);
        var (context, body) = CreateContext(HttpMethods.Post, "{\"wegknopen\":[1]}");
        var nextWasCalled = false;

        await filter.OnActionExecutionAsync(context, NextIs(() => nextWasCalled = true));

        body.ReadCount.Should().Be(0);
        nextWasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task WhenDebugLoggingIsOn_ThenTheBodyIsReadAndLeftForTheModelBinder()
    {
        var (filter, logger) = CreateFilter(debugEnabled: true);
        var (context, body) = CreateContext(HttpMethods.Post, "{\"wegknopen\":[1]}");

        await filter.OnActionExecutionAsync(context, NextIs(() => { }));

        body.ReadCount.Should().BeGreaterThan(0);
        body.Position.Should().Be(0);
        logger.Verify(x => x.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    // The framework filter logged POST and PUT only, and this keeps to that.
    [Fact]
    public async Task WhenTheMethodCarriesNoBodyToLog_ThenTheBodyIsNotRead()
    {
        var (filter, _) = CreateFilter(debugEnabled: true);
        var (context, body) = CreateContext(HttpMethods.Get, string.Empty);

        await filter.OnActionExecutionAsync(context, NextIs(() => { }));

        body.ReadCount.Should().Be(0);
    }

    // Without buffering the body can only be read once, and reading it here would leave nothing for the model binder.
    [Fact]
    public async Task WhenTheBodyCannotBeRewound_ThenItIsNotRead()
    {
        var (filter, _) = CreateFilter(debugEnabled: true);
        var (context, body) = CreateContext(HttpMethods.Post, "{}");
        body.CanSeekOverride = false;

        await filter.OnActionExecutionAsync(context, NextIs(() => { }));

        body.ReadCount.Should().Be(0);
    }

    private sealed class CountingStream : MemoryStream
    {
        public CountingStream(byte[] buffer)
            : base(buffer, writable: false)
        {
        }

        public int ReadCount { get; private set; }
        public bool CanSeekOverride { get; set; } = true;

        public override bool CanSeek => CanSeekOverride;

        public override int Read(byte[] buffer, int offset, int count)
        {
            ReadCount++;
            return base.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            ReadCount++;
            return base.Read(buffer);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadCount++;
            return base.ReadAsync(buffer, cancellationToken);
        }
    }
}
