You are an expert C# developer specializing in building performant, modern .NET libraries. Your task is to create a complete, open-source-ready NuGet package-style library for a highly efficient and easy-to-use REST API client targeting the Polar.sh payments platform. This client will interact with Polar's Core API (server-side operations like managing products, orders, subscriptions, checkouts, benefits, customer sessions, and license keys) using the official API documentation at https://polar.sh/docs/api-reference/.
Core Requirements

use IHttpClientFactory, All methods must be asynchronous.
Set default headers (e.g., User-Agent, Accept: application/json).
Handle retries for rate limits (429 responses) using exponential backoff via Polly.

Serialization: Use System.Text.Json with JsonSerializerOptions configured for:

CamelCase naming policy.
PropertyNameCaseInsensitive.
Support for polymorphic types if needed (e.g., via JsonDerivedType attributes).
Efficient deserialization with JsonSerializer.DeserializeAsync for streams.


Efficiency:

Minimize allocations: Use ValueTask for fire-and-forget or low-contention ops; stream large responses if paginated.
Support cancellation tokens everywhere.
Batch operations where API allows (e.g., bulk creates if documented).
Rate limiting: Track requests per minute (300/min default) with a semaphore or token bucket; expose config for custom limits.


Ease of Use:

Fluent API builder pattern for the client initialization and complex requests (e.g., new PolarClient().WithToken("...").WithBaseUrl(...).Products.List().WithFilter(x => x.Name == "Pro").ExecuteAsync()).
Strongly typed methods for each major endpoint category (e.g., Products.CreateAsync(ProductCreateRequest req)).
Automatic pagination helpers: Methods like ListAllAsync() that yield or collect all pages via IAsyncEnumerable<T>.
Extension methods for common patterns (e.g., .WithRetryAsync()).
Minimal boilerplate: No manual JSON handling; all in/out bound to records.
Validation: Use System.ComponentModel.DataAnnotations attributes on DTOs (e.g., [Required], [Range]) and integrate with FluentValidation if it adds value without bloat.


Authentication:

Support Bearer tokens (Organization Access Token - OAT) via header: Authorization: Bearer {token}.
Configurable via constructor or fluent setter.
Separate support for Customer Access Tokens for Customer Portal API if differentiated.


Error Handling:

Custom PolarApiException (record) for API errors, including status code, error message, and response body.
Use HttpRequestException for network issues.
Pattern-match on response status in a central handler.

Full API Coverage for httphttps://polar.sh/docs/api-reference/introductions://polar.sh/docs/api-reference/introduction


Ensure the code is production-ready, tested in spirit, add integration test to be tested against polar's sandbox environment, and follows .NET best practices (e.g., XML docs, nullable reference types enabled).

All code must have end-to-end integration tests covering all functionality against Polar's sandbox environment.

---

First analyze the project and create a progress report outlining the steps needed to complete the task.
The progress must be reported between 0% to 100%.

---

Now follow the progress report and complete the task step by step.