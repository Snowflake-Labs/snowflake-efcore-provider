# Entity Framework Core Provider for Snowflake

This repository is in maintenance mode only by Snowflake in Snowflake-Labs.

## Key Points About Snowflake-Labs Repositories
- Critical security issues will be patched if they are reported or detected.
- Official Snowflake Support will not be provided.
- The repository will not be actively monitored.

This is the open-source EF Core provider for Snowflake. It enables seamless interaction with Snowflake using Microsoft's widely-used .NET Object-Relational Mapper (ORM). 
With this provider, you can write queries using familiar LINQ syntax and take advantage of the powerful features of Entity Framework Core in your Snowflake projects.

## Getting Started

### Installation
To install the Snowflake EF Core provider, use the following command in your .NET project:

```bash
dotnet add package Snowflake.EntityFrameworkCore
```

### Usage
The provider integrates smoothly with Entity Framework Core, making it feel like any other EF Core provider. Here's a quick example to get started:
```csharp
using Microsoft.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore;

public class MyEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class SampleDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnowflake(
            "account=<your_account>;host=<your_host>;password=<your_password>;role=<your_role>;schema=<your_schema>;user=<your_user>;warehouse=<your_warehouse>;");
    }

    public DbSet<MyEntity> MyEntities { get; set; }
}

class Program
{
    static void Main()
    {
        using var context = new SampleDbContext();
        context.MyEntities.Add(new MyEntity { Id = 1, Name = "Sample Entity" });
        context.SaveChanges();
    }
}
```

## Development

### Running Tests
The project includes two test solutions:

- **Unit Tests**: Located in the `EFCore.Tests` project.
- **Functional Tests**: Located in the `Snowflake.EntityFrameworkCore.FunctionalTests` project.

#### Prerequisites
To run the tests, you need to create a `parameters.json` file for each test project.

Should be created in the following locations:
1. `.../EFCore.Tests/parameters.json`
2. `.../tests/Snowflake.EntityFrameworkCore.FunctionalTests/parameters.json`

The file should have the following structure:

```json
{
  "testconnection": {
    "SNOWFLAKE_TEST_HOST": "your_host",
    "SNOWFLAKE_TEST_USER": "your_user",
    "SNOWFLAKE_TEST_PASSWORD": "your_password",
    "SNOWFLAKE_TEST_ACCOUNT": "your_account",
    "SNOWFLAKE_TEST_WAREHOUSE": "your_warehouse",
    "SNOWFLAKE_TEST_DATABASE": "your_database",
    "SNOWFLAKE_TEST_SCHEMA": "your_schema",
    "SNOWFLAKE_TEST_ROLE": "your_role"
  }
}

```
#### Steps to Run Tests
1. Create a `parameters.json` file in the root directory of each test project using the structure provided above.
2. Ensure all dependencies are installed and your Snowflake test account is configured correctly.
3. Run the tests using your preferred test runner (e.g., Rider Test Explorer or the `dotnet test` CLI).

```bash
dotnet test EFCore.Tests
dotnet test Snowflake.EntityFrameworkCore.FunctionalTests
```

## Contributing
We welcome contributions from the community! To contribute:

1. Fork the repository and clone it locally.
2. Create a new branch for your feature or bug fix.
3. Run test locally.
4. Commit your changes and push them to your forked repository.
5. Submit a pull request detailing your changes.

## References
[Entity Framework Core](https://github.com/dotnet/efcore)
[Scaffolding (Reverse Engineering)](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/?tabs=dotnet-core-cli)
[Migrations Overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli)
