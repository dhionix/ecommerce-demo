# E-commerce API

This is a simple e-commerce API built using .NET 8. The API provides CRUD operations for managing products, orders, order details, and customers. It is designed to serve as a mock backend for e-commerce applications.

## Features

- **User Registration & Authentication**: Comprehensive user registration with email verification and role-based access.
- **Email Verification**: Account activation via email verification workflow.
- **Role-Based Access**: Support for Member, Librarian, and Admin roles.
- **JWT Authentication**: Secure user authentication with JWT tokens.
- **Rate Limiting**: Protection against spam and abuse with configurable rate limits.
- **Password Security**: Strong password requirements and BCrypt hashing.
- **Customers**: Create, Read, Update, and Delete customer information.
- **Products**: Manage product details including creation, retrieval, updating, and deletion.
- **Orders**: Handle order processing with CRUD operations.
- **Order Details**: Manage details of each order, including product quantities and prices.
- **Authorization**: Protected endpoints requiring authentication for sensitive operations.

## Security Features

- **Password Hashing**: All passwords are hashed using BCrypt before storage
- **Email Verification**: Accounts must verify email before activation
- **JWT Tokens**: Stateless authentication with secure token generation
- **Rate Limiting**: 
  - Registration endpoint: 3 requests per minute per IP
  - General endpoints: 30 requests per minute per IP
- **Input Validation**: Comprehensive validation for all user inputs
- **SQL Injection Protection**: Entity Framework with parameterized queries
- **XSS Protection**: Input sanitization and validation

## Technologies Used

- .NET 8
- C#
- Entity Framework Core (In-Memory Database)
- JWT Bearer Authentication
- BCrypt for password hashing
- AspNetCoreRateLimit for rate limiting
- Swagger for API documentation

## Getting Started

### Prerequisites

- .NET 8 SDK
- A code editor (e.g., Visual Studio Code)

### Installation

1. Clone the repository:
   ```
   git clone <repository-url>
   ```

2. Navigate to the project directory:
   ```
   cd ecommerce-api
   ```

3. Restore the dependencies:
   ```
   dotnet restore
   ```

4. Run the application:
   ```
   dotnet run
   ```

### API Documentation

Once the application is running, you can access the Swagger UI for API documentation at:
```
http://localhost:5000/swagger
```

## Authentication

The API uses JWT (JSON Web Token) based authentication with email verification. To access protected endpoints, you need to:

1. **Register a new user** to create an account
2. **Verify your email** using the activation token
3. **Login** to receive a JWT token
4. Use the token in the `Authorization` header for protected endpoints

### Authentication Endpoints

#### Register a New User
```bash
POST /api/auth/register
Content-Type: application/json

{
  "username": "your-username",
  "firstName": "Your",
  "lastName": "Name",
  "email": "your-email@example.com",
  "phoneNumber": "+1234567890",
  "password": "YourP@ssw0rd",
  "confirmPassword": "YourP@ssw0rd",
  "address": "123 Main St, City, State" (optional),
  "role": 1 (optional, defaults to Member: 1=Member, 2=Librarian, 3=Admin - Admin can only be assigned manually)
}
```

**Field Requirements:**
- **username**: 3-50 characters
- **firstName**: Required, max 50 characters
- **lastName**: Required, max 50 characters
- **email**: Valid email format, must be unique
- **phoneNumber**: Required, valid phone format (e.g., +1234567890)
- **password**: See password requirements below
- **confirmPassword**: Must match password
- **address**: Optional, max 200 characters
- **role**: Optional, defaults to Member (1). Admin role cannot be self-assigned

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (@$!%*?&)

**Response:**
```json
{
  "message": "User registered successfully. Please verify your email using the activation token: <token>",
  "email": "your-email@example.com",
  "username": "your-username",
  "activationToken": "<activation-token>"
}
```

**Note:** In production, the activation token would be sent via email. For demo/testing purposes, it's included in the response.

#### Verify Email
After registration, you must verify your email before you can login:

```bash
POST /api/auth/verify-email
Content-Type: application/json

{
  "email": "your-email@example.com",
  "token": "<activation-token-from-registration>"
}
```

**Response:**
```json
{
  "message": "Email verified successfully. Your account is now active."
}
```

#### Login
After email verification, you can login to receive your JWT token:

```bash
POST /api/auth/login
Content-Type: application/json

{
  "username": "your-username",
  "password": "YourP@ssw0rd"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "your-username",
  "email": "your-email@example.com",
  "expiresAt": "2025-11-18T12:00:00Z"
}
```

#### Change Password
```bash
POST /api/auth/change-password
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
  "currentPassword": "YourP@ssw0rd",
  "newPassword": "NewP@ssw0rd123"
}
```

#### Logout
```bash
POST /api/auth/logout
Authorization: Bearer <your-jwt-token>
```

### Using Authentication with Protected Endpoints

Protected endpoints (POST, PUT, DELETE on products, orders, etc.) require authentication:

```bash
POST /api/products
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
  "name": "New Product",
  "description": "Product description",
  "price": 29.99
}
```

### Using Swagger UI with Authentication

1. Go to the Swagger UI at `http://localhost:5000/swagger`
2. Click the **Authorize** button at the top
3. Enter your token in the format: `Bearer <your-jwt-token>`
4. Click **Authorize**
5. You can now test protected endpoints directly from Swagger UI

## Mock Data

The application includes a `MockDataInitializer` class that seeds the database with mock data for testing purposes. This can be useful for development and testing without needing a real database setup.

## Documentation

- [Authentication Guide](docs/AUTHENTICATION.md) - Comprehensive guide to using authentication
- [User Stories and Acceptance Criteria](docs/USER_STORIES_AND_ACCEPTANCE_CRITERIA.md)
- [Authentication Acceptance Tests](tests/auth-acceptance.md)
- [Product Acceptance Tests](tests/products-acceptance.md)
- [Login Feature Task Breakdown](docs/LOGIN_FEATURE_BREAKDOWN.md)

changes by dev-1
## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue for any suggestions or improvements.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.
