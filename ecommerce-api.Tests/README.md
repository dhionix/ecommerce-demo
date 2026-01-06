# ecommerce-api.Tests

Comprehensive test suite for the ecommerce-api authentication functionality.

## Overview

This test project contains unit tests and integration tests for the authentication system, specifically focusing on the login functionality validation.

## Test Structure

### Unit Tests (`UnitTests/`)

#### AuthenticationServiceTests
Tests for the `AuthenticationService.ValidateCredentials` method:

- ✅ **ValidateCredentials_ValidCredentials_ReturnsSuccessWithUser** - Verifies successful login with valid credentials
- ✅ **ValidateCredentials_NonExistentUsername_ReturnsFailure** - Tests handling of non-existent username
- ✅ **ValidateCredentials_IncorrectPassword_ReturnsFailure** - Tests handling of incorrect password
- ✅ **ValidateCredentials_InactiveAccount_ReturnsFailure** - Tests handling of inactive user accounts
- ✅ **ValidateCredentials_SuccessfulLogin_UpdatesLastLoginTime** - Verifies last login time is updated on successful login
- ✅ **ValidateCredentials_CaseInsensitiveUsername_ReturnsSuccess** - Tests case-insensitive username matching (lowercase)
- ✅ **ValidateCredentials_CaseInsensitiveUsername_UpperCase_ReturnsSuccess** - Tests case-insensitive username matching (uppercase)
- ✅ **ValidateCredentials_ReturnsProperErrorMessages** - Verifies appropriate error messages are returned

### Integration Tests (`IntegrationTests/`)

#### AuthControllerLoginTests
Tests for the `/api/auth/login` endpoint:

- ✅ **Login_ValidCredentials_Returns200OKWithToken** - Tests successful login returns 200 OK with token
- ✅ **Login_ValidCredentials_ResponseContainsJwtToken** - Verifies response contains a valid JWT token structure
- ✅ **Login_ValidCredentials_ResponseContainsUsernameAndEmail** - Verifies response includes username and email
- ✅ **Login_ValidCredentials_ResponseContainsExpirationTime** - Verifies response includes token expiration time
- ✅ **Login_InvalidUsername_Returns401Unauthorized** - Tests invalid username returns 401
- ✅ **Login_InvalidPassword_Returns401Unauthorized** - Tests invalid password returns 401
- ✅ **Login_MissingUsername_Returns400BadRequest** - Tests missing username returns 400
- ✅ **Login_MissingPassword_Returns400BadRequest** - Tests missing password returns 400
- ✅ **Login_MissingAllFields_Returns400BadRequest** - Tests missing all fields returns 400
- ✅ **Login_JwtTokenCanAccessProtectedEndpoint** - Verifies JWT token can access protected endpoints
- ✅ **Login_InactiveAccount_Returns401Unauthorized** - Documents expected behavior for inactive accounts
- ✅ **Login_WithoutAuthorization_CannotAccessProtectedEndpoint** - Tests protected endpoints require authorization
- ✅ **Login_CaseInsensitiveUsername_Returns200OK** - Tests case-insensitive username handling in API

## Technologies Used

- **xUnit** - Testing framework
- **FluentAssertions** - Assertion library for more readable tests
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing framework
- **Moq** - Mocking framework (available for future use)

## Running the Tests

### Run all tests
```bash
dotnet test
```

### Run with detailed output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run only unit tests
```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
```

### Run only integration tests
```bash
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

## Test Coverage

The test suite covers:
- ✅ Successful login scenarios
- ✅ Failed login scenarios (invalid username, invalid password, inactive account)
- ✅ Input validation (missing fields)
- ✅ JWT token generation and structure
- ✅ JWT token usage for protected endpoints
- ✅ Case-insensitive username matching
- ✅ Last login time tracking
- ✅ Proper error message responses

## Acceptance Criteria Compliance

All tests align with the acceptance criteria defined in `/tests/auth-acceptance.md`:
- Successfully login with valid credentials (200 OK)
- Fail to login with invalid username (401 Unauthorized)
- Fail to login with invalid password (401 Unauthorized)
- Fail to login with inactive account (401 Unauthorized)
- User's last login time should be updated

## Notes

- All tests are independent and can run in parallel
- Tests follow the naming convention: `MethodName_StateUnderTest_ExpectedBehavior`
- Integration tests use `CustomWebApplicationFactory` for proper test isolation
- The test project uses .NET 8 and follows best practices for test organization
