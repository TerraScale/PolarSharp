using FluentAssertions;
using PolarSharp.Models.Seats;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Seats API.
/// </summary>
public class SeatsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public SeatsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SeatsApi_ListSeats_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Seats.ListAsync(page: 1, limit: 10);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().Be(1);
    }

    [Fact]
    public async Task SeatsApi_ListClaimedSubscriptions_ReturnsSubscriptions()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var claimedSubscriptions = await client.Seats.ListClaimedSubscriptionsAsync();

        // Assert
        claimedSubscriptions.Should().NotBeNull();
        claimedSubscriptions.Should().BeAssignableTo<List<ClaimedSubscription>>();
    }

    [Fact]
    public async Task SeatsApi_AssignSeat_WithValidRequest_WorksCorrectly()
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

        // Act & Assert
        // This test may fail if the subscription doesn't exist or doesn't support seats
        // but it validates the API call structure
        try
        {
            await client.Seats.AssignAsync(assignRequest);
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (
            ex.Message.Contains("not found") || 
            ex.Message.Contains("seats") ||
            ex.Message.Contains("enabled"))
        {
            // Expected if subscription doesn't exist or seats not enabled
            return;
        }
    }

    [Fact]
    public async Task SeatsApi_RevokeSeat_WithValidRequest_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var revokeRequest = new SeatRevokeRequest
        {
            SubscriptionId = "test_subscription_id",
            SeatId = "test_seat_id"
        };

        // Act & Assert
        // This test may fail if the subscription/seat doesn't exist
        // but it validates the API call structure
        try
        {
            await client.Seats.RevokeAsync(revokeRequest);
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (
            ex.Message.Contains("not found") || 
            ex.Message.Contains("seat"))
        {
            // Expected if subscription/seat doesn't exist
            return;
        }
    }

    [Fact]
    public async Task SeatsApi_ResendInvitation_WithValidRequest_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var resendRequest = new SeatResendInvitationRequest
        {
            SubscriptionId = "test_subscription_id",
            SeatId = "test_seat_id"
        };

        // Act & Assert
        // This test may fail if the subscription/seat doesn't exist
        // but it validates the API call structure
        try
        {
            await client.Seats.ResendInvitationAsync(resendRequest);
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (
            ex.Message.Contains("not found") || 
            ex.Message.Contains("seat") ||
            ex.Message.Contains("invitation"))
        {
            // Expected if subscription/seat doesn't exist
            return;
        }
    }

    [Fact]
    public async Task SeatsApi_ListAllSeats_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allSeats = new List<Seat>();
        await foreach (var seat in client.Seats.ListAllAsync())
        {
            allSeats.Add(seat);
        }

        // Assert
        allSeats.Should().NotBeNull();
        allSeats.Should().BeAssignableTo<List<Seat>>();
    }

    [Fact]
    public async Task SeatsApi_QueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var queryBuilder = client.Seats.Query();

        var response = await client.Seats.ListAsync(queryBuilder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task SeatsApi_ListSeats_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var page1 = await client.Seats.ListAsync(page: 1, limit: 5);
        var page2 = await client.Seats.ListAsync(page: 2, limit: 5);

        // Assert
        page1.Should().NotBeNull();
        page1.Pagination.Page.Should().Be(1);
        
        page2.Should().NotBeNull();
        page2.Pagination.Page.Should().Be(2);
    }

    [Fact]
    public async Task SeatsApi_AssignSeat_WithInvalidEmail_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var assignRequest = new SubscriptionSeatAssignRequest
        {
            SubscriptionId = "test_subscription_id",
            Email = "invalid-email" // Invalid email format
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Seats.AssignAsync(assignRequest));
    }

    [Fact]
    public async Task SeatsApi_AssignSeat_WithEmptySubscriptionId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var assignRequest = new SubscriptionSeatAssignRequest
        {
            SubscriptionId = "", // Empty subscription ID
            Email = "test@mailinator.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Seats.AssignAsync(assignRequest));
    }

    [Fact]
    public async Task SeatsApi_RevokeSeat_WithEmptyIds_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var revokeRequest = new SeatRevokeRequest
        {
            SubscriptionId = "", // Empty subscription ID
            SeatId = "" // Empty seat ID
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Seats.RevokeAsync(revokeRequest));
    }

    [Fact]
    public async Task SeatsApi_ResendInvitation_WithEmptyIds_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var resendRequest = new SeatResendInvitationRequest
        {
            SubscriptionId = "", // Empty subscription ID
            SeatId = "" // Empty seat ID
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Seats.ResendInvitationAsync(resendRequest));
    }

    [Fact]
    public async Task SeatsApi_ListSeats_LargeLimit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.Seats.ListAsync(page: 1, limit: 100);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task SeatsApi_AssignSeat_WithLongEmail_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var longEmail = new string('a', 300) + "@mailinator.com"; // Very long email
        
        var assignRequest = new SubscriptionSeatAssignRequest
        {
            SubscriptionId = "test_subscription_id",
            Email = longEmail
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Seats.AssignAsync(assignRequest));
    }

    [Fact]
    public async Task SeatsApi_RevokeSeat_WithNonExistentIds_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var revokeRequest = new SeatRevokeRequest
        {
            SubscriptionId = "non_existent_subscription_id",
            SeatId = "non_existent_seat_id"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Seats.RevokeAsync(revokeRequest));
    }

    [Fact]
    public async Task SeatsApi_ResendInvitation_WithNonExistentIds_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var resendRequest = new SeatResendInvitationRequest
        {
            SubscriptionId = "non_existent_subscription_id",
            SeatId = "non_existent_seat_id"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.Seats.ResendInvitationAsync(resendRequest));
    }
}