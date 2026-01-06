# Acceptance Tests for Authentication

## **Feature:** User Registration

**Scenario:** Successfully register a new user

- **Given** the API is running
- **And** no user exists with username "newuser"
- **When** a POST request is sent to `/api/auth/register` with valid user data:
  ```json
  {
    "username": "newuser",
    "email": "newuser@example.com",
    "password": "SecureP@ss123"
  }
  ```
- **Then** the response should return status `201 Created`
- **And** the response should contain a JWT token
- **And** the response should include username and email
- **And** the response should include token expiration time

**Scenario:** Fail to register with duplicate username

- **Given** a user already exists with username "existinguser"
- **When** a POST request is sent to `/api/auth/register` with the same username
- **Then** the response should return status `400 Bad Request`
- **And** the response should contain an error message about duplicate username

**Scenario:** Fail to register with duplicate email

- **Given** a user already exists with email "existing@example.com"
- **When** a POST request is sent to `/api/auth/register` with the same email
- **Then** the response should return status `400 Bad Request`
- **And** the response should contain an error message about duplicate email

**Scenario:** Fail to register with invalid password

- **Given** the API is running
- **When** a POST request is sent to `/api/auth/register` with a weak password (e.g., "weak")
- **Then** the response should return status `400 Bad Request`
- **And** the response should contain validation errors about password requirements

**Scenario:** Fail to register with missing fields

- **Given** the API is running
- **When** a POST request is sent to `/api/auth/register` with missing required fields
- **Then** the response should return status `400 Bad Request`
- **And** the response should contain validation errors

---

## **Feature:** User Login

**Scenario:** Successfully login with valid credentials

- **Given** a user exists with username "testuser" and password "Test@1234"
- **When** a POST request is sent to `/api/auth/login` with valid credentials:
  ```json
  {
    "username": "testuser",
    "password": "Test@1234"
  }
  ```
- **Then** the response should return status `200 OK`
- **And** the response should contain a JWT token
- **And** the response should include username and email
- **And** the user's last login time should be updated

**Scenario:** Fail to login with invalid username

- **Given** the API is running
- **When** a POST request is sent to `/api/auth/login` with non-existent username
- **Then** the response should return status `401 Unauthorized`
- **And** the response should contain a generic error message

**Scenario:** Fail to login with invalid password

- **Given** a user exists with username "testuser"
- **When** a POST request is sent to `/api/auth/login` with incorrect password
- **Then** the response should return status `401 Unauthorized`
- **And** the response should contain a generic error message

**Scenario:** Fail to login with inactive account

- **Given** a user exists but is marked as inactive
- **When** a POST request is sent to `/api/auth/login` with valid credentials
- **Then** the response should return status `401 Unauthorized`
- **And** the response should indicate account is inactive

---

## **Feature:** Password Change

**Scenario:** Successfully change password

- **Given** a user is logged in with valid JWT token
- **When** a POST request is sent to `/api/auth/change-password` with:
  ```json
  {
    "currentPassword": "OldPass@123",
    "newPassword": "NewPass@456"
  }
  ```
- **And** the Authorization header contains a valid Bearer token
- **Then** the response should return status `200 OK`
- **And** the response should confirm password change
- **And** the user should be able to login with the new password

**Scenario:** Fail to change password with incorrect current password

- **Given** a user is logged in with valid JWT token
- **When** a POST request is sent to `/api/auth/change-password` with incorrect current password
- **Then** the response should return status `400 Bad Request`
- **And** the response should indicate current password is incorrect

**Scenario:** Fail to change password without authentication

- **Given** the API is running
- **When** a POST request is sent to `/api/auth/change-password` without Authorization header
- **Then** the response should return status `401 Unauthorized`

**Scenario:** Fail to change password with invalid new password

- **Given** a user is logged in with valid JWT token
- **When** a POST request is sent to `/api/auth/change-password` with weak new password
- **Then** the response should return status `400 Bad Request`
- **And** the response should contain validation errors about password requirements

---

## **Feature:** User Logout

**Scenario:** Successfully logout

- **Given** a user is logged in with valid JWT token
- **When** a POST request is sent to `/api/auth/logout` with valid Bearer token
- **Then** the response should return status `200 OK`
- **And** the response should instruct client to remove token

