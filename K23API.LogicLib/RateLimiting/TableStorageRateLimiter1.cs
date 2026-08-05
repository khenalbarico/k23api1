using System.Text;
using Azure;
using Azure.Data.Tables;
using K23API.LogicLib.TableStorageTools;
using Microsoft.Extensions.Logging;

namespace K23API.LogicLib.RateLimiting;

public class TableStorageRateLimiter1 : IApiRateLimiter
{
    private const string RateLimitTableName   = "RateLimits";
    private const int MaxContentionRetries    = 12;
    private const int ContentionRetryAfterSec = 1;

    private readonly TableClient _rateLimitTable;
    private readonly ILogger<TableStorageRateLimiter1> _logger;
    private readonly SemaphoreSlim _tableReadyGate = new(1, 1);

    private bool _tableReady;

    public TableStorageRateLimiter1(ITableStorageCfg tableStorageCfg, ILogger<TableStorageRateLimiter1> logger)
    {
        if (string.IsNullOrWhiteSpace(tableStorageCfg.TableStorageConnectionString))
            throw new InvalidOperationException(
                "App setting 'TableStorageConnectionString' is not set, so API rate limiting cannot be enforced.");

        _rateLimitTable = new TableClient(tableStorageCfg.TableStorageConnectionString, RateLimitTableName);
        _logger         = logger;
    }

    public async Task<RateLimitResult1> ConsumeAsync(
        string scopeKey, string endpointKey, ApiRateLimit1 rateLimit, CancellationToken cancellationToken)
    {
        var window = CurrentWindowFor(rateLimit);

        try
        {
            await EnsureTableExistsAsync(cancellationToken);

            return await ConsumeWindowAsync(
                TableKey($"scope-{scopeKey}"),
                TableKey($"{endpointKey}-{window.StartedAtUnix}"),
                window.EndsAt,
                rateLimit,
                cancellationToken);
        }
        catch (RequestFailedException exception)
        {
            _logger.LogError(exception,
                "Rate limit store unavailable, allowing {EndpointKey} for {ScopeKey} without counting it",
                endpointKey, scopeKey);

            return RateLimitResult1.Allowed;
        }
    }

    private async Task<RateLimitResult1> ConsumeWindowAsync(
        string partitionKey, string rowKey, DateTimeOffset windowEndsAt,
        ApiRateLimit1 rateLimit, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < MaxContentionRetries; attempt++)
        {
            var counter = await ReadCounterAsync(partitionKey, rowKey, cancellationToken);

            if (counter is null)
            {
                if (await TryCreateCounterAsync(partitionKey, rowKey, windowEndsAt, cancellationToken))
                    return RateLimitResult1.Allowed;

                continue;
            }

            if (counter.RequestCount >= rateLimit.MaxRequests)
                return RateLimitResult1.Blocked(SecondsUntil(counter.WindowEndsAt));

            counter.RequestCount++;

            if (await TryIncrementCounterAsync(counter, cancellationToken))
                return RateLimitResult1.Allowed;
        }

        _logger.LogWarning(
            "Rate limit counter {PartitionKey}/{RowKey} lost {Attempts} concurrency races, shedding the request",
            partitionKey, rowKey, MaxContentionRetries);

        return RateLimitResult1.Blocked(ContentionRetryAfterSec);
    }

    private async Task<RateLimitEntity1?> ReadCounterAsync(
        string partitionKey, string rowKey, CancellationToken cancellationToken)
    {
        try
        {
            return await _rateLimitTable.GetEntityAsync<RateLimitEntity1>(partitionKey, rowKey, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return null;
        }
    }

    private async Task<bool> TryCreateCounterAsync(
        string partitionKey, string rowKey, DateTimeOffset windowEndsAt, CancellationToken cancellationToken)
    {
        var counter = new RateLimitEntity1
        {
            PartitionKey = partitionKey,
            RowKey       = rowKey,
            RequestCount = 1,
            WindowEndsAt = windowEndsAt
        };

        try
        {
            await _rateLimitTable.AddEntityAsync(counter, cancellationToken);
            return true;
        }
        catch (RequestFailedException exception) when (exception.Status == 409)
        {
            return false;
        }
    }

    private async Task<bool> TryIncrementCounterAsync(RateLimitEntity1 counter, CancellationToken cancellationToken)
    {
        try
        {
            await _rateLimitTable.UpdateEntityAsync(counter, counter.ETag, TableUpdateMode.Replace, cancellationToken);
            return true;
        }
        catch (RequestFailedException exception) when (exception.Status == 412)
        {
            return false;
        }
    }

    private async Task EnsureTableExistsAsync(CancellationToken cancellationToken)
    {
        if (_tableReady) return;

        await _tableReadyGate.WaitAsync(cancellationToken);

        try
        {
            if (_tableReady) return;

            await _rateLimitTable.CreateIfNotExistsAsync(cancellationToken);
            _tableReady = true;
        }
        finally
        {
            _tableReadyGate.Release();
        }
    }

    private static (long StartedAtUnix, DateTimeOffset EndsAt) CurrentWindowFor(ApiRateLimit1 rateLimit)
    {
        var windowSeconds = Math.Max(1, (long)rateLimit.Window.TotalSeconds);
        var nowUnix       = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var startedAtUnix = nowUnix - nowUnix % windowSeconds;

        return (startedAtUnix, DateTimeOffset.FromUnixTimeSeconds(startedAtUnix + windowSeconds));
    }

    private static int SecondsUntil(DateTimeOffset windowEndsAt) =>
        (int)Math.Ceiling((windowEndsAt - DateTimeOffset.UtcNow).TotalSeconds);

    private static string TableKey(string rawKey)
    {
        var key = new StringBuilder(rawKey.Length);

        foreach (var character in rawKey)
            key.Append(char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' ? character : '-');

        return key.ToString();
    }
}
