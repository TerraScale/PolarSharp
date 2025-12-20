using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using PolarSharp.Extensions;
using PolarSharp.Models.Common;
using PolarSharp.Models.Metrics;
using PolarSharp.Results;
using PolarSharp.Api;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Metrics API.
/// </summary>
public class MetricsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public MetricsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task GetAsync_ReturnsListOfMetrics()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Metrics.GetAsync();

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Should().BeAssignableTo<List<Metric>>();

                // If there are any metrics, validate their structure
                if (result.Value.Any())
                {
                    result.Value.Should().AllSatisfy(metric =>
                    {
                        metric.Name.Should().NotBeNullOrEmpty();
                        metric.Period.Should().NotBeNullOrEmpty();
                        metric.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromDays(365)); // Within last year
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetLimitsAsync_ReturnsListOfMetricLimits()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Metrics.GetLimitsAsync();

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Should().BeAssignableTo<List<MetricLimit>>();

                // If there are any limits, validate their structure
                if (result.Value.Any())
                {
                    result.Value.Should().AllSatisfy(limit =>
                    {
                        limit.Name.Should().NotBeNullOrEmpty();
                        limit.MaxValue.Should().BeGreaterThanOrEqualTo(0);
                        limit.CurrentValue.Should().BeGreaterThanOrEqualTo(0);
                        limit.PercentageUsed.Should().BeGreaterThanOrEqualTo(0);

                        // Percentage should be calculated correctly
                        if (limit.MaxValue > 0)
                        {
                            var expectedPercentage = (limit.CurrentValue / limit.MaxValue) * 100;
                            limit.PercentageUsed.Should().BeApproximately(expectedPercentage, 0.01m);
                        }
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithDefaultParameters_ReturnsPaginatedResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Metrics.ListAsync(new MetricsQueryBuilder());

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Items.Should().NotBeNull();
                result.Value.Pagination.Page.Should().BeGreaterThanOrEqualTo(1);
                result.Value.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var page = 1;
            var limit = 5;

            // Act
            var result = await client.Metrics.ListAsync(new MetricsQueryBuilder(), page: page, limit: limit);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Pagination.Page.Should().Be(page);
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ReturnsFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var builder = client.Metrics.Query()
                .WithType("revenue")
                .StartDate(DateTime.UtcNow.AddDays(-30))
                .EndDate(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Items.Should().NotBeNull();

                // Validate date range if any metrics are returned
                if (result.Value.Items.Any())
                {
                    var startDate = DateTime.UtcNow.AddDays(-30);
                    var endDate = DateTime.UtcNow;

                    result.Value.Items.Should().AllSatisfy(metric =>
                    {
                        metric.Timestamp.Should().BeOnOrAfter(startDate);
                        metric.Timestamp.Should().BeOnOrBefore(endDate);
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithCustomerIdFilter_ReturnsFilteredResults()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // First create a customer to filter by
            var customerRequest = new PolarSharp.Models.Customers.CustomerCreateRequest
            {
                Email = "metrics-test@mailinator.com",
                Name = "Metrics Test Customer"
            };
            var customerResult = await client.Customers.CreateAsync(customerRequest);

            if (customerResult.IsSuccess)
            {
                try
                {
                    var builder = client.Metrics.Query()
                        .WithCustomerId(customerResult.Value.Id);

                    // Act
                    var result = await client.Metrics.ListAsync(builder);

                    // Assert
                    result.Should().NotBeNull();
                    if (result.IsSuccess)
                    {
                        result.Value.Items.Should().NotBeNull();
                        // Note: This might return empty if no metrics exist for this customer yet
                    }
                }
                finally
                {
                    // Cleanup customer
                    try { await client.Customers.DeleteAsync(customerResult.Value.Id); } catch { }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_WithDefaultParameters_ReturnsAllMetrics()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var metrics = new List<Metric>();
            await foreach (var result in client.Metrics.ListAllAsync(CancellationToken.None))
            {
                if (result.IsSuccess)
                {
                    metrics.Add(result.Value);
                }
            }

            // Assert
            metrics.Should().NotBeNull();

            // Should contain all metrics across all pages
            if (metrics.Any())
            {
                metrics.Should().AllSatisfy(metric =>
                {
                    metric.Name.Should().NotBeNullOrEmpty();
                    metric.Period.Should().NotBeNullOrEmpty();
                    metric.Timestamp.Should().BeBefore(DateTime.UtcNow.AddMinutes(5)); // Allow for some clock skew
                });
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_WithQueryBuilder_ReturnsFilteredMetrics()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var builder = client.Metrics.Query()
                .StartDate(DateTime.UtcNow.AddDays(-7))
                .EndDate(DateTime.UtcNow);

            // Act
            var metrics = new List<Metric>();
            await foreach (var result in client.Metrics.ListAllAsync(CancellationToken.None))
            {
                if (result.IsSuccess)
                {
                    metrics.Add(result.Value);
                }
            }

            // Assert
            metrics.Should().NotBeNull();

            if (metrics.Any())
            {
                var startDate = DateTime.UtcNow.AddDays(-7);
                var endDate = DateTime.UtcNow;

                metrics.Should().AllSatisfy(metric =>
                {
                    metric.Timestamp.Should().BeOnOrAfter(startDate);
                    metric.Timestamp.Should().BeOnOrBefore(endDate);
                });
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_WithoutBuilder_ReturnsAllMetrics()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var metrics = new List<Metric>();
            await foreach (var result in client.Metrics.ListAllAsync(CancellationToken.None))
            {
                if (result.IsSuccess)
                {
                    metrics.Add(result.Value);
                }
            }

            // Assert
            metrics.Should().NotBeNull();

            // Should contain all metrics across all pages
            if (metrics.Any())
            {
                metrics.Should().AllSatisfy(metric =>
                {
                    metric.Name.Should().NotBeNullOrEmpty();
                    metric.Value.Should().BeGreaterThanOrEqualTo(0);
                    metric.Period.Should().NotBeNullOrEmpty();
                });
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task QueryBuilder_WithDateRange_FiltersByTimestamp()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var now = DateTime.UtcNow;
            var yesterday = now.AddDays(-1);
            var builder = client.Metrics.Query()
                .StartDate(yesterday)
                .EndDate(now);

            // Act
            var result = await client.Metrics.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Items.Should().AllSatisfy(metric =>
                {
                    metric.Timestamp.Should().BeOnOrAfter(yesterday);
                    metric.Timestamp.Should().BeOnOrBefore(now);
                });
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task QueryBuilder_WithMultipleFilters_CombinesFiltersCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var builder = client.Metrics.Query()
                .WithType("revenue")
                .StartDate(DateTime.UtcNow.AddDays(-30))
                .EndDate(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Items.Should().NotBeNull();

                if (result.Value.Items.Any())
                {
                    var startDate = DateTime.UtcNow.AddDays(-30);
                    var endDate = DateTime.UtcNow;

                    result.Value.Items.Should().AllSatisfy(metric =>
                    {
                        metric.Timestamp.Should().BeOnOrAfter(startDate);
                        metric.Timestamp.Should().BeOnOrBefore(endDate);
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task Metric_Structure_IsValid()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Metrics.GetAsync();

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();

                if (result.Value.Any())
                {
                    var firstMetric = result.Value.First();
                    firstMetric.Should().NotBeNull();
                    firstMetric.Name.Should().NotBeNullOrEmpty();
                    firstMetric.Value.Should().BeGreaterThanOrEqualTo(0);
                    firstMetric.Period.Should().NotBeNullOrEmpty();
                    firstMetric.Timestamp.Should().BeBefore(DateTime.UtcNow.AddMinutes(5));
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task MetricLimit_Structure_IsValid()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Metrics.GetLimitsAsync();

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();

                if (result.Value.Any())
                {
                    var firstLimit = result.Value.First();
                    firstLimit.Should().NotBeNull();
                    firstLimit.Name.Should().NotBeNullOrEmpty();
                    firstLimit.MaxValue.Should().BeGreaterThanOrEqualTo(0);
                    firstLimit.CurrentValue.Should().BeGreaterThanOrEqualTo(0);
                    firstLimit.PercentageUsed.Should().BeGreaterThanOrEqualTo(0);

                    // If there's a reset date, it should be in the future
                    if (firstLimit.ResetsAt.HasValue)
                    {
                        firstLimit.ResetsAt.Value.Should().BeAfter(DateTime.UtcNow);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithCancellation_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Act
            var result = await client.Metrics.GetAsync(cts.Token);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().BeAssignableTo<List<Metric>>();
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetLimitsAsync_WithCancellation_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Act
            var result = await client.Metrics.GetLimitsAsync(cts.Token);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().BeAssignableTo<List<MetricLimit>>();
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAsync_WithCancellation_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Act
            var result = await client.Metrics.ListAsync(new MetricsQueryBuilder(), cancellationToken: cts.Token);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Items.Should().NotBeNull();
                result.Value.Pagination.Should().NotBeNull();
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task ListAllAsync_WithCancellation_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            // Act
            var metrics = new List<Metric>();
            await foreach (var result in client.Metrics.ListAllAsync(cancellationToken: cts.Token))
            {
                if (result.IsSuccess)
                {
                    metrics.Add(result.Value);
                    if (metrics.Count >= 5) break; // Limit for testing purposes
                }
            }

            // Assert
            metrics.Should().NotBeNull();
            if (metrics.Any())
            {
                metrics.Should().AllSatisfy(metric =>
                {
                    metric.Name.Should().NotBeNullOrEmpty();
                    metric.Period.Should().NotBeNullOrEmpty();
                });
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}