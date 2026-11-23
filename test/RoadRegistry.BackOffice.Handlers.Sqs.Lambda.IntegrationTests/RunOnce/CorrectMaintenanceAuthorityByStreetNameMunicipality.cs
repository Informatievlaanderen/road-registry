namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RunOnce;

using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RoadRegistry.Extracts.FeatureCompare.Inwinning.RoadSegment;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadSegment.ValueObjects;
using Xunit.Abstractions;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

/// <summary>
/// GAWR-7780. The inwinning now lets the street names lead the maintenance authority of a road segment on a
/// municipality border (see MaintenanceAuthorityByStreetNameMunicipality): when the left street name lies in
/// municipality A, the right one in municipality B, and the maintenance authority on each side is A or B, the left side
/// becomes A and the right side B. This applies the same correction to the road segments that were taken in before
/// that change, through the public edit endpoint POST v2/wegsegmenten/acties/wijzigen/attributen.
///
/// WHICH ROAD SEGMENTS. The migrated road segments (a migrated road segment is taken in, so its inwinning is complete)
/// that are neither removed nor historized, read from the RoadSegment aggregate documents in Marten: an inline
/// snapshot, so always up to date, unlike the extract documents of an async projection. The NIS code of each street name comes from the street name cache. The rule is
/// evaluated per stretch of the road segment on which both the street names and the maintenance authorities are
/// constant, so a road segment with attributes per position is corrected only where the rule applies; the request
/// carries the complete new maintenance authority of the road segment, as the endpoint requires.
///
/// ENVIRONMENT. Everything comes from appsettings.runonce.{environment}.json next to the test assembly (git ignores
/// appsettings.*.json): ConnectionStrings:Marten, ConnectionStrings:StreetNameProjections, BackOfficeApiBaseUrl,
/// CreateTokenEndpoint, ClientId and ClientSecret. The client needs the scope dv_wr_attribuutwaarden_beheer.
///
/// This WRITES when <c>dryRun</c> is false: every request becomes a change on the road network. Run it on STG before
/// PRD, after the FeatureCompare part of GAWR-7780 is deployed. It is idempotent: a second run finds nothing to correct.
/// </summary>
public class CorrectMaintenanceAuthorityByStreetNameMunicipality
{
    private const string AttribuutWaardenBeheerScope = "dv_wr_attribuutwaarden_beheer";
    private const string ChangeAttributesRoute = "v2/wegsegmenten/acties/wijzigen/attributen";
    private const int ReadBatchSize = 5000;

    // One request is one unit of work in the domain: a single road segment that fails, fails the whole request. Small
    // requests keep the damage of such a failure small and easy to retry.
    private const int RequestBatchSize = 50;
    private static readonly TimeSpan TicketTimeout = TimeSpan.FromMinutes(10);

    private readonly ITestOutputHelper _outputHelper;

    public CorrectMaintenanceAuthorityByStreetNameMunicipality(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact(Skip = "Run once, against an environment")]
    public async Task Run()
    {
        // tst, stg or prd: picks appsettings.runonce.{environment}.json.
        var environment = "stg";

        // Only lists what it would correct; set to false to call the API.
        var dryRun = true;

        var configuration = BuildConfiguration(environment);
        var sp = BuildServiceProvider(configuration);
        PrintTargets(environment, configuration, dryRun);

        var store = sp.GetRequiredService<IDocumentStore>();
        var streetNameCache = sp.GetRequiredService<IStreetNameCache>();

        var corrections = await FindCorrections(store, streetNameCache);
        _outputHelper.WriteLine($"Road segments to correct: {corrections.Count}");
        foreach (var correction in corrections)
        {
            _outputHelper.WriteLine($"  {correction.RoadSegmentId}: {Describe(correction.Current)} -> {Describe(correction.Corrected)}");
        }

        if (dryRun || corrections.Count == 0)
        {
            return;
        }

        using var httpClient = await CreateApiClient(configuration);

        var succeeded = 0;
        var failed = new List<string>();
        foreach (var batch in corrections.Chunk(RequestBatchSize))
        {
            var roadSegmentIds = string.Join(", ", batch.Select(x => x.RoadSegmentId));
            var (success, detail) = await SendAndAwaitTicket(httpClient, batch);
            if (success)
            {
                succeeded += batch.Length;
                _outputHelper.WriteLine($"OK: {roadSegmentIds}");
            }
            else
            {
                failed.Add($"{roadSegmentIds}: {detail}");
                _outputHelper.WriteLine($"FAILED: {roadSegmentIds}: {detail}");
            }
        }

        _outputHelper.WriteLine(string.Empty);
        _outputHelper.WriteLine($"Corrected: {succeeded}");
        _outputHelper.WriteLine($"Failed batches: {failed.Count}");
        foreach (var line in failed)
        {
            _outputHelper.WriteLine($"  {line}");
        }
    }

