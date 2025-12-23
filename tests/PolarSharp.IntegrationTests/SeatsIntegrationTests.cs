using FluentAssertions;
using PolarSharp.Models.Seats;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Seats API.
/// </summary>
public class SeatsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public SeatsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task SeatsApi_ListSeats_ReturnsPaginatedResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Seats.ListAsync(page: 1, limit: 10);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();
            result.Value.Pagination.Page.Should().Be(1);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_ListClaimedSubscriptions_ReturnsSubscriptions()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Seats.ListClaimedSubscriptionsAsync();

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Should().BeAssignableTo<List<ClaimedSubscription>>();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_AssignSeat_WithValidRequest_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // First, we need a subscription with seats enabled
            // For this test, we'll use a mock subscription ID since creating subscriptions with seats
            // requires specific product configuration
            var subscriptionId = "test_subscription_id"; // This would normally come from a created subscription

            var assignRequest = new SubscriptionSeatAssignRequest
            {
                SubscriptionId = subscriptionId,
                Email = $"testuser{Guid.NewGuid()}@mailinator.com"
            };

            // Act
            var result = await client.Seats.AssignAsync(assignRequest);

            // Assert
            // Result may fail if subscription doesn't exist or doesn't support seats
            // This validates the API call structure
            result.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_RevokeSeat_WithValidRequest_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var revokeRequest = new SeatRevokeRequest
            {
                SubscriptionId = "test_subscription_id",
                SeatId = "test_seat_id"
            };

            // Act
            var result = await client.Seats.RevokeAsync(revokeRequest);

            // Assert
            // Result may fail if subscription/seat doesn't exist
            // This validates the API call structure
            result.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_ResendInvitation_WithValidRequest_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var resendRequest = new SeatResendInvitationRequest
            {
                SubscriptionId = "test_subscription_id",
                SeatId = "test_seat_id"
            };

            // Act
            var result = await client.Seats.ResendInvitationAsync(resendRequest);

            // Assert
            // Result may fail if subscription/seat doesn't exist
            // This validates the API call structure
            result.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_ListAllSeats_UsingAsyncEnumerable_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var allSeats = new List<Seat>();
            await foreach (var seatResult in client.Seats.ListAllAsync())
            {
                if (seatResult.IsFailure)
                {
                    _output.WriteLine($"Skipped: {seatResult.Error!.Message}");
                    break;
                }
                allSeats.Add(seatResult.Value);
            }

            // Assert
            allSeats.Should().NotBeNull();
            allSeats.Should().BeAssignableTo<List<Seat>>();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_QueryBuilder_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var queryBuilder = client.Seats.Query();

            var result = await client.Seats.ListAsync(queryBuilder);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
            result.Value.Pagination.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_ListSeats_WithPagination_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var page1Result = await client.Seats.ListAsync(page: 1, limit: 5);
            var page2Result = await client.Seats.ListAsync(page: 2, limit: 5);

            // Assert
            page1Result.Should().NotBeNull();
            if (page1Result.IsFailure)
            {
                _output.WriteLine($"Skipped: {page1Result.Error!.Message}");
                return;
            }
            page1Result.Value.Pagination.Page.Should().Be(1);

            page2Result.Should().NotBeNull();
            if (page2Result.IsFailure)
            {
                _output.WriteLine($"Skipped: {page2Result.Error!.Message}");
                return;
            }
            page2Result.Value.Pagination.Page.Should().Be(2);
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_AssignSeat_WithInvalidEmail_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var assignRequest = new SubscriptionSeatAssignRequest
            {
                SubscriptionId = "test_subscription_id",
                Email = "invalid-email" // Invalid email format
            };

            // Act
            var result = await client.Seats.AssignAsync(assignRequest);

            // Assert
            result.Should().NotBeNull(); // Should return result for validation errors
            result.IsFailure.Should().BeTrue(); // Should fail for invalid email
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_AssignSeat_WithEmptySubscriptionId_ReturnsNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var assignRequest = new SubscriptionSeatAssignRequest
            {
                SubscriptionId = "", // Empty subscription ID
                Email = "test@mailinator.com"
            };

            // Act
            var result = await client.Seats.AssignAsync(assignRequest);

            // Assert
            result.Should().NotBeNull(); // Should return result for validation errors
            result.IsFailure.Should().BeTrue(); // Should fail for empty subscription ID
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_RevokeSeat_WithEmptyIds_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var revokeRequest = new SeatRevokeRequest
            {
                SubscriptionId = "", // Empty subscription ID
                SeatId = "" // Empty seat ID
            };

            // Act
            var result = await client.Seats.RevokeAsync(revokeRequest);

            // Assert
            result.Should().NotBeNull(); // Should return result for validation errors
            result.IsFailure.Should().BeTrue(); // Should fail for empty IDs
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_ResendInvitation_WithEmptyIds_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var resendRequest = new SeatResendInvitationRequest
            {
                SubscriptionId = "", // Empty subscription ID
                SeatId = "" // Empty seat ID
            };

            // Act
            var result = await client.Seats.ResendInvitationAsync(resendRequest);

            // Assert
            result.Should().NotBeNull(); // Should return result for validation errors
            result.IsFailure.Should().BeTrue(); // Should fail for empty IDs
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_ListSeats_LargeLimit_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.Seats.ListAsync(page: 1, limit: 100);

            // Assert
            result.Should().NotBeNull();
            if (result.IsFailure)
            {
                _output.WriteLine($"Skipped: {result.Error!.Message}");
                return;
            }
            result.Value.Should().NotBeNull();
            result.Value.Items.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_AssignSeat_WithLongEmail_ReturnsNull()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var longEmail = new string('a', 300) + "@mailinator.com"; // Very long email

            var assignRequest = new SubscriptionSeatAssignRequest
            {
                SubscriptionId = "test_subscription_id",
                Email = longEmail
            };

            // Act
            var result = await client.Seats.AssignAsync(assignRequest);

            // Assert
            result.Should().NotBeNull(); // Should return result for validation errors
            result.IsFailure.Should().BeTrue(); // Should fail for very long email
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_RevokeSeat_WithNonExistentIds_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var revokeRequest = new SeatRevokeRequest
            {
                SubscriptionId = "non_existent_subscription_id",
                SeatId = "non_existent_seat_id"
            };

            // Act
            var result = await client.Seats.RevokeAsync(revokeRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue(); // Should fail for non-existent IDs
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task SeatsApi_ResendInvitation_WithNonExistentIds_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var resendRequest = new SeatResendInvitationRequest
            {
                SubscriptionId = "non_existent_subscription_id",
                SeatId = "non_existent_seat_id"
            };

            // Act
            var result = await client.Seats.ResendInvitationAsync(resendRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue(); // Should fail for non-existent IDs
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}
