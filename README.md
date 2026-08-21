# CRN Technical Assessment — Product Management API

A production-quality RESTful Product Management API built with **ASP.NET Core 9**, demonstrating clean architecture, JWT authentication with refresh token rotation, role-based authorization, EF Core, pagination, FluentValidation, Swagger, and comprehensive testing.

---

## Project Overview

This API provides full CRUD operations for **Products** (with nested **Items**), secured with JWT Bearer authentication and role-based access control. It is organized as a clean layered architecture within a single ASP.NET Core project.

---

## Architecture

```
CRN_Technical_Assessment/
│
├── Domain/                     # Entities, domain exceptions
│   ├── Entities/               # Product, Item, User
│   └── Exceptions/             # NotFoundException, ConflictException, UnauthorizedException
│
├── Application/                # Business logic layer
│   ├── DTOs/                   # Request/response DTOs
│   ├── Interfaces/             # Service + repository contracts
│   ├── Services/               # ProductService, AuthService
│   ├── Validators/             # FluentValidation validators
│   └── Mapping/                # Manual DTO↔Entity mapping
│
├── Infrastructure/             # Data access + external concerns
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/     # EF Core fluent configs
│   │   ├── Repositories/       # ProductRepository, ItemRepository, UserRepository
│   │   ├── Migrations/         # EF Core migrations
│   │   └── UnitOfWork.cs
│   ├── Identity/               # BcryptPasswordHasher
│   └── Services/               # JwtTokenService
│
├── Controllers/
│   └── v1/                     # ProductsController, AuthController
│
├── Extensions/                 # Swagger config, Database seeder
├── Middleware/                 # ExceptionHandlingMiddleware
├── Program.cs
├── appsettings.json            # (git-ignored — use appsettings.example.json)
├── appsettings.example.json    # Safe config template
│
└── tests/
    └── CRN_Technical_Assessment.Tests/
        ├── Services/           # ProductServiceTests (unit)
        ├── Validators/         # ProductValidatorTests (unit)
        └── Integration/        # ProductsControllerIntegrationTests
```

**Request Flow:**
```
Controller → Service → Repository / UnitOfWork → EF Core → SQL Server
```

---

## Technologies

| Technology | Version | Purpose |
|---|---|---|
| ASP.NET Core | 9.0 | Web API framework |
| Entity Framework Core | 9.0 | ORM |
| SQL Server | 2022 | Database |
| JWT Bearer | 9.0 | Authentication |
| BCrypt.Net-Next | 4.0 | Password hashing |
| FluentValidation | 11.x | Input validation |
| Swashbuckle.AspNetCore | 7.3 | Swagger/OpenAPI |
| Asp.Versioning.Mvc | 8.1 | API versioning |
| xUnit | 2.x | Testing framework |
| Moq | 4.x | Mocking |
| Microsoft.AspNetCore.Mvc.Testing | 9.0 | Integration testing |

---

## Database Setup

### Requirements
- SQL Server 2019+ (local or Docker)
- .NET 9 SDK

### Connection String

Copy `appsettings.example.json` to `appsettings.json` and configure your connection string:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "Server=YOUR_SERVER;Database=ProductManagementSystem;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

### Apply Migrations

```bash
dotnet ef database update
```

This creates the `ProductManagementSystem` database with `Products`, `Items`, and `Users` tables.

---

## Running Locally

```bash
# Restore packages
dotnet restore

# Apply database migrations
dotnet ef database update

# Run the API
dotnet run

# Or with a specific profile
dotnet run --launch-profile https
```

The API starts at:
- **HTTPS**: `https://localhost:7291`
- **HTTP**: `http://localhost:5018`

On first run in Development mode, the application automatically seeds:
- Admin user and regular user
- 5 sample products with items

---

## Swagger / OpenAPI

Swagger UI is available at:

```
https://localhost:7291/swagger
```

To test protected endpoints:
1. Call `POST /api/v1/auth/login` with credentials
2. Copy the `accessToken` from the response
3. Click **Authorize** in Swagger UI
4. Enter: `Bearer <your_token>`

---

## Authentication

### Login
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

Returns `accessToken` (short-lived JWT) and `refreshToken` (long-lived).

### Refresh Token
```http
POST /api/v1/auth/refresh
Content-Type: application/json

{
  "username": "admin",
  "refreshToken": "<your_refresh_token>"
}
```

Returns a new access token and a rotated refresh token (old token is invalidated).

### Revoke Token (requires auth)
```http
POST /api/v1/auth/revoke
Authorization: Bearer <access_token>
```

### Authentication Flow

