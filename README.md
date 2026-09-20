# Job Application Management API

A robust, enterprise-grade ASP.NET Core Web API for managing job postings and applications. This project is built using **Clean Architecture** principles and implements the **CQRS (Command Query Responsibility Segregation)** pattern with **MediatR** to ensure scalability, maintainability, and a clear separation of concerns.

## 🏛 Architecture

The solution follows a strict Clean Architecture structure, consisting of the following layers:

- **`JobApplicationManagement.Domain`**: Contains enterprise logic and types (Entities, Enums, Exceptions). It has zero dependencies on other layers or frameworks.
- **`JobApplicationManagement.Application`**: Contains the business use cases (Commands, Queries, Handlers) and interfaces (e.g., `IUnitOfWork`, `ICurrentUserService`).
- **`JobApplicationManagement.Infrastructure`**: Implements the interfaces defined in the Application layer. Contains Entity Framework Core configurations, Data Access, Identity (JWT), and external services.
- **`JobApplicationManagement.API`**: The presentation layer. Contains the ASP.NET Core Controllers, Middleware, and Dependency Injection bootstrapping.

## ✨ Key Features

- **CQRS Pattern**: Segregation of read (Queries) and write (Commands) operations using `MediatR`.
- **JWT Authentication & Authorization**: Secure role-based access control protecting specific endpoints (e.g., Candidate vs. Recruiter actions).
- **Centralized Error Handling**: Global exception handling middleware ensuring consistent API error responses.
- **Global Logging**: Centralized structured logging using `Serilog`, capturing request metrics, trace IDs, and user context.
- **Data Integrity**: Database-level unique constraints and cascade delete protections (Restrict).
- **Health Checks**: Built-in endpoints for monitoring API and SQL Server database health.
- **API Documentation**: Comprehensive Swagger UI with integrated XML comments.
- **Containerization**: Multi-stage Dockerfile for optimized production builds.

## 🛠 Technologies & Tools

- **.NET 8.0**
- **Entity Framework Core 8.0** (Code-First Migrations)
- **SQL Server**
- **MediatR**
- **Serilog**
- **xUnit & Moq** (Unit Testing)
- **Docker**
- **GitHub Actions** (CI pipeline)

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB or Docker instance)
- [Docker](https://www.docker.com/) (Optional, for containerized execution)

### 1. Clone the repository
```bash
git clone https://github.com/your-username/JobApplicationManagement.git
cd JobApplicationManagement/JobApplicationManagement.API
```

### 2. Configure the Database
Update the `DefaultConnection` string in `JobApplicationManagement.API/appsettings.json` (or `appsettings.Development.json`) to point to your local SQL Server instance.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=JobAppDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Apply Migrations
Navigate to the API project folder and apply the EF Core migrations to create the database schema:
```bash
dotnet ef database update -s JobApplicationManagement.API -p JobApplicationManagement.Infrastucture
```

### 4. Run the API
```bash
cd JobApplicationManagement.API
dotnet run
```

Navigate to `https://localhost:<port>/swagger` in your browser to view the API documentation and test the endpoints.

---

## 🐳 Docker Setup

You can easily run the application using Docker. A multi-stage `Dockerfile` is included in the root of the repository.

1. **Build the image:**
```bash
docker build -t jobapp-api -f Dockerfile .
```

2. **Run the container:**
*(Make sure to pass a valid connection string pointing to a reachable SQL Server instance)*
```bash
docker run -it --rm -p 8080:8080 -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=JobAppDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True" jobapp-api
```

---

## 🧪 Testing

The project includes an `xUnit` test suite focusing on the core business logic (e.g., Application Cancellation rules, Ownership validation).

To run the tests:
```bash
dotnet test
```

---

## 🏥 Health Checks

The API includes a built-in health check endpoint that verifies the status of the application and its connection to the SQL Server database.

- **Endpoint**: `GET /health`
- **Response**: `Healthy` / `Unhealthy`

---

## 📄 License
This project is licensed under the MIT License. See the `LICENSE` file for details.