**Scenario:** Fail to logout without authentication

- **Given** the API is running
- **When** a POST request is sent to `/api/auth/logout` without Authorization header
- **Then** the response should return status `401 Unauthorized`

---

## **Feature:** Protected Endpoint Access

**Scenario:** Successfully access protected endpoint with valid token

- **Given** a user is logged in and has a valid JWT token
- **When** a POST request is sent to `/api/products` with valid Bearer token
- **Then** the response should return status `201 Created` (or appropriate success status)
- **And** the operation should be performed successfully

**Scenario:** Fail to access protected endpoint without token

- **Given** the API is running
- **When** a POST request is sent to `/api/products` without Authorization header
- **Then** the response should return status `401 Unauthorized`

**Scenario:** Fail to access protected endpoint with invalid token

- **Given** the API is running
- **When** a POST request is sent to `/api/products` with invalid or malformed token
- **Then** the response should return status `401 Unauthorized`

**Scenario:** Fail to access protected endpoint with expired token

- **Given** a JWT token that has expired
- **When** a POST request is sent to `/api/products` with the expired token
- **Then** the response should return status `401 Unauthorized`

---

## **Feature:** JWT Token Validation

**Scenario:** Token contains correct user claims

- **Given** a user logs in successfully
- **When** the JWT token is decoded
- **Then** the token should contain:
  - User ID claim (nameid)
  - Username claim (unique_name)
  - Email claim (email)
  - JWT ID (jti)
  - Issuer (iss)
  - Audience (aud)
  - Expiration (exp)

**Scenario:** Token expiration is configurable

- **Given** JWT expiration is configured to 60 minutes in production
- **When** a user logs in
- **Then** the token should expire 60 minutes after issuance
- **And** the expiresAt field should reflect the correct expiration time

---

## **Feature:** Password Security Requirements

**Scenario:** Password validation enforces security rules

- **Given** the API is running
- **When** attempting to register or change password
- **Then** the password must meet all requirements:
  - Minimum 8 characters
  - At least one uppercase letter
  - At least one lowercase letter
  - At least one digit
  - At least one special character (@$!%*?&)

**Scenario:** Password is securely hashed

- **Given** a user registers with password "SecureP@ss123"
- **When** the user data is stored
- **Then** the password should be stored as a BCrypt hash
- **And** the original password should never be stored in plain text
- **And** each user should have a unique hash (due to salt)

---

## **Security Tests**

**Scenario:** API prevents SQL injection in login

- **Given** the API is running
- **When** a login request contains SQL injection attempts in username or password
- **Then** the request should be safely handled without executing SQL
- **And** the response should return `401 Unauthorized`

**Scenario:** API rate limits login attempts

- **Given** the API is running
- **When** multiple failed login attempts are made from the same source
- **Then** the API should implement rate limiting (future enhancement)
- **Note:** This is a recommended security feature for production

**Scenario:** Sensitive information is not exposed in error messages

- **Given** the API is running
- **When** an authentication error occurs
- **Then** error messages should not reveal whether username or email exists
- **And** error messages should not expose system internals

---

## **Integration Tests**

**Scenario:** Complete user journey - Registration to Protected Resource Access

- **Given** the API is running
- **When** a new user registers at `/api/auth/register`
- **And** uses the returned token to access `/api/products` (POST)
- **Then** both operations should succeed
- **And** the user should be able to create a product

**Scenario:** Complete user journey - Login and password change

- **Given** a user exists in the system
- **When** the user logs in at `/api/auth/login`
- **And** uses the token to change password at `/api/auth/change-password`
- **And** logs out at `/api/auth/logout`
- **And** logs in again with the new password
- **Then** all operations should succeed

---

## **API Documentation Tests**

**Scenario:** Swagger UI displays authentication endpoints

- **Given** the API is running
- **When** navigating to `/swagger`
- **Then** all authentication endpoints should be visible:
  - POST /api/auth/register
  - POST /api/auth/login
  - POST /api/auth/change-password
  - POST /api/auth/logout

**Scenario:** Swagger UI has authentication support

- **Given** the Swagger UI is loaded
- **When** viewing the page
- **Then** an "Authorize" button should be available
- **And** users should be able to input their JWT token
- **And** protected endpoints should show a lock icon
