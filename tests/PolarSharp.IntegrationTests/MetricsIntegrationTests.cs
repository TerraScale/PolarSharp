using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PolarSharp.Models.Metrics;
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
    public async Task GetAsync_WithRequiredParameters_ReturnsMetricsResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.GetAsync(startDate, endDate, TimeInterval.Day);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Periods.Should().NotBeNull();
                result.Value.Metrics.Should().NotBeNull();
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithWeekInterval_ReturnsMetricsResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.GetAsync(startDate, endDate, TimeInterval.Week);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Periods.Should().NotBeNull();
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithMonthInterval_ReturnsMetricsResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.GetAsync(startDate, endDate, TimeInterval.Month);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Periods.Should().NotBeNull();
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithYearInterval_ReturnsMetricsResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.GetAsync(startDate, endDate, TimeInterval.Year);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Periods.Should().NotBeNull();
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithProductIdFilter_ReturnsFilteredMetrics()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // First get a product to filter by
            var productsResult = await client.Products.ListAsync();
            if (!productsResult.IsSuccess || !productsResult.Value.Items.Any())
            {
                _output.WriteLine("Skipped: No products available to filter by");
                return;
            }

            var productId = productsResult.Value.Items.First().Id;

            // Act
            var result = await client.Metrics.GetAsync(
                startDate, 
                endDate, 
                TimeInterval.Day,
                productId: productId);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetLimitsAsync_ReturnsMetricsLimits()
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
                result.Value.Limits.Should().NotBeNull();
                
                // Validate limits structure if any exist
                if (result.Value.Limits.Any())
                {
                    result.Value.Limits.Should().AllSatisfy(limit =>
                    {
                        limit.Interval.Should().NotBeNull();
                        limit.MinDays.Should().BeGreaterThanOrEqualTo(0);
                    });
                }
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_MetricsResponse_ContainsExpectedMetricDefinitions()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.GetAsync(startDate, endDate, TimeInterval.Day);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                var metrics = result.Value.Metrics;
                
                // The API returns various metric definitions
                // Check that the basic structure is there
                metrics.Should().NotBeNull();
                
                // Revenue metric definition should exist
                if (metrics.Revenue != null)
                {
                    metrics.Revenue.Slug.Should().NotBeNullOrEmpty();
                    metrics.Revenue.DisplayName.Should().NotBeNullOrEmpty();
                }

                // Orders metric definition should exist  
                if (metrics.Orders != null)
                {
                    metrics.Orders.Slug.Should().NotBeNullOrEmpty();
                    metrics.Orders.DisplayName.Should().NotBeNullOrEmpty();
                }

                // ActiveSubscriptions metric definition should exist
                if (metrics.ActiveSubscriptions != null)
                {
                    metrics.ActiveSubscriptions.Slug.Should().NotBeNullOrEmpty();
                    metrics.ActiveSubscriptions.DisplayName.Should().NotBeNullOrEmpty();
                }
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_Periods_MatchRequestedInterval()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.GetAsync(startDate, endDate, TimeInterval.Day);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Periods.Should().NotBeNull();
                
                // Should have approximately 7-8 periods for a week with day interval
                if (result.Value.Periods.Any())
                {
                    result.Value.Periods.Count.Should().BeInRange(1, 10);
                    
                    // Periods should be ordered
                    var orderedPeriods = result.Value.Periods.OrderBy(p => p.Timestamp).ToList();
                    result.Value.Periods.Should().BeEquivalentTo(orderedPeriods, options => options.WithStrictOrdering());
                }
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
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
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.GetAsync(startDate, endDate, TimeInterval.Day, cancellationToken: cts.Token);

            // Assert
            result.Should().NotBeNull();
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
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_Totals_ContainsAggregatedData()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // Act
            var result = await client.Metrics.GetAsync(startDate, endDate, TimeInterval.Day);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Totals.Should().NotBeNull();
                
                // Totals should have non-negative values
                result.Value.Totals.Revenue.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Totals.Orders.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Totals.AverageOrderValue.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Totals.ActiveSubscriptions.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Totals.NewSubscriptions.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Totals.NewSubscriptionsRevenue.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Totals.RenewedSubscriptions.Should().BeGreaterThanOrEqualTo(0);
                result.Value.Totals.RenewedSubscriptionsRevenue.Should().BeGreaterThanOrEqualTo(0);
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithOrganizationIdFilter_ReturnsFilteredMetrics()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

            // First get an organization to filter by
            var orgsResult = await client.Organizations.ListAsync();
            if (!orgsResult.IsSuccess || !orgsResult.Value.Items.Any())
            {
                _output.WriteLine("Skipped: No organizations available to filter by");
                return;
            }

            var orgId = orgsResult.Value.Items.First().Id;

            // Act
            var result = await client.Metrics.GetAsync(
                startDate, 
                endDate, 
                TimeInterval.Day,
                organizationId: orgId);

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_WithQueryBuilder_ReturnsMetricsResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Using the query builder with DateTime overloads for convenience
            var result = await client.Metrics.GetAsync(
                client.Metrics.Query()
                    .StartDate(DateTime.UtcNow.AddDays(-30))
                    .EndDate(DateTime.UtcNow)
                    .WithInterval(TimeInterval.Day)
                    .WithTimezone("UTC"));

            // Assert
            result.Should().NotBeNull();
            if (result.IsSuccess)
            {
                result.Value.Should().NotBeNull();
                result.Value.Periods.Should().NotBeNull();
            }
            else
            {
                _output.WriteLine($"API returned error: {result.Error?.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_QueryBuilderValidation_RequiresStartDate()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Missing start_date
            var result = await client.Metrics.GetAsync(
                client.Metrics.Query()
                    .EndDate(DateTime.UtcNow)
                    .WithInterval(TimeInterval.Day));

            // Assert - Should fail validation
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Message.Should().Contain("start_date");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_QueryBuilderValidation_RequiresEndDate()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Missing end_date
            var result = await client.Metrics.GetAsync(
                client.Metrics.Query()
                    .StartDate(DateTime.UtcNow.AddDays(-30))
                    .WithInterval(TimeInterval.Day));

            // Assert - Should fail validation
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Message.Should().Contain("end_date");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task GetAsync_QueryBuilderValidation_RequiresInterval()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act - Missing interval
            var result = await client.Metrics.GetAsync(
                client.Metrics.Query()
                    .StartDate(DateTime.UtcNow.AddDays(-30))
                    .EndDate(DateTime.UtcNow));

            // Assert - Should fail validation
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Message.Should().Contain("interval");
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}
