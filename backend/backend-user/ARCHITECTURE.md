# Architecture Documentation

## Overview

The User Service backend follows Clean Architecture principles with a clear separation of concerns and dependency inversion.

## Layers

### 1. API Layer (Controllers)

**Location**: `Controllers/`

**Responsibility**: HTTP endpoints and request/response handling

**Key Features**:
- Minimal logic - delegates to service layer
- Input validation using Data Annotations
- Proper HTTP status codes
- RESTful API design

**Example**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
}
```

### 2. Service Layer

**Location**: `Services/` and `Services/Interfaces/`

**Responsibility**: Business logic and orchestration

**Key Features**:
- Business rule validation
- Transaction coordination
- Error handling and logging
- Result pattern for operation outcomes

**Example**:
```csharp
public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken);
}
```

### 3. Repository Layer

**Location**: `Repositories/` and `Repositories/Interfaces/`

**Responsibility**: Data access abstraction

**Key Features**:
- Generic repository for common operations
- Specific repositories for entity-specific queries
- Unit of Work for transaction management
- Query optimization

**Example**:
```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
}
```

### 4. Data Layer

**Location**: `Data/` and `Data/Configurations/`

**Responsibility**: Database context and entity configurations

**Key Features**:
- Entity Framework DbContext
- Fluent API entity configurations
- Database schema mapping
- Migration support

**Example**:
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        // ... configuration
    }
}
```

### DTO Organization

**Request DTOs** (`DTOs/Request/`):
- `CreateUserRequest` - For creating new users
- `UpdateUserRequest` - For updating existing users
- Include validation attributes
- Accept IDs in body when needed (e.g., UserId in CreateOAuthProviderRequest)

**Response DTOs** (`DTOs/Response/`):
- `UserResponse` - User data without ID
- Security-focused: No database IDs exposed
- Include all safe-to-display fields
- Used in all GET endpoints

### 5. Domain Layer

**Location**: `Model/`, `DTOs/`, `Common/`

**Responsibility**: Domain models and shared utilities

**Key Features**:
- Models matching database schema (in `Model/` folder)
- Request DTOs for incoming data (in `DTOs/Request/`)
- Response DTOs for outgoing data without IDs (in `DTOs/Response/`)
- Enums and value objects
- Common utilities (Result pattern, base entities)

**Security Design**:
- Response DTOs deliberately exclude IDs to prevent:
  - Enumeration attacks
  - Unauthorized resource access
  - Information disclosure
- IDs are only used internally and in route parameters for authorized operations

## Design Patterns

### 1. Repository Pattern

**Purpose**: Abstraction over data access

**Implementation**:
- Generic `IRepository<T>` interface
- Specific repositories (e.g., `IUserRepository`)
- Concrete implementations with EF Core

**Benefits**:
- Testability (easy mocking)
- Centralized data access logic
- Flexibility to change data source

### 2. Unit of Work Pattern

**Purpose**: Coordinate multiple repository operations

**Implementation**:
```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOAuthProviderRepository OAuthProviders { get; }
    IUserAnalyticsRepository UserAnalytics { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
```

**Benefits**:
- Transaction management
- Consistent data operations
- Single point of database commit

### 3. Result Pattern

**Purpose**: Standardized operation outcomes

**Implementation**:
```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    
    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

**Benefits**:
- Clear success/failure handling
- Type-safe error handling
- Functional programming approach

### 4. Service Layer Pattern

**Purpose**: Separate business logic from presentation

**Benefits**:
- Reusable business logic
- Easier testing
- Clear separation of concerns

### 5. Dependency Injection

**Purpose**: Loose coupling and testability

**Implementation**: Configuration classes in `Config/` folder

**Benefits**:
- Easy to test (mock dependencies)
- Flexible configuration
- Follows SOLID principles

## SOLID Principles Application

### Single Responsibility Principle (SRP)

Each class has one reason to change:
- Controllers: Handle HTTP
- Services: Business logic
- Repositories: Data access
- Configurations: DI setup

### Open/Closed Principle (OCP)

Open for extension, closed for modification:
- Generic repository can be extended
- New services can be added without modifying existing ones
- Configuration classes can be extended

### Liskov Substitution Principle (LSP)

Implementations can replace their interfaces:
- All repository implementations follow `IRepository<T>`
- All services follow their interface contracts
- Unit of Work coordinates all repositories

### Interface Segregation Principle (ISP)

Clients don't depend on unused methods:
- Focused interfaces (e.g., `IUserRepository` vs `IRepository<T>`)
- Specific service interfaces
- Configuration interfaces

### Dependency Inversion Principle (DIP)

Depend on abstractions, not concretions:
- Controllers depend on `IUserService`, not `UserService`
- Services depend on `IUnitOfWork`, not `UnitOfWork`
- All dependencies injected through interfaces

## Data Flow

```
HTTP Request
    ↓
Controller (API Layer)
    ↓
Service (Business Logic)
    ↓
Repository (Data Access)
    ↓
DbContext (Entity Framework)
    ↓
Database (PostgreSQL)
```

## Error Handling

### Controller Level
- Model validation
- HTTP status codes
- Exception handling middleware

### Service Level
- Business rule validation
- Result pattern for outcomes
- Logging

### Repository Level
- Database exceptions
- Query optimization
- Transaction management

## Testing Strategy

### Unit Tests
- Services (mock repositories)
- Repository methods (in-memory database)
- Mappings (AutoMapper)

### Integration Tests
- Controllers (TestServer)
- Database operations (test database)
- End-to-end API flows

### Example Test Structure
```csharp
public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UserService _userService;
    
    [Fact]
    public async Task CreateAsync_ValidUser_ReturnsSuccess()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

## Configuration Management

### Modular Configuration
All configuration organized in `Config/` folder:
- `DatabaseConfiguration.cs`
- `RepositoryConfiguration.cs`
- `ServiceConfiguration.cs`
- `AutoMapperConfiguration.cs`
- `SwaggerConfiguration.cs`
- `CorsConfiguration.cs`
- `HealthCheckConfiguration.cs`

### Program.cs Organization
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add configurations using extension methods
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddRepositoryConfiguration();
builder.Services.AddServiceConfiguration();
// ... more configurations

var app = builder.Build();

// Configure middleware pipeline
app.UseSwaggerConfiguration();
app.UseCorsConfiguration();
// ... more middleware
```

## Performance Considerations

1. **Async/Await**: All I/O operations are async
2. **IQueryable**: Deferred execution for complex queries
3. **Indexes**: Proper database indexes on frequently queried columns
4. **Connection Pooling**: Configured in DbContext options
5. **Cancellation Tokens**: Support for request cancellation

## Security Considerations

1. **Input Validation**: Data annotations and FluentValidation
2. **SQL Injection**: Parameterized queries through EF Core
3. **CORS**: Configurable allowed origins
4. **Authentication**: JWT Bearer ready (configured)
5. **Sensitive Data**: Logging configured to not expose sensitive info

## Scalability

1. **Stateless Design**: No server-side session state
2. **Database Connection Pooling**: Efficient resource usage
3. **Async Operations**: Non-blocking I/O
4. **Repository Pattern**: Easy to add caching layer
5. **Horizontal Scaling**: Service can run multiple instances

