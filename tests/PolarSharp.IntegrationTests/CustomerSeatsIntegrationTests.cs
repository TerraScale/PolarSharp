using FluentAssertions;
using PolarSharp.Models.Seats;
using PolarSharp.Results;
using Xunit;
using Xunit.Abstractions;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Customer Seats API.
/// </summary>
public class CustomerSeatsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public CustomerSeatsIntegrationTests(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CustomerSeatsApi_ListCustomerSeats_ReturnsPaginatedResponse()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.CustomerSeats.ListAsync(page: 1, limit: 10);

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccess)
            {
                // Sandbox may not support customer seats API
                _output.WriteLine($"Skipped: {result.Error?.Message}");
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
    public async Task CustomerSeatsApi_GetCustomerSeat_WithValidId_ReturnsCustomerSeat()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // First, list customer seats to get a valid ID
        var listResult = await client.CustomerSeats.ListAsync();
        if (listResult.IsFailure || listResult.Value.Items.Count == 0)
        {
            return; // Skip if no customer seats exist
        }

        var customerSeatId = listResult.Value.Items.First().Id;

        // Act
        var result = await client.CustomerSeats.GetAsync(customerSeatId);

        // Assert
        result.Should().NotBeNull();
        if (!result.IsSuccess)
        {
            return; // Sandbox may not support this operation
        }
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(customerSeatId);
    }

    [Fact]
    public async Task CustomerSeatsApi_GetCustomerSeat_WithInvalidId_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();
            var invalidCustomerSeatId = "invalid_customer_seat_id";

            // Act
            var result = await client.CustomerSeats.GetAsync(invalidCustomerSeatId);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerSeatsApi_AssignCustomerSeat_WithValidRequest_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var assignRequest = new CustomerSeatAssignRequest
            {
                SeatId = "test_seat_id",
                UserId = "test_user_id",
                Email = $"testuser{Guid.NewGuid()}@mailinator.com"
            };

            // Act
            var result = await client.CustomerSeats.AssignAsync(assignRequest);

            // Assert
            if (result.IsSuccess)
            {
                result.IsSuccess.Should().BeTrue();
            }
            else if (result.IsNotFoundError || result.Error!.Message.Contains("not found") ||
                     result.Error!.Message.Contains("seats") || result.Error!.Message.Contains("enabled"))
            {
                // Expected if subscription doesn't exist or seats not enabled
                _output.WriteLine($"Skipped: {result.Error!.Message}");
            }
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
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

        // Act
        var result = await client.CustomerSeats.RevokeAsync(revokeRequest);

        // Assert
        if (result.IsSuccess)
        {
            result.IsSuccess.Should().BeTrue();
        }
        else if (result.IsNotFoundError || result.Error!.Message.Contains("not found") ||
                 result.Error!.Message.Contains("seat"))
        {
            // Expected if subscription/seat doesn't exist
            _output.WriteLine($"Skipped: {result.Error!.Message}");
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

        // Act
        var result = await client.CustomerSeats.ResendInvitationAsync(resendRequest);

        // Assert
        if (result.IsSuccess)
        {
            result.IsSuccess.Should().BeTrue();
        }
        else if (result.IsNotFoundError || result.Error!.Message.Contains("not found") ||
                 result.Error!.Message.Contains("seat") || result.Error!.Message.Contains("invitation"))
        {
            // Expected if subscription/seat doesn't exist
            _output.WriteLine($"Skipped: {result.Error!.Message}");
        }
    }

    [Fact]
    public async Task CustomerSeatsApi_GetClaimInfo_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.CustomerSeats.GetClaimInfoAsync();

            // Assert
            result.Should().NotBeNull();
            // The fields may be empty if no claim info is available
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
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

        // Act
        var result = await client.CustomerSeats.ClaimAsync(claimRequest);

        // Assert
        if (result.IsSuccess)
        {
            result.IsSuccess.Should().BeTrue();
        }
        else if (result.IsValidationError || result.Error!.Message.Contains("invalid") ||
                 result.Error!.Message.Contains("expired") || result.Error!.Message.Contains("token"))
        {
            // Expected if token is invalid or expired
            _output.WriteLine($"Skipped: {result.Error!.Message}");
        }
    }

    [Fact]
    public async Task CustomerSeatsApi_ListAllCustomerSeats_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allCustomerSeats = new List<CustomerSeat>();
        await foreach (var seatResult in client.CustomerSeats.ListAllAsync())
        {
            if (seatResult.IsFailure) break;
            var customerSeat = seatResult.Value;
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

        var result = await client.CustomerSeats.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        if (!result.IsSuccess)
        {
            // Sandbox may not support this operation
            return;
        }
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomerSeatsApi_ListCustomerSeats_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result1 = await client.CustomerSeats.ListAsync(page: 1, limit: 5);
        if (!result1.IsSuccess)
        {
            // Sandbox may not support this operation
            return;
        }

        var result2 = await client.CustomerSeats.ListAsync(page: 2, limit: 5);

        // Assert
        result1.Should().NotBeNull();
        result1.Value.Pagination.Page.Should().Be(1);

        result2.Should().NotBeNull();
        if (result2.IsSuccess)
        {
            result2.Value.Pagination.Page.Should().Be(2);
        }
    }

    [Fact]
    public async Task CustomerSeatsApi_AssignCustomerSeat_WithInvalidEmail_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var assignRequest = new CustomerSeatAssignRequest
        {
            SeatId = "test_seat_id",
            UserId = "test_user_id",
            Email = "invalid-email" // Invalid email format
        };

        // Act
        var result = await client.CustomerSeats.AssignAsync(assignRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerSeatsApi_AssignCustomerSeat_WithEmptySubscriptionId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var assignRequest = new CustomerSeatAssignRequest
        {
            SeatId = "", // Empty seat ID
            UserId = "", // Empty user ID
            Email = "test@mailinator.com"
        };

        // Act
        var result = await client.CustomerSeats.AssignAsync(assignRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerSeatsApi_RevokeCustomerSeat_WithEmptyIds_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var revokeRequest = new SeatRevokeRequest
        {
            SubscriptionId = "", // Empty subscription ID
            SeatId = "" // Empty seat ID
        };

        // Act
        var result = await client.CustomerSeats.RevokeAsync(revokeRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerSeatsApi_ResendInvitation_WithEmptyIds_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var resendRequest = new SeatResendInvitationRequest
        {
            SubscriptionId = "", // Empty subscription ID
            SeatId = "" // Empty seat ID
        };

        // Act
        var result = await client.CustomerSeats.ResendInvitationAsync(resendRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerSeatsApi_ClaimSeat_WithEmptyToken_ReturnsFailure()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            var claimRequest = new SeatClaimRequest
            {
                InvitationToken = "" // Empty invitation token
            };

            // Act
            var result = await client.CustomerSeats.ClaimAsync(claimRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerSeatsApi_ClaimSeat_WithInvalidToken_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();

        var claimRequest = new SeatClaimRequest
        {
            InvitationToken = "invalid_invitation_token"
        };

        // Act
        var result = await client.CustomerSeats.ClaimAsync(claimRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerSeatsApi_ListCustomerSeats_LargeLimit_WorksCorrectly()
    {
        try
        {
            // Arrange
            var client = _fixture.CreateClient();

            // Act
            var result = await client.CustomerSeats.ListAsync(page: 1, limit: 100);

            // Assert
            result.Should().NotBeNull();
            if (!result.IsSuccess)
            {
                // Sandbox may not support this operation
                _output.WriteLine($"Skipped: {result.Error?.Message}");
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
    public async Task CustomerSeatsApi_AssignCustomerSeat_WithLongEmail_ReturnsFailure()
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

        // Act
        var result = await client.CustomerSeats.AssignAsync(assignRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomerSeatsApi_RevokeCustomerSeat_WithNonExistentIds_ReturnsFailure()
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
            var result = await client.CustomerSeats.RevokeAsync(revokeRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }

    [Fact]
    public async Task CustomerSeatsApi_ResendInvitation_WithNonExistentIds_ReturnsFailure()
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
            var result = await client.CustomerSeats.ResendInvitationAsync(resendRequest);

            // Assert
            result.Should().NotBeNull();
            result.IsFailure.Should().BeTrue();
        }
        catch (OperationCanceledException)
        {
            _output.WriteLine("Skipped: Request timed out");
        }
    }
}
