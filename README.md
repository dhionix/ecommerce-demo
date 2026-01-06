# E-commerce API

This is a simple e-commerce API built using .NET 8. The API provides CRUD operations for managing products, orders, order details, and customers. It is designed to serve as a mock backend for e-commerce applications.

## Features

- **Authentication**: Secure user registration and login with JWT tokens.
- **Customers**: Create, Read, Update, and Delete customer information.
- **Products**: Manage product details including creation, retrieval, updating, and deletion.
- **Orders**: Handle order processing with CRUD operations.
- **Order Details**: Manage details of each order, including product quantities and prices.
- **Authorization**: Protected endpoints requiring authentication for sensitive operations.

## Technologies Used

- .NET 8
- C#
- Entity Framework Core (In-Memory Database)
- JWT Bearer Authentication
- BCrypt for password hashing
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

The API uses JWT (JSON Web Token) based authentication. To access protected endpoints, you need to:

1. **Register a new user** or **Login** to receive a JWT token
2. Use the token in the `Authorization` header for protected endpoints

### Authentication Endpoints

#### Register a New User
```bash
POST /api/auth/register
Content-Type: application/json

{
  "username": "your-username",
  "email": "your-email@example.com",
  "password": "YourP@ssw0rd"
}
```

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (@$!%*?&)

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "your-username",
  "email": "your-email@example.com",
  "expiresAt": "2025-11-18T12:00:00Z"
}
```

#### Login
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
