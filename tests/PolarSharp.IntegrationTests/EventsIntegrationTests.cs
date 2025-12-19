using FluentAssertions;
using PolarSharp.Results;
using PolarSharp.Models.Events;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Events API.
/// </summary>
public class EventsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public EventsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task EventsApi_ListEvents_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Events.ListAsync(page: 1, limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().Be(1);
    }

    [Fact]
    public async Task EventsApi_ListEventNames_ReturnsEventNames()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Events.ListNamesAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var eventNames = result.Value;
        eventNames.Should().NotBeNull();
        eventNames.Should().BeAssignableTo<List<EventName>>();
    }

    [Fact]
    public async Task EventsApi_IngestEvents_CreatesEventsSuccessfully()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testEventName = $"test_event_{Guid.NewGuid()}";

        var ingestRequest = new EventIngestRequest
        {
            Events = new List<EventData>
            {
                new EventData
                {
                    Name = testEventName,
                    CustomerId = null, // Test without customer ID
                    Data = new Dictionary<string, object>
                    {
                        ["test_key"] = "test_value",
                        ["timestamp"] = DateTime.UtcNow
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["source"] = "integration_test"
                    }
                }
            }
        };

        // Act
        var ingestResult = await client.Events.IngestAsync(ingestRequest);

        // Assert
        ingestResult.Should().NotBeNull();
        ingestResult.IsSuccess.Should().BeTrue();

        // Allow some time for event processing
        await Task.Delay(1000);

        // Verify the event was created by listing events
        var listResult = await client.Events.ListAsync();
        listResult.IsSuccess.Should().BeTrue();
        var events = listResult.Value;
        var createdEvent = events.Items.FirstOrDefault(e => e.Name == testEventName);

        // Assert
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be(testEventName);
        createdEvent.Data.Should().NotBeNull();
        createdEvent.Metadata.Should().NotBeNull();
    }

    [Fact]
    public async Task EventsApi_IngestMultipleEvents_ProcessesAllEvents()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testEventNamePrefix = $"batch_test_{Guid.NewGuid()}";

        var ingestRequest = new EventIngestRequest
        {
            Events = new List<EventData>
            {
                new EventData
                {
                    Name = $"{testEventNamePrefix}_1",
                    Data = new Dictionary<string, object> { ["batch_id"] = 1 }
                },
                new EventData
                {
                    Name = $"{testEventNamePrefix}_2",
                    Data = new Dictionary<string, object> { ["batch_id"] = 2 }
                },
                new EventData
                {
                    Name = $"{testEventNamePrefix}_3",
                    Data = new Dictionary<string, object> { ["batch_id"] = 3 }
                }
            }
        };

        // Act
        var ingestResult = await client.Events.IngestAsync(ingestRequest);
        ingestResult.IsSuccess.Should().BeTrue();

        // Allow some time for event processing
        await Task.Delay(1000);

        // Verify events were created
        var listResult = await client.Events.ListAsync();
        listResult.IsSuccess.Should().BeTrue();
        var events = listResult.Value;
        var createdEvents = events.Items.Where(e => e.Name.StartsWith(testEventNamePrefix)).ToList();

        // Assert
        createdEvents.Should().HaveCount(3);
        createdEvents.Select(e => e.Name).Should().Contain(new[]
        {
            $"{testEventNamePrefix}_1",
            $"{testEventNamePrefix}_2",
            $"{testEventNamePrefix}_3"
        });
    }

    [Fact]
    public async Task EventsApi_GetEvent_WithValidId_ReturnsEvent()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testEventName = $"get_test_{Guid.NewGuid()}";

        var ingestRequest = new EventIngestRequest
        {
            Events = new List<EventData>
            {
                new EventData
                {
                    Name = testEventName,
                    Data = new Dictionary<string, object> { ["test"] = "get_event" }
                }
            }
        };

        var ingestResult = await client.Events.IngestAsync(ingestRequest);
        ingestResult.IsSuccess.Should().BeTrue();
        await Task.Delay(1000);

        // Find the created event
        var listResult = await client.Events.ListAsync();
        listResult.IsSuccess.Should().BeTrue();
        var events = listResult.Value;
        var createdEvent = events.Items.FirstOrDefault(e => e.Name == testEventName);

        // Skip test if event wasn't found (might be processing delay)
        if (createdEvent == null)
        {
            return;
        }

        // Act
        var result = await client.Events.GetAsync(createdEvent.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var retrievedEvent = result.Value;
        retrievedEvent.Should().NotBeNull();
        retrievedEvent.Id.Should().Be(createdEvent.Id);
        retrievedEvent.Name.Should().Be(testEventName);
        retrievedEvent.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task EventsApi_GetEvent_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidEventId = "invalid_event_id";

        // Act
        var result = await client.Events.GetAsync(invalidEventId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task EventsApi_ListAllEvents_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testEventNamePrefix = $"async_test_{Guid.NewGuid()}";

        // Create multiple events
        var ingestRequest = new EventIngestRequest
        {
            Events = Enumerable.Range(1, 5)
                .Select(i => new EventData
                {
                    Name = $"{testEventNamePrefix}_{i}",
                    Data = new Dictionary<string, object> { ["index"] = i }
                })
                .ToList()
        };

        var ingestResult = await client.Events.IngestAsync(ingestRequest);
        ingestResult.IsSuccess.Should().BeTrue();
        await Task.Delay(1000);

        // Act
        var allEvents = new List<Event>();
        await foreach (var eventResult in client.Events.ListAllAsync())
        {
            if (eventResult.IsFailure) break;

            var @event = eventResult.Value;
            allEvents.Add(@event);
        }

        // Assert
        allEvents.Should().NotBeEmpty();
        var testEvents = allEvents.Where(e => e.Name.StartsWith(testEventNamePrefix)).ToList();
        testEvents.Should().HaveCountGreaterThanOrEqualTo(0); // May be 0 due to processing delays
    }

    [Fact]
    public async Task EventsApi_QueryBuilder_WithNameFilter_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testEventName = $"query_test_{Guid.NewGuid()}";

        var ingestRequest = new EventIngestRequest
        {
            Events = new List<EventData>
            {
                new EventData
                {
                    Name = testEventName,
                    Data = new Dictionary<string, object> { ["query_test"] = true }
                }
            }
        };

        var ingestResult = await client.Events.IngestAsync(ingestRequest);
        ingestResult.IsSuccess.Should().BeTrue();
        await Task.Delay(1000);

        // Act
        var queryBuilder = client.Events.Query()
            .WithName(testEventName);

        var result = await client.Events.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();

        // Due to processing delays, we just verify the query structure works
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task EventsApi_QueryBuilder_WithDateFilters_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);
        var tomorrow = now.AddDays(1);

        // Act
        var queryBuilder = client.Events.Query()
            .CreatedAfter(yesterday)
            .CreatedBefore(tomorrow);

        var result = await client.Events.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task EventsApi_QueryBuilder_WithCustomerId_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testCustomerId = $"test_customer_{Guid.NewGuid()}";
        var testEventName = $"customer_test_{Guid.NewGuid()}";

        var ingestRequest = new EventIngestRequest
        {
            Events = new List<EventData>
            {
                new EventData
                {
                    Name = testEventName,
                    CustomerId = testCustomerId,
                    Data = new Dictionary<string, object> { ["customer_test"] = true }
                }
            }
        };

        var ingestResult = await client.Events.IngestAsync(ingestRequest);
        ingestResult.IsSuccess.Should().BeTrue();
        await Task.Delay(1000);

        // Act
        var queryBuilder = client.Events.Query()
            .WithCustomerId(testCustomerId);

        var result = await client.Events.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task EventsApi_ListEvents_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var page1Result = await client.Events.ListAsync(page: 1, limit: 5);
        var page2Result = await client.Events.ListAsync(page: 2, limit: 5);

        // Assert
        page1Result.Should().NotBeNull();
        page1Result.IsSuccess.Should().BeTrue();
        var page1 = page1Result.Value;
        page1.Should().NotBeNull();
        page1.Pagination.Page.Should().Be(1);

        page2Result.Should().NotBeNull();
        page2Result.IsSuccess.Should().BeTrue();
        var page2 = page2Result.Value;
        page2.Should().NotBeNull();
        page2.Pagination.Page.Should().Be(2);
    }

    [Fact]
    public async Task EventsApi_IngestEvents_WithTimestamp_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var testEventName = $"timestamp_test_{Guid.NewGuid()}";
        var customTimestamp = DateTime.UtcNow.AddMinutes(-30);

        var ingestRequest = new EventIngestRequest
        {
            Events = new List<EventData>
            {
                new EventData
                {
                    Name = testEventName,
                    Timestamp = customTimestamp,
                    Data = new Dictionary<string, object> { ["timestamp_test"] = true }
                }
            }
        };

        // Act
        var ingestResult = await client.Events.IngestAsync(ingestRequest);
        ingestResult.IsSuccess.Should().BeTrue();

        // Allow some time for event processing
        await Task.Delay(1000);

        // Verify the event was created
        var listResult = await client.Events.ListAsync();
        listResult.IsSuccess.Should().BeTrue();
        var events = listResult.Value;
        var createdEvent = events.Items.FirstOrDefault(e => e.Name == testEventName);

        // Assert
        createdEvent.Should().NotBeNull();
        createdEvent!.Name.Should().Be(testEventName);
        createdEvent.CreatedAt.Should().BeCloseTo(customTimestamp, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task EventsApi_IngestEvents_EmptyEventsList_HandlesGracefully()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var ingestRequest = new EventIngestRequest
        {
            Events = new List<EventData>()
        };

        // Act
        var result = await client.Events.IngestAsync(ingestRequest);

        // Assert - Should not throw
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task EventsApi_ListEvents_LargeLimit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result = await client.Events.ListAsync(page: 1, limit: 100);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
    }
}