    [Fact]
    public void PlanCorrection_WholeSegmentWithBothSidesTheSameMunicipality_CorrectsLeftAndRight()
    {
        var nisCodes = new Dictionary<int, string> { [1] = "23000", [2] = "24038" };

        var plan = PlanCorrection(
            [Value(0, 100, RoadSegmentAttributeSide.Links, new StreetNameLocalId(1)), Value(0, 100, RoadSegmentAttributeSide.Rechts, new StreetNameLocalId(2))],
            [Value(0, 100, RoadSegmentAttributeSide.Beide, new OrganizationId("24038"))],
            nisCodes);

        plan.Should().BeEquivalentTo([new Stretch(0, 100, new OrganizationId("23000"), new OrganizationId("24038"))]);
    }

    [Fact]
    public void PlanCorrection_OnlyPartOfTheSegmentOnTheBorder_CorrectsOnlyThatPart()
    {
        var nisCodes = new Dictionary<int, string> { [1] = "23000", [2] = "24038", [3] = "24038" };

        var plan = PlanCorrection(
            [
                Value(0, 40, RoadSegmentAttributeSide.Links, new StreetNameLocalId(1)),
                Value(40, 100, RoadSegmentAttributeSide.Links, new StreetNameLocalId(3)),
                Value(0, 100, RoadSegmentAttributeSide.Rechts, new StreetNameLocalId(2))
            ],
            [Value(0, 100, RoadSegmentAttributeSide.Beide, new OrganizationId("24038"))],
            nisCodes);

        plan.Should().BeEquivalentTo([
            new Stretch(0, 40, new OrganizationId("23000"), new OrganizationId("24038")),
            new Stretch(40, 100, new OrganizationId("24038"), new OrganizationId("24038"))
        ]);
    }

    [Fact]
    public void PlanCorrection_OtherMaintenanceAuthority_NothingToCorrect()
    {
        var nisCodes = new Dictionary<int, string> { [1] = "23000", [2] = "24038" };

        var plan = PlanCorrection(
            [Value(0, 100, RoadSegmentAttributeSide.Links, new StreetNameLocalId(1)), Value(0, 100, RoadSegmentAttributeSide.Rechts, new StreetNameLocalId(2))],
            [Value(0, 100, RoadSegmentAttributeSide.Beide, new OrganizationId("AWV112"))],
            nisCodes);

        plan.Should().BeNull();
    }

    private async Task<IReadOnlyList<Correction>> FindCorrections(IDocumentStore store, IStreetNameCache streetNameCache)
    {
        var corrections = new List<Correction>();
        var lastId = 0;
        var read = 0;

        while (true)
        {
            // The generated columns of the aggregate document (migration 20261103120000): has_migrated is
            // RoadSegment.HasMigrated, status the bare V2 status name.
            await using var session = store.QuerySession();
            var roadSegments = (await session.QueryAsync<RoadSegment>(
                    "where road_segment_id > ? and has_migrated and not is_removed and status <> ? order by road_segment_id limit ?",
                    lastId, RoadSegmentStatusV2.Gehistoreerd.ToString(), ReadBatchSize))
                .ToList();
            if (roadSegments.Count == 0)
            {
                break;
            }

            lastId = roadSegments.Max(x => x.RoadSegmentId.ToInt32());
            read += roadSegments.Count;

            // Only a road segment with a street name on both sides can lie on a municipality border.
            var candidates = roadSegments
                .Where(x => x.HasMigrated() && !x.IsRemovedOrHistorized())
                .Where(x => x.Attributes!.StreetNameId.Values.Any(v => v.Value > 0 && v.Side != RoadSegmentAttributeSide.Rechts)
                            && x.Attributes!.StreetNameId.Values.Any(v => v.Value > 0 && v.Side != RoadSegmentAttributeSide.Links))
                .ToList();

            var streetNameIds = candidates
                .SelectMany(x => x.Attributes!.StreetNameId.Values)
                .Where(x => x.Value > 0)
                .Select(x => x.Value.ToInt32())
                .Distinct()
                .ToList();
            var nisCodes = (await streetNameCache.GetAsync(streetNameIds, CancellationToken.None))
                .Where(x => !x.IsRemoved && !string.IsNullOrWhiteSpace(x.NisCode))
                .ToDictionary(x => x.Id, x => x.NisCode);

            foreach (var roadSegment in candidates)
            {
                var attributes = roadSegment.Attributes!;
                var plan = PlanCorrection(attributes.StreetNameId.Values, attributes.MaintenanceAuthorityId.Values, nisCodes);
                if (plan is not null)
                {
                    corrections.Add(new Correction(roadSegment.RoadSegmentId, ToStretches(attributes.MaintenanceAuthorityId.Values), plan));
                }
            }

            _outputHelper.WriteLine($"Read {read} migrated road segments, {corrections.Count} to correct so far");
        }

        return corrections;
    }

