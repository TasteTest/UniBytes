# UniBytes Testing Guide 🧪
<!-- Ctrl + Shit + v for preview with Markdown All in One -->
**Last Updated:** November 13, 2025  
**Author:** Elisa :)

---

## 📚 Quick Links

- [Overview](#overview)
- [Initial Setup](#initial-setup-one-time)
- [Creating Tests for New Microservice](#creating-tests-for-a-new-microservice)
- [Running Tests](#running-tests)
- [Code Coverage](#code-coverage-reports)
- [Quick Reference](#quick-reference-commands)

---

## Overview

UniBytes uses **centralized testing** with all test projects in the `Tests/` directory.

**Stack:**
- xUnit (testing framework)
- NSubstitute (mocking)
- FluentAssertions (readable assertions)
- InMemory Database (repository tests)
- `UniBytes.sln` (run all tests at once)

**Project Structure:**
```
UniBytes/
├── UniBytes.sln              ← Solution file
├── backend/
│   ├── backend-menu/
│   ├── backend-payment/
│   └── ...
└── Tests/
    ├── Menu.Tests/           ← Reference implementation
    ├── Payment.Tests/        ← Create following same pattern
    └── ...
```

---

## Initial Setup (One-Time)

### Install Global Tools

```cmd
dotnet tool install -g dotnet-reportgenerator-globaltool
```

That's it! ✅

---

## Creating Tests for a New Microservice

### Step 1: Create Test Project

Replace `Payment` with your service name (User, Order, Kitchen, Loyalty):

```cmd
cd C:\Users\Elisa\Desktop\UniBytes\Tests

# Create test project
dotnet new xunit -n Payment.Tests
cd Payment.Tests

# Add reference to microservice
dotnet add reference ../../backend/backend-payment/backend-payment.csproj

# Add required packages
dotnet add package NSubstitute --version 5.3.0
dotnet add package FluentAssertions --version 8.8.0
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 9.0.0
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 9.0.0

# Add to solution
cd C:\Users\Elisa\Desktop\UniBytes
dotnet sln add Tests/Payment.Tests/Payment.Tests.csproj

# Clean up
cd Tests/Payment.Tests
del UnitTest1.cs
```

### Step 2: Create Folder Structure

```
Payment.Tests/
├── Services/           ← Unit tests for business logic
├── Repositories/       ← Integration tests with InMemory DB
├── Controllers/        ← API endpoint tests
└── Payment.Tests.csproj
```

### Step 3: Write Tests

**📖 See Reference Implementation:**
- Service tests: `Tests/Menu.Tests/Services/MenuServiceTests.cs`
- Repository tests: `Tests/Menu.Tests/Repositories/CategoryRepositoryTests.cs`
- Controller tests: `Tests/Menu.Tests/Controllers/MenuItemsControllerTests.cs`

**Copy the structure, replace class names, and adjust to your service logic.**

### Test Naming Convention

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Setup
    
    // Act - Execute
    
    // Assert - Verify
}
```

**Examples:**
- `CreatePayment_WithValidData_ReturnsSuccess`
- `GetPayment_WhenNotFound_ReturnsFailure`
- `ProcessRefund_WithInsufficientFunds_ThrowsException`

---

## Running Tests

### ✅ Run All Tests (Recommended)

```cmd
cd C:\Users\Elisa\Desktop\UniBytes
dotnet test
```

### Run Specific Test Project

```cmd
cd Tests/Menu.Tests
dotnet test
```

### Run Specific Test

```cmd
dotnet test --filter "FullyQualifiedName~MenuServiceTests"
```

### Detailed Output

```cmd
dotnet test --logger "console;verbosity=detailed"
```

---

## Code Coverage Reports

### Generate Coverage for One Service

```cmd
cd Tests/Menu.Tests
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
start coveragereport\index.html
```

### Generate Coverage for All Services

```cmd
cd C:\Users\Elisa\Desktop\UniBytes
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"Tests/**/TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
start coveragereport\index.html
```

### Coverage Report Legend

- 🟢 **Green** = Code is tested
- 🔴 **Red** = Code is NOT tested
- 🟡 **Yellow** = Partially tested (some branches)

**Target: 80%+ coverage**

---

## Best Practices

### ✅ DO

1. **Follow AAA Pattern** (Arrange, Act, Assert)
2. **Use descriptive test names** (see naming convention above)
3. **Test edge cases** (null values, empty lists, invalid IDs)
4. **Isolate tests** (use InMemory DB with unique name per test)
5. **Mock external dependencies** (databases, APIs, file systems)

### ❌ DON'T

1. **Don't test framework code** (focus on YOUR logic)
2. **Don't use real databases** (use InMemory or mocks)
3. **Don't share state between tests**
4. **Don't ignore failing tests** (fix or remove)

---

## Troubleshooting

### JsonDocument InMemory Database Error

**Error:** `JsonDocument property could not be mapped`

**Solution:** See `backend/backend-menu/Data/MenuDbContext.cs` lines 24-33 for the fix pattern.

### Tests Can't Find Dependencies

```cmd
dotnet restore
dotnet build
```

### Coverage Report Not Generating

```cmd
# Verify tests ran
dir TestResults

# If empty, run tests again
dotnet test --collect:"XPlat Code Coverage"

# Check tool is installed
reportgenerator --version
```

---

## Quick Reference Commands

```cmd
# ========================================
# CREATE NEW TEST PROJECT
# ========================================
cd Tests
dotnet new xunit -n ServiceName.Tests
cd ServiceName.Tests
dotnet add reference ../../backend/backend-servicename/backend-servicename.csproj
dotnet add package NSubstitute FluentAssertions Microsoft.EntityFrameworkCore.InMemory Microsoft.AspNetCore.Mvc.Testing
cd ../..
dotnet sln add Tests/ServiceName.Tests/ServiceName.Tests.csproj

# ========================================
# RUN TESTS
# ========================================
dotnet test                                    # All tests
dotnet test Tests/Menu.Tests                   # Specific project
dotnet test --filter "ClassName"               # Specific class

# ========================================
# CODE COVERAGE
# ========================================
cd Tests/Menu.Tests
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
start coveragereport\index.html
```

---

## Team Checklist

Create tests for remaining microservices:

- [x] Menu Service → `Tests/Menu.Tests/` ✅ (Reference implementation)
- [ ] Payment Service → `Tests/Payment.Tests/`
- [ ] User Service → `Tests/User.Tests/`
- [ ] Order Service → `Tests/Order.Tests/`
- [ ] Kitchen Service → `Tests/Kitchen.Tests/`
- [ ] Loyalty Service → `Tests/Loyalty.Tests/`

For each service, test:
- ✅ Services (business logic)
- ✅ Repositories (database operations)
- ✅ Controllers (API endpoints)

---

## Resources

- **Reference Implementation:** `Tests/Menu.Tests/`
- **xUnit Docs:** https://xunit.net/
- **FluentAssertions:** https://fluentassertions.com/
- **NSubstitute:** https://nsubstitute.github.io/

---

## Need Help?

- Check `Tests/Menu.Tests/` for complete examples
- Ask in #dev-testing Slack channel
- Review this guide's [Troubleshooting](#troubleshooting) section

**Happy Testing! 🎉**
