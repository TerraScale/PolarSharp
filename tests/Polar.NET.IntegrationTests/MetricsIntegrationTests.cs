using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Polar.NET.Extensions;
using Polar.NET.Models.Common;
using Polar.NET.Models.Metrics;
using Polar.NET.Exceptions;
using Polar.NET.Api;
using Xunit;

namespace Polar.NET.IntegrationTests;

/// <summary>
/// Integration tests for Metrics API.
/// </summary>
public class MetricsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public MetricsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAsync_ReturnsListOfMetrics()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Metrics.GetAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<List<Metric>>();
        
        // If there are any metrics, validate their structure
        if (result.Any())
        {
            result.Should().AllSatisfy(metric =>
            {
                metric.Name.Should().NotBeNullOrEmpty();
                metric.Period.Should().NotBeNullOrEmpty();
                metric.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromDays(365)); // Within last year
            });
        }
    }

    [Fact]
    public async Task GetLimitsAsync_ReturnsListOfMetricLimits()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Metrics.GetLimitsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<List<MetricLimit>>();
        
        // If there are any limits, validate their structure
        if (result.Any())
        {
            result.Should().AllSatisfy(limit =>
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

    [Fact]
    public async Task ListAsync_WithDefaultParameters_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Metrics.ListAsync(new MetricsQueryBuilder());

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Page.Should().BeGreaterThanOrEqualTo(1);
        result.Pagination.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Pagination.MaxPage.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ListAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var page = 1;
        var limit = 5;

        // Act
        var result = await client.Metrics.ListAsync(new MetricsQueryBuilder(), page: page, limit: limit);

        // Assert
        result.Should().NotBeNull();
        result.Pagination.Page.Should().Be(page);
    }

    [Fact]
    public async Task ListAsync_WithQueryBuilder_ReturnsFilteredResults()
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
        result.Items.Should().NotBeNull();
        
        // Validate date range if any metrics are returned
        if (result.Items.Any())
        {
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;
            
            result.Items.Should().AllSatisfy(metric =>
            {
                metric.Timestamp.Should().BeOnOrAfter(startDate);
                metric.Timestamp.Should().BeOnOrBefore(endDate);
            });
        }
    }

    [Fact]
    public async Task ListAsync_WithCustomerIdFilter_ReturnsFilteredResults()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First create a customer to filter by
        var customerRequest = new Polar.NET.Models.Customers.CustomerCreateRequest
        {
            Email = "metrics-test@example.com",
            Name = "Metrics Test Customer"
        };
        var customer = await client.Customers.CreateAsync(customerRequest);

        try
        {
            var builder = client.Metrics.Query()
                .WithCustomerId(customer.Id);

            // Act
            var result = await client.Metrics.ListAsync(builder);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull();
            // Note: This might return empty if no metrics exist for this customer yet
        }
        finally
        {
            // Cleanup customer
            try { await client.Customers.DeleteAsync(customer.Id); } catch { }
        }
    }

    [Fact]
    public async Task ListAllAsync_WithDefaultParameters_ReturnsAllMetrics()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var metrics = new List<Metric>();
        var method = typeof(MetricsApi).GetMethod("ListAllAsync", new[] { typeof(CancellationToken) });
        var listAllFunc = (Func<CancellationToken, IAsyncEnumerable<Metric>>)Delegate.CreateDelegate(typeof(Func<CancellationToken, IAsyncEnumerable<Metric>>), client.Metrics, method!);
        await foreach (var metric in listAllFunc(CancellationToken.None))
        {
            metrics.Add(metric);
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

    [Fact]
    public async Task ListAllAsync_WithQueryBuilder_ReturnsFilteredMetrics()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var builder = client.Metrics.Query()
            .StartDate(DateTime.UtcNow.AddDays(-7))
            .EndDate(DateTime.UtcNow);

        // Act
        var metrics = new List<Metric>();
        var listAllMethod = (Func<CancellationToken, IAsyncEnumerable<Metric>>)(ct => client.Metrics.ListAllAsync(ct));
        await foreach (var metric in listAllMethod(CancellationToken.None))
        {
            metrics.Add(metric);
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

    [Fact]
    public async Task ListAllAsync_WithoutBuilder_ReturnsAllMetrics()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var metrics = new List<Metric>();
        var method = typeof(MetricsApi).GetMethod("ListAllAsync", new[] { typeof(CancellationToken) });
        var listAllFunc = (Func<CancellationToken, IAsyncEnumerable<Metric>>)Delegate.CreateDelegate(typeof(Func<CancellationToken, IAsyncEnumerable<Metric>>), client.Metrics, method!);
        await foreach (var metric in listAllFunc(CancellationToken.None))
        {
            metrics.Add(metric);
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

    [Fact]
    public async Task QueryBuilder_WithDateRange_FiltersByTimestamp()
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
        result.Items.Should().AllSatisfy(metric =>
        {
            metric.Timestamp.Should().BeOnOrAfter(yesterday);
            metric.Timestamp.Should().BeOnOrBefore(now);
        });
    }

    [Fact]
    public async Task QueryBuilder_WithMultipleFilters_CombinesFiltersCorrectly()
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
        result.Items.Should().NotBeNull();
        
        if (result.Items.Any())
        {
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;
            
            result.Items.Should().AllSatisfy(metric =>
            {
                metric.Timestamp.Should().BeOnOrAfter(startDate);
                metric.Timestamp.Should().BeOnOrBefore(endDate);
            });
        }
    }

    [Fact]
    public async Task Metric_Structure_IsValid()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var metrics = await client.Metrics.GetAsync();

        // Assert
        metrics.Should().NotBeNull();
        
        if (metrics.Any())
        {
            var firstMetric = metrics.First();
            firstMetric.Should().NotBeNull();
            firstMetric.Name.Should().NotBeNullOrEmpty();
            firstMetric.Value.Should().BeGreaterThanOrEqualTo(0);
            firstMetric.Period.Should().NotBeNullOrEmpty();
            firstMetric.Timestamp.Should().BeBefore(DateTime.UtcNow.AddMinutes(5));
        }
    }

    [Fact]
    public async Task MetricLimit_Structure_IsValid()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var limits = await client.Metrics.GetLimitsAsync();

        // Assert
        limits.Should().NotBeNull();
        
        if (limits.Any())
        {
            var firstLimit = limits.First();
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

    [Fact]
    public async Task GetAsync_WithCancellation_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var result = await client.Metrics.GetAsync(cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<List<Metric>>();
    }

    [Fact]
    public async Task GetLimitsAsync_WithCancellation_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var result = await client.Metrics.GetLimitsAsync(cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<List<MetricLimit>>();
    }

    [Fact]
    public async Task ListAsync_WithCancellation_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var result = await client.Metrics.ListAsync(new MetricsQueryBuilder(), cancellationToken: cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAllAsync_WithCancellation_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        // Act
        var metrics = new List<Metric>();
        await foreach (var metric in client.Metrics.ListAllAsync(cancellationToken: cts.Token))
        {
            metrics.Add(metric);
            if (metrics.Count >= 5) break; // Limit for testing purposes
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
}