    // The corrected maintenance authority for the whole road segment, or null when nothing changes. The road segment is
    // cut at every position where a street name or a maintenance authority changes; on each stretch the rule decides.
    private static IReadOnlyList<Stretch>? PlanCorrection(
        IReadOnlyCollection<RoadSegmentDynamicAttributeValue<StreetNameLocalId>> streetNames,
        IReadOnlyCollection<RoadSegmentDynamicAttributeValue<OrganizationId>> maintenanceAuthorities,
        IReadOnlyDictionary<int, string> nisCodes)
    {
        var positions = streetNames.SelectMany(x => new[] { x.Coverage.From, x.Coverage.To })
            .Concat(maintenanceAuthorities.SelectMany(x => new[] { x.Coverage.From, x.Coverage.To }))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var stretches = new List<Stretch>();
        var corrected = false;

        for (var i = 0; i < positions.Count - 1; i++)
        {
            var from = positions[i];
            var to = positions[i + 1];

            var leftMaintenanceAuthority = ValueOn(maintenanceAuthorities, from, to, RoadSegmentAttributeSide.Links);
            var rightMaintenanceAuthority = ValueOn(maintenanceAuthorities, from, to, RoadSegmentAttributeSide.Rechts);
            if (leftMaintenanceAuthority is null || rightMaintenanceAuthority is null)
            {
                // A gap in the maintenance authority: not something to send a complete new value for.
                return null;
            }

            var leftStreetName = ValueOn(streetNames, from, to, RoadSegmentAttributeSide.Links);
            var rightStreetName = ValueOn(streetNames, from, to, RoadSegmentAttributeSide.Rechts);

            var left = leftMaintenanceAuthority.Value;
            var right = rightMaintenanceAuthority.Value;
            if (MaintenanceAuthorityByStreetNameMunicipality.TryCorrect(
                    NisCodeOf(leftStreetName),
                    NisCodeOf(rightStreetName),
                    left,
                    right,
                    out var correctedLeft,
                    out var correctedRight))
            {
                corrected = true;
                left = correctedLeft;
                right = correctedRight;
            }

            stretches.Add(new Stretch(from.ToDouble(), to.ToDouble(), left, right));
        }

        return corrected ? Merge(stretches) : null;

        string? NisCodeOf(StreetNameLocalId? streetNameId)
        {
            return streetNameId > 0 && nisCodes.TryGetValue(streetNameId.Value.ToInt32(), out var nisCode) ? nisCode : null;
        }
    }

    private static T? ValueOn<T>(
        IEnumerable<RoadSegmentDynamicAttributeValue<T>> values,
        RoadSegmentPositionV2 from,
        RoadSegmentPositionV2 to,
        RoadSegmentAttributeSide side)
        where T : struct
    {
        var value = values.FirstOrDefault(x =>
            x.Coverage.From <= from && to <= x.Coverage.To
            && (x.Side == side || x.Side == RoadSegmentAttributeSide.Beide));
        return value is null ? null : value.Value;
    }

    private static IReadOnlyList<Stretch> ToStretches(IReadOnlyCollection<RoadSegmentDynamicAttributeValue<OrganizationId>> maintenanceAuthorities)
    {
        var positions = maintenanceAuthorities.SelectMany(x => new[] { x.Coverage.From, x.Coverage.To }).Distinct().OrderBy(x => x).ToList();
        var stretches = new List<Stretch>();
        for (var i = 0; i < positions.Count - 1; i++)
        {
            var left = ValueOn(maintenanceAuthorities, positions[i], positions[i + 1], RoadSegmentAttributeSide.Links);
            var right = ValueOn(maintenanceAuthorities, positions[i], positions[i + 1], RoadSegmentAttributeSide.Rechts);
            stretches.Add(new Stretch(positions[i].ToDouble(), positions[i + 1].ToDouble(), left ?? default, right ?? default));
        }

        return Merge(stretches);
    }

    private static IReadOnlyList<Stretch> Merge(IReadOnlyList<Stretch> stretches)
    {
        var merged = new List<Stretch>();
        foreach (var stretch in stretches)
        {
            if (merged.Count > 0 && merged[^1].Left == stretch.Left && merged[^1].Right == stretch.Right && merged[^1].To == stretch.From)
            {
                merged[^1] = merged[^1] with { To = stretch.To };
            }
            else
            {
                merged.Add(stretch);
            }
        }

        return merged;
    }

