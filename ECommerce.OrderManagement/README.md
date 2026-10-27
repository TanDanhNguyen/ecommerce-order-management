# ECommerce Order Management API

A RESTful Web API built with ASP.NET Core and Clean Architecture to manage products and orders. This project demonstrates backend development practices, authentication, transactional consistency, idempotency, and automated testing.

## Tech Stack

- C# and .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT Authentication and Role-Based Authorization
- xUnit
- Swagger / OpenAPI
- Serilog

## Architecture

The solution follows Clean Architecture principles to separate business rules, application use cases, infrastructure concerns, and API endpoints.

- **Domain** — Core business entities, rules, and domain logic.
- **Application** — Use cases, application services, DTOs, and repository abstractions.
- **Infrastructure** — Database access, repository implementations, Identity, and background services.
- **API** — HTTP endpoints, authentication, authorization, exception handling, and middleware.
- **UnitTests** — Unit tests for application and business logic.
- **IntegrationTests** — Integration tests for API behavior and database operations.

## Features

### Product Management
- Create, retrieve, update, and list products.
- Validate product data and prevent duplicate SKUs.
- Support paginated product retrieval.

### Order Management
- Create and cancel orders.
- Validate product availability and stock quantities.
- Update inventory consistently when orders are created or cancelled.
- Use database transactions to protect related changes.
- Handle concurrency conflicts.

### Reliability and API Security
- JWT-based authentication.
- Role-based authorization.
- ASP.NET Core Identity for user management.
- Idempotency support for safe retries of supported requests.
- Background cleanup of expired idempotency records.
- Global exception handling with appropriate HTTP status codes.
- Correlation IDs and structured request logging.
- Rate limiting and health checks.

## Prerequisites

- .NET 8 SDK
- SQL Server
- Visual Studio 2022 or another compatible .NET IDE

## Configuration

Configure the connection strings and JWT settings in the appropriate configuration file or through environment variables.

Required settings include:

- Business database connection string
- Identity database connection string
- JWT issuer
- JWT audience
- JWT signing key

Do not commit real database credentials, production secrets, or signing keys to source control.

## Getting Started

1. Clone the repository.
2. Open `ECommerce.OrderManagement.sln` in Visual Studio.
3. Configure the required connection strings and JWT settings.
4. Apply the Entity Framework Core migrations to the required databases.
5. Set `ECommerce.OrderManagement.API` as the startup project.
6. Run the API.
7. Open the Swagger UI URL shown by the application to explore the endpoints.

Ensure the required databases and migrations are configured before starting the API.

## Running Tests

Run all tests from the solution directory:

```bash
dotnet test
```

To build the solution:

```bash
dotnet build
```

## Project Goals

This project serves as a practical portfolio demonstrating:

- Clean Architecture and separation of concerns
- Dependency Injection
- REST API design and validation
- Entity Framework Core and SQL Server
- Authentication and authorization
- Transactional data consistency and concurrency handling
- Idempotent request processing
- Unit and integration testing
- Logging, error handling, and operational reliability

## Future Improvements

Potential extensions include Docker support, CI/CD automation, API versioning, and additional observability features.