```
POST /api/v1/auth/login
        ↓
Access Token (60 min) + Refresh Token (7 days)
        ↓
API Requests with Authorization: Bearer <access_token>
        ↓
Access Token Expired? → POST /api/v1/auth/refresh
        ↓
New Access Token + Rotated Refresh Token
```

---

## API Endpoints

### Authentication (no auth required)
| Method | Route | Description |
|---|---|---|
| `POST` | `/api/v1/auth/login` | Login, get tokens |
| `POST` | `/api/v1/auth/refresh` | Refresh access token |
| `POST` | `/api/v1/auth/revoke` | Revoke refresh token (requires auth) |

### Products (auth required)
| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/api/v1/products` | User + Admin | Get paginated product list |
| `GET` | `/api/v1/products/{id}` | User + Admin | Get product by ID |
| `POST` | `/api/v1/products` | **Admin only** | Create product |
| `PUT` | `/api/v1/products/{id}` | **Admin only** | Update product |
| `DELETE` | `/api/v1/products/{id}` | **Admin only** | Delete product |

### Health
| Method | Route | Description |
|---|---|---|
| `GET` | `/health` | API + DB health check |

### Pagination Parameters
```
GET /api/v1/products?pageNumber=1&pageSize=10
```
- `pageNumber` default: 1
- `pageSize` default: 10, max: 100

---

## Request / Response Format

### Success
```json
{
  "success": true,
  "message": "Product retrieved successfully.",
  "data": { "id": 1, "productName": "Widget", ... }
}
```

### Paginated
```json
{
  "success": true,
  "message": "Products retrieved successfully.",
  "data": [...],
  "pagination": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 25,
    "totalPages": 3
  }
}
```

### Validation Error
```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": ["Product name is required.", "CreatedBy is required."]
}
```

---

## Development Seed Credentials

> ⚠️ **Development use only** — never use these in production.

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `user1` | `User@123` | User |

---

## Testing

```bash
# Run all tests (unit + integration)
dotnet test

# Verbose output
dotnet test --verbosity normal
```

**Test Coverage:**
- Unit tests: `ProductService` (CRUD + not-found scenarios), `FluentValidation` validators
- Integration tests: Auth endpoints, Product endpoints (401/403/400/200/201/404), Health check

---

## Docker

### Prerequisites
- Docker Desktop

### Setup
1. Copy `.env.example` to `.env` and fill in secrets:
   ```bash
   cp .env.example .env
   # Edit SA_PASSWORD and JWT_KEY
   ```

2. Start the application:
   ```bash
   docker compose up --build
   ```

   The API will be available at `http://localhost:8080`.

3. Apply migrations (first run):
   ```bash
   docker compose exec api dotnet ef database update
   ```

> **Note**: The API container waits for SQL Server to pass its health check before starting.

---

## Configuration

### Required Settings

| Setting | Description |
|---|---|
| `ConnectionStrings:ConnectionString` | SQL Server connection string |
| `Jwt:Key` | JWT signing secret (min 32 characters) |
| `Jwt:Issuer` | JWT issuer identifier |
| `Jwt:Audience` | JWT audience identifier |
| `Jwt:AccessTokenExpirationMinutes` | Access token lifetime in minutes |
| `Jwt:RefreshTokenExpirationDays` | Refresh token lifetime in days |

### Using Environment Variables (Docker / Production)

All settings can be overridden via environment variables using `__` as separator:

```bash
ConnectionStrings__ConnectionString=Server=...
Jwt__Key=YourSecretKey...
```

### Using .NET User Secrets (Local Development)

```bash
dotnet user-secrets set "Jwt:Key" "your_secret_key"
dotnet user-secrets set "ConnectionStrings:ConnectionString" "your_connection_string"
```

---

## Security Notes

- ⚠️ **Never commit `appsettings.json`** — it is listed in `.gitignore`
- ⚠️ **Never commit `.env`** — it is listed in `.gitignore`
- The `Jwt:Key` in `appsettings.json` is a **development placeholder** — replace with a strong secret in production
- Passwords are hashed with **BCrypt** — never stored in plaintext
- Refresh tokens are **SHA-256 hashed** before database storage
- Access tokens are **short-lived** (60 min default); refresh tokens rotate on each use

---

## Assumptions

1. Authentication is implemented with a custom `Users` table (not ASP.NET Core Identity) — appropriate for this assessment scope.
2. Item quantities must be positive integers.
3. Swagger is enabled in all environments for assessment demonstration purposes.
4. Seed data is only applied on first run when the database is empty.
5. Integration tests use the real database configured in `appsettings.json` — this is by design for assessment validation.
