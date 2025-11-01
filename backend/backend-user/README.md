# User Service Backend

A comprehensive user management service built with ASP.NET Core 9.0, following SOLID principles and design patterns.

## Architecture

This service implements a clean, layered architecture with:

### Folder Structure

```
backend-user/
├── Common/                 # Shared utilities and base classes
│   ├── Enums/             # Enumeration types (OAuthProviderType)
│   ├── BaseEntity.cs      # Base entity class for common properties
│   └── Result.cs          # Result pattern for operation outcomes
├── Model/                 # Database models
│   ├── User.cs
│   ├── OAuthProvider.cs
│   └── UserAnalytics.cs
├── DTOs/                  # Data Transfer Objects
│   ├── Request/           # Request DTOs (Create, Update)
│   └── Response/          # Response DTOs (without IDs for security)
├── Data/                  # Database context and configurations
│   ├── ApplicationDbContext.cs
│   └── Configurations/
├── Repositories/          # Repository pattern implementation
│   ├── Interfaces/
│   │   ├── IRepository.cs          # Generic repository
│   │   ├── IUserRepository.cs
│   │   ├── IOAuthProviderRepository.cs
│   │   ├── IUserAnalyticsRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Repository.cs               # Generic implementation
│   ├── UserRepository.cs
│   ├── OAuthProviderRepository.cs
│   ├── UserAnalyticsRepository.cs
│   └── UnitOfWork.cs
├── Services/              # Business logic layer
│   ├── Interfaces/
│   │   ├── IUserService.cs
│   │   ├── IOAuthProviderService.cs
│   │   └── IUserAnalyticsService.cs
│   ├── UserService.cs
│   ├── OAuthProviderService.cs
│   └── UserAnalyticsService.cs
├── Controllers/           # API Controllers
│   ├── UsersController.cs
│   ├── OAuthProvidersController.cs
│   └── UserAnalyticsController.cs
├── Mappings/             # AutoMapper profiles
│   └── MappingProfile.cs
└── Config/               # Dependency injection configurations
    ├── DatabaseConfiguration.cs
    ├── RepositoryConfiguration.cs
    ├── ServiceConfiguration.cs
    ├── AutoMapperConfiguration.cs
    ├── SwaggerConfiguration.cs
    ├── CorsConfiguration.cs
    └── HealthCheckConfiguration.cs
```

## Design Patterns Implemented

1. **Repository Pattern**: Generic and specific repositories for data access
2. **Unit of Work Pattern**: Coordinated operations across multiple repositories
3. **Result Pattern**: Standardized operation outcomes
4. **Dependency Injection**: Configured through dedicated configuration classes
5. **Service Layer Pattern**: Business logic separated from controllers
6. **DTO Pattern**: Clear separation between models and API contracts with Request/Response segregation
7. **Configuration Objects**: Modular DI setup in Config folder

## Security Features

- **No ID Exposure**: Response DTOs do not include database IDs to prevent enumeration attacks
- **Request/Response Separation**: Clear distinction between incoming and outgoing data structures
- **Model Isolation**: Database models are never directly exposed through the API

## SOLID Principles

- **Single Responsibility**: Each class has a single, well-defined purpose
- **Open/Closed**: Extensible through interfaces and base classes
- **Liskov Substitution**: Interfaces and base classes can be substituted
- **Interface Segregation**: Focused interfaces for specific needs
- **Dependency Inversion**: Dependencies on abstractions, not concretions

## Database Schema

### Users Table
- User accounts with profile information
- Support for active/inactive states
- Admin role management
- Last login tracking

### OAuth Providers Table
- Multiple OAuth providers per user
- Support for Google, GitHub, LinkedIn, Facebook
- Token management (access, refresh, expiration)

### User Analytics Table
- User activity tracking
- Session-based analytics
- Event tracking with JSON metadata
- IP address and user agent logging

## API Endpoints

### Users
- `GET /api/users` - Get all users
- `GET /api/users/active` - Get active users
- `GET /api/users/admins` - Get admin users
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/by-email/{email}` - Get user by email
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `POST /api/users/{id}/last-login` - Update last login

### OAuth Providers
- `GET /api/oauthproviders/{id}` - Get provider by ID
- `GET /api/oauthproviders/user/{userId}` - Get providers for user
- `GET /api/oauthproviders/provider/{provider}/{providerId}` - Get by provider details
- `POST /api/oauthproviders` - Create provider
- `PUT /api/oauthproviders/{id}` - Update provider
- `DELETE /api/oauthproviders/{id}` - Delete provider

### User Analytics
- `GET /api/useranalytics/{id}` - Get analytics by ID
- `GET /api/useranalytics/user/{userId}` - Get analytics for user
- `GET /api/useranalytics/session/{sessionId}` - Get analytics for session
- `GET /api/useranalytics/event/{eventType}` - Get analytics by event type
- `GET /api/useranalytics/date-range?startDate={start}&endDate={end}` - Get by date range
- `POST /api/useranalytics` - Create analytics event
- `DELETE /api/useranalytics/{id}` - Delete analytics

## Configuration

### Connection String
Update `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=users_service;Username=postgres;Password=postgres"
  }
}
```

### CORS
Configure allowed origins in `appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

## Running the Service

1. **Restore packages:**
   ```bash
   dotnet restore
   ```

2. **Apply migrations:**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

3. **Run the service:**
   ```bash
   dotnet run
   ```

4. **Access Swagger UI:**
   Navigate to `http://localhost:5000` (or your configured port)

## Health Checks

The service includes health checks accessible at:
- `/health` - Overall health status including database connectivity

## Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL with Entity Framework Core
- **ORM**: Entity Framework Core 9.0 with Npgsql
- **Mapping**: AutoMapper 12.0
- **Validation**: FluentValidation 11.3
- **API Documentation**: Swagger/OpenAPI
- **Authentication**: JWT Bearer (configured, ready for implementation)

## Development

### Adding a New Model

1. Create model in `Model/`
2. Create Request DTOs in `DTOs/Request/`
3. Create Response DTOs in `DTOs/Response/` (without ID for security)
4. Create entity configuration in `Data/Configurations/`
5. Add DbSet to `ApplicationDbContext`
6. Create repository interface and implementation
7. Create service interface and implementation
8. Add AutoMapper mappings (Model <-> Request/Response)
9. Create controller
10. Register in appropriate Config classes

**Important**: Response DTOs should NOT include IDs for security reasons. IDs are only used internally and in route parameters.

### Running Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Rollback
dotnet ef database update PreviousMigrationName
```

## Testing

The service is designed to be easily testable:
- Repository pattern allows easy mocking
- Service layer can be tested independently
- Controllers have minimal logic

## License

MIT

