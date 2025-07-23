# Git Repository Setup and .gitignore Configuration

This document explains the `.gitignore` configuration for the Fabrikam Project, which consists of two .NET applications: FabrikamApi and FabrikamMcp.

## Repository Structure

```
Fabrikam-Project/
├── .gitignore                    ← Root-level gitignore (comprehensive)
├── FabrikamApi/
│   ├── .gitignore                ← API-specific gitignore
│   ├── src/
│   │   ├── Controllers/
│   │   ├── FabrikamApi.csproj
│   │   └── Program.cs
│   ├── infra/                    ← Bicep infrastructure files
│   └── azure.yaml
├── FabrikamMcp/
│   ├── .gitignore                ← MCP-specific gitignore
│   ├── src/
│   │   ├── Tools/
│   │   ├── McpServer.csproj
│   │   └── Program.cs
│   ├── infra/                    ← Bicep infrastructure files
│   └── azure.yaml
└── Documentation files
```

## .gitignore Files Created

### 1. Root Level `.gitignore`
**Location**: `Fabrikam-Project/.gitignore`

**Purpose**: Provides comprehensive ignore patterns for the entire solution, including:
- .NET build artifacts (`bin/`, `obj/`, `publish/`)
- Visual Studio and VS Code files
- NuGet packages and caches
- User-specific files
- Azure deployment artifacts
- Operating system specific files
- Various IDE and tool-specific files

### 2. FabrikamApi `.gitignore`
**Location**: `Fabrikam-Project/FabrikamApi/.gitignore`

**Purpose**: API-specific patterns including:
- ASP.NET Core specific files
- Entity Framework artifacts
- API documentation builds
- Health check databases
- Application Insights configurations
- Web publish profiles
- Local development settings

### 3. FabrikamMcp `.gitignore`
**Location**: `Fabrikam-Project/FabrikamMcp/.gitignore`

**Purpose**: MCP server specific patterns including:
- Model Context Protocol cache files
- MCP client configurations
- Server logs and runtime data
- HTTP files with sensitive data
- Certificate files
- Tool-specific configurations

## Key Patterns Ignored

### Build Artifacts
```
bin/
obj/
publish/
*.dll
*.exe
```

### Configuration Files
```
appsettings.*.json
!appsettings.json
!appsettings.Development.json
local.settings.json
```

### Azure Deployment
```
.azure/
!.azure/.env
!.azure/*/.env
infra/*.json
!infra/*.parameters.json
```

### IDE and Tools
```
.vs/
.vscode/
!.vscode/settings.json
!.vscode/tasks.json
.idea/
*.suo
*.user
```

### Security and Sensitive Data
```
*.pfx
*.p12
*.key
*.crt
UserSecrets/
.env
.env.local
```

## Files Cleaned Up

The following build artifacts were removed as they should not be tracked:
- `FabrikamApi/bin/` and `FabrikamApi/obj/`
- `FabrikamApi/src/bin/`, `FabrikamApi/src/obj/`, `FabrikamApi/src/publish/`
- `FabrikamMcp/src/bin/` and `FabrikamMcp/src/obj/`

## Important Files That ARE Tracked

The following files are explicitly allowed and should be tracked:
- `appsettings.json` and `appsettings.Development.json`
- `infra/*.parameters.json` (Bicep parameter files)
- `.vscode/settings.json`, `.vscode/tasks.json`, `.vscode/launch.json`
- `azure.yaml` (Azure Developer CLI configuration)
- All source code files (`.cs`, `.csproj`, etc.)

## Git Repository Initialization

If this is not yet a git repository, initialize it with:

```bash
# From the Fabrikam-Project directory
git init
git add .
git commit -m "Initial commit with proper .gitignore configuration"
```

## Best Practices

1. **Never commit build artifacts** - these are automatically generated
2. **Never commit user secrets or sensitive configuration** - use Azure Key Vault or user secrets
3. **Always use parameters files for deployment** - never hardcode sensitive values
4. **Keep development settings separate** - use `appsettings.Development.json` for local development
5. **Use environment variables** - for deployment-specific configuration

## Maintenance

- Review and update `.gitignore` files when adding new tools or dependencies
- Regularly clean build artifacts with `dotnet clean`
- Use `git status` to verify that only intended files are staged for commit
- Consider using `git check-ignore <file>` to test if a file will be ignored

## Azure Deployment Considerations

- `.azure/` folders are ignored but environment files (`.env`) are allowed
- Bicep compiled JSON templates are ignored but parameter files are tracked
- Azure publish profiles are ignored for security
- Application Insights configuration files are ignored (use environment variables instead)

This configuration ensures a clean repository with proper separation of source code, configuration, and generated artifacts.