    private async Task<(bool Success, string Detail)> SendAndAwaitTicket(HttpClient httpClient, IReadOnlyCollection<Correction> batch)
    {
        var body = batch.Select(correction => new Dictionary<string, object>
        {
            ["wegsegmenten"] = new[] { correction.RoadSegmentId.ToInt32() },
            ["wegbeheerder"] = correction.Corrected
                .SelectMany(stretch => stretch.Left == stretch.Right
                    ? new[] { WegbeheerderParameter(RoadSegmentAttributeSide.Beide, stretch, stretch.Left) }
                    : new[]
                    {
                        WegbeheerderParameter(RoadSegmentAttributeSide.Links, stretch, stretch.Left),
                        WegbeheerderParameter(RoadSegmentAttributeSide.Rechts, stretch, stretch.Right)
                    })
                .ToArray()
        }).ToArray();

        using var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var response = await httpClient.PostAsync(ChangeAttributesRoute, content);
        if (response.StatusCode != HttpStatusCode.Accepted)
        {
            return (false, $"{(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        }

        var ticketUrl = response.Headers.Location;
        if (ticketUrl is null)
        {
            return (false, "202 without a ticket location");
        }

        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < TicketTimeout)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));

            using var ticketResponse = await httpClient.GetAsync(ticketUrl);
            var ticketJson = await ticketResponse.Content.ReadAsStringAsync();
            if (!ticketResponse.IsSuccessStatusCode)
            {
                continue;
            }

            using var ticket = JsonDocument.Parse(ticketJson);
            var status = ticket.RootElement.TryGetProperty("status", out var statusElement) ? statusElement.ToString() : string.Empty;
            if (string.Equals(status, "complete", StringComparison.OrdinalIgnoreCase))
            {
                return (true, ticketUrl.ToString());
            }

            if (string.Equals(status, "error", StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"{ticketUrl} {ticketJson}");
            }
        }

        return (false, $"{ticketUrl} still not done after {TicketTimeout}");
    }

    private static Dictionary<string, object> WegbeheerderParameter(RoadSegmentAttributeSide side, Stretch stretch, OrganizationId maintenanceAuthority)
    {
        return new Dictionary<string, object>
        {
            ["kant"] = side.Translation.Name,
            ["vanPositie"] = stretch.From,
            ["totPositie"] = stretch.To,
            ["wegbeheerder"] = maintenanceAuthority.ToString()
        };
    }

    private static async Task<HttpClient> CreateApiClient(IConfiguration configuration)
    {
        using var tokenClient = new HttpClient();
        using var tokenResponse = await tokenClient.PostAsync(Required(configuration, "CreateTokenEndpoint"), new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = Required(configuration, "ClientId"),
            ["client_secret"] = Required(configuration, "ClientSecret"),
            ["scope"] = AttribuutWaardenBeheerScope
        }));
        var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
        if (!tokenResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Requesting a token failed: {(int)tokenResponse.StatusCode} {tokenJson}");
        }

        using var token = JsonDocument.Parse(tokenJson);
        var accessToken = token.RootElement.GetProperty("access_token").GetString();

        var baseUrl = Required(configuration, "BackOfficeApiBaseUrl").TrimEnd('/') + "/";
        var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return httpClient;
    }

    private static string Required(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? throw new InvalidOperationException($"'{key}' is missing in the appsettings.runonce file.") : value;
    }

    private static string Describe(IEnumerable<Stretch> stretches)
    {
        return string.Join(" | ", stretches.Select(x => $"[{x.From:0.###}-{x.To:0.###}] L {x.Left} R {x.Right}"));
    }

    private static RoadSegmentDynamicAttributeValue<T> Value<T>(double from, double to, RoadSegmentAttributeSide side, T value)
    {
        return new RoadSegmentDynamicAttributeValue<T>
        {
            Coverage = new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(from), new RoadSegmentPositionV2(to)),
            Side = side,
            Value = value
        };
    }

    private void PrintTargets(string environment, IConfiguration configuration, bool dryRun)
    {
        _outputHelper.WriteLine($"Environment: {environment}{(dryRun ? " (dry run)" : string.Empty)}");
        var marten = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString(WellKnownConnectionNames.Marten));
        _outputHelper.WriteLine($"  Marten: {marten.Host}:{marten.Port}/{marten.Database}");
        _outputHelper.WriteLine($"  API: {configuration["BackOfficeApiBaseUrl"]}");
        _outputHelper.WriteLine(string.Empty);
    }

    private static IConfiguration BuildConfiguration(string environment)
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile($"appsettings.runonce.{environment}.json", optional: false, reloadOnChange: false)
            .Build();
    }

    private static IServiceProvider BuildServiceProvider(IConfiguration configuration)
    {
        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddLogging();

        services
            .AddMartenRoad(options => options.AddRoadAggregatesSnapshots()).Services
            .AddStreetNameCache();

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }

    private sealed record Correction(RoadSegmentId RoadSegmentId, IReadOnlyList<Stretch> Current, IReadOnlyList<Stretch> Corrected);

    private sealed record Stretch(double From, double To, OrganizationId Left, OrganizationId Right);
}
