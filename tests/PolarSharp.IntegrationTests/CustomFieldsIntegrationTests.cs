using FluentAssertions;
using PolarSharp.Models.CustomFields;
using Xunit;

namespace PolarSharp.IntegrationTests;

/// <summary>
/// Integration tests for Custom Fields API.
/// </summary>
public class CustomFieldsIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly IntegrationTestFixture _fixture;

    public CustomFieldsIntegrationTests(IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CustomFieldsApi_ListCustomFields_ReturnsPaginatedResponse()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var response = await client.CustomFields.ListAsync(page: 1, limit: 10);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
        response.Pagination.Page.Should().Be(1);
    }

    [Fact]
    public async Task CustomFieldsApi_CreateCustomField_WithTextType_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Test Text Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Description = "Integration test text field",
            Type = CustomFieldType.Text,
            IsRequired = true,
            Metadata = new Dictionary<string, object>
            {
                ["test"] = true,
                ["environment"] = "integration"
            }
        };

        // Act
        var createdField = await client.CustomFields.CreateAsync(createRequest);

        // Assert
        createdField.Should().NotBeNull();
        createdField.Id.Should().NotBeNullOrEmpty();
        createdField.Name.Should().Be(fieldName);
        createdField.Description.Should().Be("Integration test text field");
        createdField.Type.Should().Be(CustomFieldType.Text);
        createdField.IsRequired.Should().BeTrue();
        createdField.IsActive.Should().BeTrue();
        createdField.Metadata.Should().NotBeNull();
        createdField.Metadata.Should().ContainKey("test");

        // Cleanup
        try
        {
            await client.CustomFields.DeleteAsync(createdField.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task CustomFieldsApi_CreateCustomField_WithSelectType_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Test Select Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Description = "Integration test select field",
            Type = CustomFieldType.Select,
            IsRequired = false,
            Options = new List<CustomFieldOptionCreateRequest>
            {
                new CustomFieldOptionCreateRequest
                {
                    Label = "Option 1",
                    Value = "option1",
                    SortOrder = 1
                },
                new CustomFieldOptionCreateRequest
                {
                    Label = "Option 2",
                    Value = "option2",
                    SortOrder = 2
                },
                new CustomFieldOptionCreateRequest
                {
                    Label = "Option 3",
                    Value = "option3",
                    SortOrder = 3
                }
            }
        };

        // Act
        var createdField = await client.CustomFields.CreateAsync(createRequest);

        // Assert
        createdField.Should().NotBeNull();
        createdField.Id.Should().NotBeNullOrEmpty();
        createdField.Name.Should().Be(fieldName);
        createdField.Type.Should().Be(CustomFieldType.Select);
        createdField.IsRequired.Should().BeFalse();
        createdField.Options.Should().HaveCount(3);
        createdField.Options.Should().Contain(o => o.Value == "option1");
        createdField.Options.Should().Contain(o => o.Value == "option2");
        createdField.Options.Should().Contain(o => o.Value == "option3");

        // Cleanup
        try
        {
            await client.CustomFields.DeleteAsync(createdField.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task CustomFieldsApi_GetCustomField_WithValidId_ReturnsCustomField()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Test Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Type = CustomFieldType.Boolean
        };

        var createdField = await client.CustomFields.CreateAsync(createRequest);

        // Act
        var retrievedField = await client.CustomFields.GetAsync(createdField.Id);

        // Assert
        retrievedField.Should().NotBeNull();
        retrievedField.Id.Should().Be(createdField.Id);
        retrievedField.Name.Should().Be(fieldName);
        retrievedField.Type.Should().Be(CustomFieldType.Boolean);

        // Cleanup
        try
        {
            await client.CustomFields.DeleteAsync(createdField.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task CustomFieldsApi_GetCustomField_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidFieldId = "invalid_field_id";

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomFields.GetAsync(invalidFieldId));
    }

    [Fact]
    public async Task CustomFieldsApi_UpdateCustomField_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Test Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Description = "Original description",
            Type = CustomFieldType.Text,
            IsRequired = false
        };

        var createdField = await client.CustomFields.CreateAsync(createRequest);

        // Update request
        var updateRequest = new CustomFieldUpdateRequest
        {
            Name = $"{fieldName} (Updated)",
            Description = "Updated description",
            IsRequired = true,
            IsActive = false,
            Metadata = new Dictionary<string, object>
            {
                ["updated"] = true,
                ["version"] = 2
            }
        };

        // Act
        var updatedField = await client.CustomFields.UpdateAsync(createdField.Id, updateRequest);

        // Assert
        updatedField.Should().NotBeNull();
        updatedField.Id.Should().Be(createdField.Id);
        updatedField.Name.Should().Be($"{fieldName} (Updated)");
        updatedField.Description.Should().Be("Updated description");
        updatedField.IsRequired.Should().BeTrue();
        updatedField.IsActive.Should().BeFalse();
        updatedField.Metadata.Should().ContainKey("updated");

        // Cleanup
        try
        {
            await client.CustomFields.DeleteAsync(updatedField.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task CustomFieldsApi_DeleteCustomField_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Test Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Type = CustomFieldType.Number
        };

        var createdField = await client.CustomFields.CreateAsync(createRequest);

        // Act
        await client.CustomFields.DeleteAsync(createdField.Id);

        // Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomFields.GetAsync(createdField.Id));
    }

    [Fact]
    public async Task CustomFieldsApi_DeleteCustomField_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidFieldId = "invalid_field_id";

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomFields.DeleteAsync(invalidFieldId));
    }

    [Fact]
    public async Task CustomFieldsApi_CreateCustomField_WithAllTypes_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldTypes = new[]
        {
            CustomFieldType.Text,
            CustomFieldType.Number,
            CustomFieldType.Boolean,
            CustomFieldType.Date,
            CustomFieldType.Textarea,
            CustomFieldType.Url,
            CustomFieldType.Email
        };

        var createdFields = new List<CustomField>();

        foreach (var fieldType in fieldTypes)
        {
            var fieldName = $"Test {fieldType} Field {Guid.NewGuid()}";

            var createRequest = new CustomFieldCreateRequest
            {
                Name = fieldName,
                Description = $"Test {fieldType} field",
                Type = fieldType,
                IsRequired = false
            };

            // Act
            var createdField = await client.CustomFields.CreateAsync(createRequest);
            createdFields.Add(createdField);

            // Assert
            createdField.Should().NotBeNull();
            createdField.Type.Should().Be(fieldType);
        }

        // Cleanup
        foreach (var field in createdFields)
        {
            try
            {
                await client.CustomFields.DeleteAsync(field.Id);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task CustomFieldsApi_UpdateCustomField_WithPartialData_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Partial Update Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Description = "Original description",
            Type = CustomFieldType.Text,
            IsRequired = false
        };

        var createdField = await client.CustomFields.CreateAsync(createRequest);

        // Update only the description
        var updateRequest = new CustomFieldUpdateRequest
        {
            Description = "Updated description only"
        };

        // Act
        var updatedField = await client.CustomFields.UpdateAsync(createdField.Id, updateRequest);

        // Assert
        updatedField.Should().NotBeNull();
        updatedField.Id.Should().Be(createdField.Id);
        updatedField.Name.Should().Be(fieldName); // Should remain unchanged
        updatedField.Description.Should().Be("Updated description only");
        updatedField.Type.Should().Be(CustomFieldType.Text); // Should remain unchanged
        updatedField.IsRequired.Should().BeFalse(); // Should remain unchanged

        // Cleanup
        try
        {
            await client.CustomFields.DeleteAsync(updatedField.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task CustomFieldsApi_UpdateCustomField_WithInvalidId_ThrowsException()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidFieldId = "invalid_field_id";

        var updateRequest = new CustomFieldUpdateRequest
        {
            Name = "Updated Name"
        };

        // Act & Assert
        await Assert.ThrowsAsync<PolarSharp.Exceptions.PolarApiException>(
            () => client.CustomFields.UpdateAsync(invalidFieldId, updateRequest));
    }

    [Fact]
    public async Task CustomFieldsApi_ListAllCustomFields_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allFields = new List<CustomField>();
        await foreach (var field in client.CustomFields.ListAllAsync())
        {
            allFields.Add(field);
        }

        // Assert
        allFields.Should().NotBeNull();
        allFields.Should().BeAssignableTo<List<CustomField>>();
    }

    [Fact]
    public async Task CustomFieldsApi_QueryBuilder_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Query Test Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Type = CustomFieldType.Text
        };

        await client.CustomFields.CreateAsync(createRequest);

        // Act
        var queryBuilder = client.CustomFields.Query();

        var response = await client.CustomFields.ListAsync(queryBuilder);

        // Assert
        response.Should().NotBeNull();
        response.Items.Should().NotBeNull();
        response.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomFieldsApi_ListCustomFields_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var page1 = await client.CustomFields.ListAsync(page: 1, limit: 5);
        var page2 = await client.CustomFields.ListAsync(page: 2, limit: 5);

        // Assert
        page1.Should().NotBeNull();
        page1.Pagination.Page.Should().Be(1);
        
        page2.Should().NotBeNull();
        page2.Pagination.Page.Should().Be(2);
    }

    [Fact]
    public async Task CustomFieldsApi_CreateCustomField_WithMultiSelectType_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Test Multi-Select Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Description = "Integration test multi-select field",
            Type = CustomFieldType.MultiSelect,
            IsRequired = false,
            Options = new List<CustomFieldOptionCreateRequest>
            {
                new CustomFieldOptionCreateRequest
                {
                    Label = "Tag A",
                    Value = "tag_a",
                    SortOrder = 1
                },
                new CustomFieldOptionCreateRequest
                {
                    Label = "Tag B",
                    Value = "tag_b",
                    SortOrder = 2
                }
            }
        };

        // Act
        var createdField = await client.CustomFields.CreateAsync(createRequest);

        // Assert
        createdField.Should().NotBeNull();
        createdField.Type.Should().Be(CustomFieldType.MultiSelect);
        createdField.Options.Should().HaveCount(2);
        createdField.Options.Should().Contain(o => o.Value == "tag_a");
        createdField.Options.Should().Contain(o => o.Value == "tag_b");

        // Cleanup
        try
        {
            await client.CustomFields.DeleteAsync(createdField.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    [Fact]
    public async Task CustomFieldsApi_CreateCustomField_WithMinimalData_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var fieldName = $"Minimal Field {Guid.NewGuid()}";

        var createRequest = new CustomFieldCreateRequest
        {
            Name = fieldName,
            Type = CustomFieldType.Text
        };

        // Act
        var createdField = await client.CustomFields.CreateAsync(createRequest);

        // Assert
        createdField.Should().NotBeNull();
        createdField.Id.Should().NotBeNullOrEmpty();
        createdField.Name.Should().Be(fieldName);
        createdField.Type.Should().Be(CustomFieldType.Text);
        createdField.Description.Should().BeNull();
        createdField.IsRequired.Should().BeFalse();
        createdField.Metadata.Should().BeNull();

        // Cleanup
        try
        {
            await client.CustomFields.DeleteAsync(createdField.Id);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}