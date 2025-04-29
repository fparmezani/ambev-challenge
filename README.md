# Ambev Developer Evaluation - Sales API

## Overview

This project is a .NET Core Web API for managing sales operations. It follows Clean Architecture principles and implements CQRS pattern using MediatR for handling commands and queries.

## Features

- Create and manage sales with multiple items
- Retrieve individual sales by ID
- List sales with pagination
- Modify item quantities in a sale
- Remove items from a sale
- Cancel sales
- Business rule enforcement (e.g., maximum 20 items per product)

## Tech Stack

- .NET Core 6.0+
- Entity Framework Core
- MediatR for CQRS implementation
- Docker support for containerization
- Unit and integration testing

## Project Structure

The solution follows Clean Architecture principles with the following projects:

- **Ambev.DeveloperEvaluation.WebApi**: API controllers and endpoints
- **Ambev.DeveloperEvaluation.Application**: Application services, commands, queries, and DTOs
- **Ambev.DeveloperEvaluation.Domain**: Domain entities, business rules, and interfaces
- **Ambev.DeveloperEvaluation.ORM**: Database access and Entity Framework implementation
- **Ambev.DeveloperEvaluation.Common**: Shared utilities and common components
- **Ambev.DeveloperEvaluation.IoC**: Dependency injection configuration

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Docker (optional, for containerized deployment)

### Running Locally

1. Clone the repository
2. Navigate to the project directory
3. Run the application using .NET CLI:

```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.csproj
```

### Using Docker

```bash
docker-compose up
```

## API Endpoints

### Sales

- **POST /api/sales** - Create a new sale
- **GET /api/sales/{id}** - Get a sale by ID
- **GET /api/sales** - List sales with pagination
- **PATCH /api/sales/{id}/items/{productId}** - Modify item quantity
- **DELETE /api/sales/{id}/items/{productId}** - Remove an item from a sale
- **POST /api/sales/{id}/cancel** - Cancel a sale

## Running Tests

```bash
# Windows
.\coverage-report.bat

# Linux/macOS
./coverage-report.sh
```

## Domain Model

The core domain entities include:

- **Sale**: The aggregate root representing a sales transaction
- **SaleItem**: Individual items within a sale
- **ProductInfo**: Information about products being sold
- **CustomerInfo**: Information about the customer
- **BranchInfo**: Information about the branch where the sale occurs

## Business Rules

- Sales have a unique sale number
- Each product in a sale can have a maximum quantity of 20 units
- Cancelled sales cannot be modified
- Sale items must have positive quantities

## License

[MIT](LICENSE)

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.
