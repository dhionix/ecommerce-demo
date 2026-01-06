# Authentication Guide

This guide provides comprehensive information about using the authentication system in the E-commerce API.

## Table of Contents

- [Overview](#overview)
- [Getting Started](#getting-started)
- [Registration](#registration)
- [Login](#login)
- [Using JWT Tokens](#using-jwt-tokens)
- [Password Management](#password-management)
- [Protected Endpoints](#protected-endpoints)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)
- [Examples](#examples)

## Overview

The E-commerce API uses JWT (JSON Web Token) based authentication. This provides:

- **Stateless authentication**: No session storage required on the server
- **Scalability**: Easily scale horizontally without session synchronization
- **Security**: Tokens are cryptographically signed and have expiration times
- **Flexibility**: Tokens can be used across different services and platforms

### Authentication Flow

```
┌─────────┐                                    ┌─────────┐
│  Client │                                    │   API   │
└────┬────┘                                    └────┬────┘
     │                                              │
     │  POST /api/auth/register or login            │
     │  (username, password)                        │
     ├─────────────────────────────────────────────>│
     │                                              │
     │                 JWT Token                    │
     │<─────────────────────────────────────────────┤
     │                                              │
     │  Request with Authorization: Bearer {token}  │
     ├─────────────────────────────────────────────>│
     │                                              │
     │                 Response                     │
     │<─────────────────────────────────────────────┤
     │                                              │
```

## Getting Started

### Prerequisites

- .NET 8 SDK installed
- API server running (default: http://localhost:5000)
- An HTTP client (curl, Postman, or similar)

### Starting the API

```bash
cd ecommerce-api
dotnet run
```

The API will be available at `http://localhost:5000`.

## Registration

### Endpoint

```
POST /api/auth/register
```

### Request Body

```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecureP@ss123"
}
```

### Password Requirements

Your password must meet the following criteria:

- **Minimum length**: 8 characters
- **Uppercase letter**: At least one (A-Z)
- **Lowercase letter**: At least one (a-z)
- **Digit**: At least one (0-9)
- **Special character**: At least one (@$!%*?&)

### Response

**Success (201 Created):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "john_doe",
  "email": "john@example.com",
  "expiresAt": "2025-11-18T12:00:00Z"
}
```

**Error (400 Bad Request):**

```json
{
  "message": "Username already exists"
}
```

or

```json
{
  "errors": {
    "Password": [
      "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character"
    ]
  }
}
```

### Example with curl

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "email": "john@example.com",
    "password": "SecureP@ss123"
  }'
```

## Login

### Endpoint

```
POST /api/auth/login
```

### Request Body

```json
{
  "username": "john_doe",
  "password": "SecureP@ss123"
}
```

### Response

**Success (200 OK):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "john_doe",
  "email": "john@example.com",
  "expiresAt": "2025-11-18T12:00:00Z"
}
```

**Error (401 Unauthorized):**

```json
{
  "message": "Invalid username or password"
}
```

### Example with curl

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "john_doe",
    "password": "SecureP@ss123"
  }'
```

## Using JWT Tokens

### Token Structure

A JWT token consists of three parts separated by dots:

```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.  <- Header
eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWU...  <- Payload
bkc_hGWir7EsKkOkfACR7gY7BOGv2oSWimD...  <- Signature
```

### Token Claims

The token includes the following claims:

- **nameid**: User ID
- **unique_name**: Username
- **email**: User email
- **jti**: Unique token identifier
- **iss**: Token issuer
- **aud**: Token audience
- **exp**: Expiration timestamp
- **nbf**: Not before timestamp
- **iat**: Issued at timestamp

### Token Expiration

- **Production**: 60 minutes
- **Development**: 120 minutes

Tokens are automatically validated for expiration. Expired tokens will be rejected with a 401 Unauthorized response.

### Adding Token to Requests

Include the token in the `Authorization` header using the Bearer scheme:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Example with curl

```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "New Product",
    "description": "Product description",
    "price": 29.99
  }'
```

## Password Management

### Change Password

#### Endpoint

```
POST /api/auth/change-password
```

**Authentication Required**: Yes

#### Request Body

```json
{
  "currentPassword": "SecureP@ss123",
  "newPassword": "NewSecureP@ss456"
}
```

#### Response

**Success (200 OK):**

```json
{
  "message": "Password changed successfully"
}
```

**Error (400 Bad Request):**

```json
{
  "message": "Current password is incorrect"
}
```

#### Example with curl

```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X POST http://localhost:5000/api/auth/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "currentPassword": "SecureP@ss123",
    "newPassword": "NewSecureP@ss456"
  }'
```

### Password Reset

Password reset functionality (forgot password) is planned for a future release. Currently, passwords can only be changed by authenticated users.

## Protected Endpoints

The following endpoints require authentication:

### Products

- `POST /api/products` - Create a new product
- `POST /api/products/bulk` - Bulk create products
- `PUT /api/products/{id}` - Update a product
- `DELETE /api/products/{id}` - Delete a product

### Public Endpoints

These endpoints do NOT require authentication:

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get a specific product
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login

## Security Best Practices

### For Users

1. **Strong Passwords**: Always use strong passwords that meet all requirements
2. **Keep Tokens Safe**: Never share your JWT token or commit it to version control
3. **Token Expiration**: Be aware that tokens expire and you'll need to re-authenticate
4. **HTTPS in Production**: Always use HTTPS in production environments
5. **Secure Storage**: Store tokens securely in your application (not in localStorage for web apps)

### For Developers

1. **Environment Variables**: Use environment variables for sensitive configuration in production
2. **Secret Rotation**: Rotate JWT secrets regularly in production
3. **HTTPS Only**: Enforce HTTPS in production environments
4. **Rate Limiting**: Implement rate limiting to prevent brute force attacks
5. **Logging**: Log authentication failures for security monitoring
6. **Token Validation**: Always validate tokens on the server side
7. **Input Validation**: Validate all user inputs on both client and server

### Configuration Security

The JWT secret should be:

- **Long**: At least 32 characters
- **Random**: Generated using cryptographically secure random number generators
- **Secret**: Never committed to source control
- **Different**: Use different secrets for development and production

#### Development Configuration

Located in `appsettings.Development.json`:

```json
{
  "Jwt": {
    "Secret": "DevelopmentSecretKeyForJWTTokenGeneration123456789",
    "Issuer": "ecommerce-api-dev",
    "Audience": "ecommerce-api-users-dev",
    "ExpirationMinutes": 120
  }
}
```

#### Production Configuration

For production, use environment variables or Azure Key Vault:

```bash
export JWT__SECRET="your-production-secret-key-minimum-32-chars"
export JWT__ISSUER="your-production-issuer"
export JWT__AUDIENCE="your-production-audience"
export JWT__EXPIRATIONMINUTES=60
```

## Troubleshooting

### Common Issues and Solutions

#### 1. 401 Unauthorized

**Problem**: Receiving 401 Unauthorized when accessing protected endpoints.

**Solutions**:
- Verify you included the `Authorization` header
- Check the token format: `Bearer {token}`
- Ensure the token hasn't expired
- Verify you're using the correct token
- Check if your account is active

#### 2. Token Expired

**Problem**: Token has expired.

**Solution**:
- Login again to get a new token
- Tokens expire after 60 minutes (production) or 120 minutes (development)

#### 3. Invalid Token

**Problem**: Token is invalid or malformed.

**Solutions**:
- Ensure you copied the entire token
- Verify no extra spaces or characters
- Check that the token wasn't modified
- Login again to get a fresh token

#### 4. Password Validation Failed

**Problem**: Password doesn't meet requirements.

**Solutions**:
- Ensure password is at least 8 characters
- Include at least one uppercase letter
- Include at least one lowercase letter
- Include at least one digit
- Include at least one special character (@$!%*?&)

#### 5. Username or Email Already Exists

**Problem**: Cannot register because username or email is taken.

**Solution**:
- Choose a different username
- Use a different email address
- If you already have an account, use the login endpoint instead

## Examples

### Complete Authentication Flow

#### 1. Register a New User

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "alice",
    "email": "alice@example.com",
    "password": "Alice@123"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIxIiwidW5pcXVlX25hbWUiOiJhbGljZSIsImVtYWlsIjoiYWxpY2VAZXhhbXBsZS5jb20iLCJqdGkiOiIxMjM0NTY3OCIsIm5iZiI6MTcwMDAwMDAwMCwiZXhwIjoxNzAwMDA3MjAwLCJpYXQiOjE3MDAwMDAwMDAsImlzcyI6ImVjb21tZXJjZS1hcGktZGV2IiwiYXVkIjoiZWNvbW1lcmNlLWFwaS11c2Vycy1kZXYifQ.signature",
  "username": "alice",
  "email": "alice@example.com",
  "expiresAt": "2025-11-18T14:00:00Z"
}
```

#### 2. Use Token to Create a Product

```bash
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Wireless Mouse",
    "description": "Ergonomic wireless mouse",
    "price": 29.99
  }'
```

**Response:**
```json
{
  "id": 10,
  "name": "Wireless Mouse",
  "description": "Ergonomic wireless mouse",
  "price": 29.99,
  "stock": 0
}
```

#### 3. Change Password

```bash
curl -X POST http://localhost:5000/api/auth/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "currentPassword": "Alice@123",
    "newPassword": "NewAlice@456"
  }'
```

**Response:**
```json
{
  "message": "Password changed successfully"
}
```

#### 4. Logout

```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Authorization: Bearer $TOKEN"
```

**Response:**
```json
{
  "message": "Logout successful. Please remove the token from your client."
}
```

### Using Swagger UI

1. **Open Swagger UI**: Navigate to `http://localhost:5000/swagger`

2. **Register or Login**:
   - Find the `/api/auth/register` or `/api/auth/login` endpoint
   - Click "Try it out"
   - Enter your credentials
   - Click "Execute"
   - Copy the `token` from the response

3. **Authorize Swagger**:
   - Click the "Authorize" button at the top of the page
   - In the "Value" field, enter: `Bearer {your-token}`
   - Click "Authorize"
   - Click "Close"

4. **Test Protected Endpoints**:
   - All protected endpoints will now include your token
   - You can test them directly from Swagger UI

### JavaScript/TypeScript Example

```typescript
// Register
const register = async (username: string, email: string, password: string) => {
  const response = await fetch('http://localhost:5000/api/auth/register', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ username, email, password }),
  });
  
  if (!response.ok) {
    throw new Error('Registration failed');
  }
  
  const data = await response.json();
  // Store token securely (e.g., in httpOnly cookie or secure storage)
  return data.token;
};

// Login
const login = async (username: string, password: string) => {
  const response = await fetch('http://localhost:5000/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ username, password }),
  });
  
  if (!response.ok) {
    throw new Error('Login failed');
  }
  
  const data = await response.json();
  return data.token;
};

// Use protected endpoint
const createProduct = async (token: string, product: any) => {
  const response = await fetch('http://localhost:5000/api/products', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify(product),
  });
  
  if (!response.ok) {
    throw new Error('Failed to create product');
  }
  
  return await response.json();
};
```

### Python Example

```python
import requests
import json

# Register
def register(username, email, password):
    url = 'http://localhost:5000/api/auth/register'
    data = {
        'username': username,
        'email': email,
        'password': password
    }
    response = requests.post(url, json=data)
    response.raise_for_status()
    return response.json()['token']

# Login
def login(username, password):
    url = 'http://localhost:5000/api/auth/login'
    data = {
        'username': username,
        'password': password
    }
    response = requests.post(url, json=data)
    response.raise_for_status()
    return response.json()['token']

# Use protected endpoint
def create_product(token, name, description, price):
    url = 'http://localhost:5000/api/products'
    headers = {
        'Authorization': f'Bearer {token}',
        'Content-Type': 'application/json'
    }
    data = {
        'name': name,
        'description': description,
        'price': price
    }
    response = requests.post(url, json=data, headers=headers)
    response.raise_for_status()
    return response.json()

# Example usage
token = register('bob', 'bob@example.com', 'Bob@12345')
product = create_product(token, 'Gaming Keyboard', 'RGB mechanical keyboard', 79.99)
print(f"Created product: {product['name']} with ID: {product['id']}")
```

## API Reference

### Authentication Endpoints

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/api/auth/register` | No | Register a new user |
| POST | `/api/auth/login` | No | Login and get JWT token |
| POST | `/api/auth/change-password` | Yes | Change user password |
| POST | `/api/auth/logout` | Yes | Logout (client-side token removal) |

### HTTP Status Codes

| Code | Meaning | When It Occurs |
|------|---------|----------------|
| 200 | OK | Successful request |
| 201 | Created | Resource created successfully |
| 400 | Bad Request | Invalid input or validation error |
| 401 | Unauthorized | Missing, invalid, or expired token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Server error |

## Additional Resources

- [JWT.io](https://jwt.io/) - JWT token decoder and information
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Microsoft Authentication Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)

## Support

For issues, questions, or contributions:

1. Check this guide and the troubleshooting section
2. Review the [User Stories and Acceptance Criteria](../docs/USER_STORIES_AND_ACCEPTANCE_CRITERIA.md)
3. Check the [Acceptance Tests](../tests/auth-acceptance.md)
4. Open an issue on the repository

---

**Last Updated**: 2025-11-18  
**Version**: 1.0
