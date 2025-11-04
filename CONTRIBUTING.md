# Contributing to Polar.NET

Thank you for your interest in contributing to Polar.NET! This document provides guidelines and information for contributors.

## Getting Started

### Prerequisites

- .NET 8.0 or .NET 9.0 SDK
- Visual Studio 2022, Visual Studio Code, or your preferred .NET IDE
- Git

### Setup

1. Fork the repository
2. Clone your fork locally:
   ```bash
   git clone https://github.com/yourusername/Polar.NET.git
   cd Polar.NET
   ```
3. Create a feature branch:
   ```bash
   git checkout -b feature/your-feature-name
   ```
4. Open the solution in your IDE and restore NuGet packages

## Development Workflow

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test tests/Polar.NET.Tests/

# Run only integration tests (requires Polar access token)
dotnet test tests/Polar.NET.IntegrationTests/
```

### Code Style

This project follows standard .NET coding conventions:

- Use PascalCase for public members
- Use camelCase for private members and parameters
- Include XML documentation comments for all public APIs
- Use meaningful variable and method names
- Keep methods and classes focused on single responsibilities

### Adding New Features

1. **API Endpoints**: When adding new API endpoints:
   - Create corresponding model classes in the appropriate `Models/` subdirectory
   - Add methods to the relevant API class in `Api/`
   - Include comprehensive XML documentation
   - Add query builders if the endpoint supports filtering
   - Add unit tests for the new functionality

2. **Models**: When creating new model classes:
   - Use record types for immutable data models
   - Include validation attributes where appropriate
   - Add XML documentation for all properties
   - Place models in the appropriate namespace under `Models/`

3. **Error Handling**: Follow the existing error handling patterns:
   - Use `PolarApiException` for API errors
   - Include proper error details and status codes
   - Handle HTTP errors appropriately

## Testing

### Unit Tests

- Unit tests are located in `tests/Polar.NET.Tests/`
- Test all public methods and edge cases
- Use descriptive test method names
- Mock external dependencies

### Integration Tests

- Integration tests are located in `tests/Polar.NET.IntegrationTests/`
- These tests require a Polar access token
- Set the `POLAR_ACCESS_TOKEN` environment variable or add to `appsettings.json`
- Tests should be idempotent and clean up after themselves

## Submitting Changes

1. Ensure all tests pass
2. Follow the coding style guidelines
3. Update documentation if needed
4. Commit your changes with a clear commit message
5. Push to your fork and create a pull request

### Pull Request Guidelines

- Use a clear title and description
- Reference any relevant issues
- Include screenshots if the change affects UI
- Ensure CI/CD pipeline passes
- Wait for code review before merging

## Bug Reports

When reporting bugs, please include:

- .NET version
- OS version
- Polar.NET version
- Steps to reproduce
- Expected vs actual behavior
- Any error messages or stack traces

## Feature Requests

Feature requests are welcome! Please:

- Check if the feature already exists
- Provide a clear use case
- Consider if it fits the project's scope
- Be open to discussion

## Questions

If you have questions:

- Check existing documentation
- Search existing issues and discussions
- Create a new issue with the "question" label

## License

By contributing to this project, you agree that your contributions will be licensed under the MIT License.