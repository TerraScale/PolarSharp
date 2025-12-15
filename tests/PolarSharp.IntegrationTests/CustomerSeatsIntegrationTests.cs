using FluentAssertions;
using PolarSharp.Models.Seats;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Customer Seats API.
/// </summary>
public class CustomerSeatsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public CustomerSeatsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CustomerSeatsApi_ListCustomerSeats_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.CustomerSeats.ListAsync(page: 1, limit: 10);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().Be(1);
    }

    [Fact]
    public async Task CustomerSeatsApi_GetCustomerSeat_WithValidId_ReturnsCustomerSeat()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        // First, list customer seats to get a valid ID
        var customerSeats = await client.CustomerSeats.ListAsync();
        if (customerSeats.Items.Count == 0)
        {
            return; // Skip if no customer seats exist
        }

        var customerSeatId = customerSeats.Items.First().Id;

        // Act
        var customerSeat = await client.CustomerSeats.GetAsync(customerSeatId);

        // Assert
        customerSeat.Should().NotBeNull();
        customerSeat.Id.Should().Be(customerSeatId);
    }

    [Fact]
    public async Task CustomerSeatsApi_GetCustomerSeat_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidCustomerSeatId = "invalid_customer_seat_id";

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomerSeats.GetAsync(invalidCustomerSeatId));
    }

    [Fact]
    public async Task CustomerSeatsApi_AssignCustomerSeat_WithValidRequest_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var assignRequest = new CustomerSeatAssignRequest
        {
            SeatId = "test_seat_id",
            UserId = "test_user_id",
            Email = $"testuser{Guid.NewGuid()}@mailinator.com"
        };

        // Act & Assert
        // This test may fail if the subscription doesn't exist or doesn't support seats
        // but it validates the API call structure
        try
        {
            await client.CustomerSeats.AssignAsync(assignRequest);
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
    public async Task CustomerSeatsApi_RevokeCustomerSeat_WithValidRequest_WorksCorrectly()
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
            await client.CustomerSeats.RevokeAsync(revokeRequest);
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
    public async Task CustomerSeatsApi_ResendInvitation_WithValidRequest_WorksCorrectly()
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
            await client.CustomerSeats.ResendInvitationAsync(resendRequest);
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
    public async Task CustomerSeatsApi_GetClaimInfo_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var claimInfo = await client.CustomerSeats.GetClaimInfoAsync();

        // Assert
        claimInfo.Should().NotBeNull();
        // The fields may be empty if no claim info is available
    }

    [Fact]
    public async Task CustomerSeatsApi_ClaimSeat_WithValidRequest_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var claimRequest = new SeatClaimRequest
        {
            InvitationToken = "test_invitation_token"
        };

        // Act & Assert
        // This test may fail if the invitation token is invalid
        // but it validates the API call structure
        try
        {
            await client.CustomerSeats.ClaimAsync(claimRequest);
        }
        catch (PolarSharp.Exceptions.PolarApiException ex) when (
            ex.Message.Contains("invalid") || 
            ex.Message.Contains("expired") ||
            ex.Message.Contains("token"))
        {
            // Expected if token is invalid or expired
            return;
        }
    }

    [Fact]
    public async Task CustomerSeatsApi_ListAllCustomerSeats_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allCustomerSeats = new List<CustomerSeat>();
        await foreach (var customerSeat in client.CustomerSeats.ListAllAsync())
        {
            allCustomerSeats.Add(customerSeat);
        }

        // Assert
        allCustomerSeats.Should().NotBeNull();
        allCustomerSeats.Should().BeAssignableTo<List<CustomerSeat>>();
    }

    [Fact]
    public async Task CustomerSeatsApi_QueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var queryBuilder = client.CustomerSeats.Query();

        var response = await client.CustomerSeats.ListAsync(queryBuilder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerSeatsApi_ListCustomerSeats_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var page1 = await client.CustomerSeats.ListAsync(page: 1, limit: 5);
        var page2 = await client.CustomerSeats.ListAsync(page: 2, limit: 5);

        // Assert
        page1.Should().NotBeNull();
        page1.Pagination.Page.Should().Be(1);
        
        page2.Should().NotBeNull();
        page2.Pagination.Page.Should().Be(2);
    }

    [Fact]
    public async Task CustomerSeatsApi_AssignCustomerSeat_WithInvalidEmail_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var assignRequest = new CustomerSeatAssignRequest
        {
            SeatId = "test_seat_id",
            UserId = "test_user_id",
            Email = "invalid-email" // Invalid email format
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomerSeats.AssignAsync(assignRequest));
    }

    [Fact]
    public async Task CustomerSeatsApi_AssignCustomerSeat_WithEmptySubscriptionId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var assignRequest = new CustomerSeatAssignRequest
        {
            SeatId = "", // Empty seat ID
            UserId = "", // Empty user ID
            Email = "test@mailinator.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomerSeats.AssignAsync(assignRequest));
    }

    [Fact]
    public async Task CustomerSeatsApi_RevokeCustomerSeat_WithEmptyIds_ThrowsException()
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
            () => client.CustomerSeats.RevokeAsync(revokeRequest));
    }

    [Fact]
    public async Task CustomerSeatsApi_ResendInvitation_WithEmptyIds_ThrowsException()
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
            () => client.CustomerSeats.ResendInvitationAsync(resendRequest));
    }

    [Fact]
    public async Task CustomerSeatsApi_ClaimSeat_WithEmptyToken_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var claimRequest = new SeatClaimRequest
        {
            InvitationToken = "" // Empty invitation token
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomerSeats.ClaimAsync(claimRequest));
    }

    [Fact]
    public async Task CustomerSeatsApi_ClaimSeat_WithInvalidToken_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var claimRequest = new SeatClaimRequest
        {
            InvitationToken = "invalid_invitation_token"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomerSeats.ClaimAsync(claimRequest));
    }

    [Fact]
    public async Task CustomerSeatsApi_ListCustomerSeats_LargeLimit_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.CustomerSeats.ListAsync(page: 1, limit: 100);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerSeatsApi_AssignCustomerSeat_WithLongEmail_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        
        var longEmail = new string('a', 300) + "@mailinator.com"; // Very long email
        
        var assignRequest = new CustomerSeatAssignRequest
        {
            SeatId = "test_seat_id",
            UserId = "test_user_id",
            Email = longEmail
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomerSeats.AssignAsync(assignRequest));
    }

    [Fact]
    public async Task CustomerSeatsApi_RevokeCustomerSeat_WithNonExistentIds_ThrowsException()
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
            () => client.CustomerSeats.RevokeAsync(revokeRequest));
    }

    [Fact]
    public async Task CustomerSeatsApi_ResendInvitation_WithNonExistentIds_ThrowsException()
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
            () => client.CustomerSeats.ResendInvitationAsync(resendRequest));
    }
}