using FluentAssertions;
using PolarSharp.Models.CustomFields;
using PolarSharp.Results;
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
        var result = await client.CustomFields.ListAsync(page: 1, limit: 10);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
        result.Value.Pagination.Page.Should().Be(1);
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
        createdField.Value.Id.Should().NotBeNullOrEmpty();
        createdField.Value.Name.Should().Be(fieldName);
        createdField.Value.Description.Should().Be("Integration test text field");
        createdField.Value.Type.Should().Be(CustomFieldType.Text);
        createdField.Value.IsRequired.Should().BeTrue();
        createdField.Value.IsActive.Should().BeTrue();
        createdField.Value.Metadata.Should().NotBeNull();
        createdField.Value.Metadata.Should().ContainKey("test");

        // Cleanup
        var deleteResult = await client.CustomFields.DeleteAsync(createdField.Value.Id);
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
        createdField.Value.Id.Should().NotBeNullOrEmpty();
        createdField.Value.Name.Should().Be(fieldName);
        createdField.Value.Type.Should().Be(CustomFieldType.Select);
        createdField.Value.IsRequired.Should().BeFalse();
        createdField.Value.Options.Should().HaveCount(3);
        createdField.Value.Options.Should().Contain(o => o.Value == "option1");
        createdField.Value.Options.Should().Contain(o => o.Value == "option2");
        createdField.Value.Options.Should().Contain(o => o.Value == "option3");

        // Cleanup
        var deleteResult = await client.CustomFields.DeleteAsync(createdField.Value.Id);
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
        var retrievedField = await client.CustomFields.GetAsync(createdField.Value.Id);

        // Assert
        retrievedField.Should().NotBeNull();
        retrievedField.IsSuccess.Should().BeTrue();
        retrievedField.Value.Id.Should().Be(createdField.Value.Id);
        retrievedField.Value.Name.Should().Be(fieldName);
        retrievedField.Value.Type.Should().Be(CustomFieldType.Boolean);

        // Cleanup
        var deleteResult = await client.CustomFields.DeleteAsync(createdField.Value.Id);
    }

    [Fact]
    public async Task CustomFieldsApi_GetCustomField_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidFieldId = "invalid_field_id";

        // Act
        var result = await client.CustomFields.GetAsync(invalidFieldId);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
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
        var updatedField = await client.CustomFields.UpdateAsync(createdField.Value.Id, updateRequest);

        // Assert
        updatedField.Should().NotBeNull();
        updatedField.IsSuccess.Should().BeTrue();
        updatedField.Value.Id.Should().Be(createdField.Value.Id);
        updatedField.Value.Name.Should().Be($"{fieldName} (Updated)");
        updatedField.Value.Description.Should().Be("Updated description");
        updatedField.Value.IsRequired.Should().BeTrue();
        updatedField.Value.IsActive.Should().BeFalse();
        updatedField.Value.Metadata.Should().ContainKey("updated");

        // Cleanup
        var deleteResult = await client.CustomFields.DeleteAsync(updatedField.Value.Id);
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
        var deleteResult = await client.CustomFields.DeleteAsync(createdField.Value.Id);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue();

        // After deletion, getting the field should return failure
        var afterDelete = await client.CustomFields.GetAsync(createdField.Value.Id);
        afterDelete.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomFieldsApi_DeleteCustomField_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidFieldId = "invalid_field_id";

        // Act
        var result = await client.CustomFields.DeleteAsync(invalidFieldId);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
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
            createdFields.Add(createdField.Value);

            // Assert
            createdField.Should().NotBeNull();
            createdField.IsSuccess.Should().BeTrue();
            createdField.Value.Type.Should().Be(fieldType);
        }

        // Cleanup
        foreach (var field in createdFields)
        {
            var deleteResult = await client.CustomFields.DeleteAsync(field.Id);
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
        var updatedField = await client.CustomFields.UpdateAsync(createdField.Value.Id, updateRequest);

        // Assert
        updatedField.Should().NotBeNull();
        updatedField.IsSuccess.Should().BeTrue();
        updatedField.Value.Id.Should().Be(createdField.Value.Id);
        updatedField.Value.Name.Should().Be(fieldName); // Should remain unchanged
        updatedField.Value.Description.Should().Be("Updated description only");
        updatedField.Value.Type.Should().Be(CustomFieldType.Text); // Should remain unchanged
        updatedField.Value.IsRequired.Should().BeFalse(); // Should remain unchanged

        // Cleanup
        var deleteResult = await client.CustomFields.DeleteAsync(updatedField.Value.Id);
    }

    [Fact]
    public async Task CustomFieldsApi_UpdateCustomField_WithInvalidId_ReturnsFailure()
    {
        // Arrange
        var client = _fixture.CreateClient();
        var invalidFieldId = "invalid_field_id";

        var updateRequest = new CustomFieldUpdateRequest
        {
            Name = "Updated Name"
        };

        // Act
        var result = await client.CustomFields.UpdateAsync(invalidFieldId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CustomFieldsApi_ListAllCustomFields_UsingAsyncEnumerable_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var allFields = new List<CustomField>();
        await foreach (var fieldResult in client.CustomFields.ListAllAsync())
        {
            if (fieldResult.IsFailure) break;
            var field = fieldResult.Value;
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

        var result = await client.CustomFields.ListAsync(queryBuilder);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().NotBeNull();
        result.Value.Pagination.Should().NotBeNull();
    }

    [Fact]
    public async Task CustomFieldsApi_ListCustomFields_WithPagination_WorksCorrectly()
    {
        // Arrange
        var client = _fixture.CreateClient();

        // Act
        var result1 = await client.CustomFields.ListAsync(page: 1, limit: 5);
        var result2 = await client.CustomFields.ListAsync(page: 2, limit: 5);

        // Assert
        result1.Should().NotBeNull();
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Pagination.Page.Should().Be(1);

        result2.Should().NotBeNull();
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Pagination.Page.Should().Be(2);
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
        createdField.IsSuccess.Should().BeTrue();
        createdField.Value.Type.Should().Be(CustomFieldType.MultiSelect);
        createdField.Value.Options.Should().HaveCount(2);
        createdField.Value.Options.Should().Contain(o => o.Value == "tag_a");
        createdField.Value.Options.Should().Contain(o => o.Value == "tag_b");

        // Cleanup
        var deleteResult = await client.CustomFields.DeleteAsync(createdField.Value.Id);
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
        createdField.IsSuccess.Should().BeTrue();
        createdField.Value.Id.Should().NotBeNullOrEmpty();
        createdField.Value.Name.Should().Be(fieldName);
        createdField.Value.Type.Should().Be(CustomFieldType.Text);
        createdField.Value.Description.Should().BeNull();
        createdField.Value.IsRequired.Should().BeFalse();
        createdField.Value.Metadata.Should().BeNull();

        // Cleanup
        var deleteResult = await client.CustomFields.DeleteAsync(createdField.Value.Id);
    }
}