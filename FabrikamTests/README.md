# Fabrikam Testing Strategy

## 🗄️ Database Configuration Strategy

The project uses a **unified database context** (`FabrikamIdentityDbContext`) that combines:

- **Business Data**: Customers, Orders, Products, Support Tickets
- **Authentication Data**: Users, Roles, Claims, User-Role assignments

### Database Provider Configuration

The database provider is explicitly configurable via `Database:Provider` setting:

#### **In-Memory Database** (Default for Development/Testing)

```json
{
  "Database": {
    "Provider": "InMemory",
    "Description": "Fast, isolated testing - no persistence"
  }
}
```

#### **SQL Server** (Production/Integration Testing)

```json
{
  "Database": {
    "Provider": "SqlServer",
    "Description": "Full persistence and scalability"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  }
}
```

#### **Auto-Detection** (Legacy Compatibility)

```json
{
  "Database": {
    "Provider": "Auto",
    "Description": "SqlServer if connection string exists, InMemory otherwise"
  }
}
```

## Test Categories

### **Fast Tests (Default - In-Memory Database)**

```powershell
# Run all fast tests (default)
dotnet test FabrikamTests/

# Run specific categories
dotnet test FabrikamTests/ --filter "Category=Api"
dotnet test FabrikamTests/ --filter "Category=Authentication"
```

### **Integration Tests (SQL Server)**

```powershell
# Run SQL Server integration tests (requires database)
dotnet test FabrikamTests/ --filter "Category=SqlServer"

# Run specific Issue #4 integration tests
dotnet test FabrikamTests/ --filter "Issue=4&Category=SqlServer"

# Skip SQL Server tests (useful for CI without database)
dotnet test FabrikamTests/ --filter "Category!=SqlServer"
```

## Configuration

### **Development Environment**

- **Fast Tests**: Use in-memory database (current default)
- **Integration Tests**: Connect to local SQL Server or Azure SQL

### **CI/CD Pipeline**

- **Pull Requests**: Fast tests only (in-memory)
- **Main Branch**: Fast tests + Integration tests (with SQL Server)

### **Local Development**

```powershell
# Quick development cycle (fast)
.\Test-Development.ps1 -Quick

# Full validation (includes SQL Server if available)
.\Test-Development.ps1 -Verbose
```

## Environment Variables

Set these for SQL Server integration tests:

```bash
# For local SQL Server
export ConnectionStrings__DefaultConnection="Server=localhost;Database=FabrikamDev;Integrated Security=true;TrustServerCertificate=true;"

# For Azure SQL Database
export ConnectionStrings__DefaultConnection="Server=your-server.database.windows.net;Database=fabrikam-dev;Authentication=Active Directory Default;"
```

## Best Practices

1. **Default to Fast**: In-memory tests run by default
2. **Conditional Integration**: SQL Server tests skip gracefully if database unavailable
3. **CI/CD Friendly**: Both approaches work in automated pipelines
4. **Performance Validation**: SQL Server tests validate real-world performance
5. **Schema Validation**: Integration tests verify actual database schema

## Test Execution Strategy

### **Development Workflow**

1. Write feature code
2. Run fast tests (`dotnet test`)
3. Fix any issues
4. Run integration tests locally
5. Commit when all pass

### **CI/CD Workflow**

1. **Fast Pipeline**: In-memory tests on every PR
2. **Full Pipeline**: Integration tests on main branch
3. **Nightly**: Full suite including performance benchmarks
