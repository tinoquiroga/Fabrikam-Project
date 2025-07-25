# 🏗️ Fabrikam Monorepo Structure Guide for GitHub Copilot

## 📁 **Repository Structure Overview**

This is a **monorepo** containing multiple .NET projects. Here's the critical structure:

```
Fabrikam-Project/                    # 🏠 WORKSPACE ROOT
├── 📄 Fabrikam.sln                 # Solution file (contains all projects)
├── 🧪 Test-Development.ps1         # Main testing script
├── 📡 api-tests.http               # API testing file
│
├── 🌐 FabrikamApi/                 # ⭐ PROJECT 1: Web API
│   ├── 📄 azure.yaml              # AZD deployment config
│   ├── 📁 src/                     # Source code
│   │   ├── 📄 FabrikamApi.csproj   # 🎯 PROJECT FILE
│   │   ├── 📄 Program.cs           # Entry point
│   │   ├── 📁 Controllers/         # API controllers
│   │   ├── 📁 Models/              # Data models
│   │   └── 📁 Data/                # Database context
│   └── 📁 infra/                   # Bicep infrastructure files
│
├── 🤖 FabrikamMcp/                 # ⭐ PROJECT 2: MCP Server  
│   ├── 📄 azure.yaml              # AZD deployment config
│   ├── 📁 src/                     # Source code
│   │   ├── 📄 FabrikamMcp.csproj   # 🎯 PROJECT FILE
│   │   ├── 📄 Program.cs           # Entry point
│   │   └── 📁 Tools/               # MCP tools
│   └── 📁 infra/                   # Bicep infrastructure files
│
├── 📄 FabrikamContracts/           # ⭐ PROJECT 3: Shared DTOs
│   └── 📄 FabrikamContracts.csproj # 🎯 PROJECT FILE
│
└── 🧪 FabrikamTests/               # ⭐ PROJECT 4: Test Project
    └── 📄 FabrikamTests.csproj     # 🎯 PROJECT FILE
```

## 🚀 **How to Start/Stop Projects**

### ⚡ **Quick Start (Recommended)**
```powershell
# FROM WORKSPACE ROOT: c:\Users\davidb\1Repositories\Fabrikam-Project

# 🌐 Start API Server (Terminal 1)
dotnet run --project FabrikamApi\src\FabrikamApi.csproj

# 🤖 Start MCP Server (Terminal 2) 
dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj

# 🧪 Run Tests
.\Test-Development.ps1 -Quick
```

### 📁 **Working Directory Context**
- **ALWAYS** run commands from **workspace root** (`c:\Users\davidb\1Repositories\Fabrikam-Project`)
- **NEVER** navigate into individual project folders unless specifically needed
- Use `--project` flag to specify which project to run

### 🔗 **Project Dependencies**
- **FabrikamApi**: Independent web API (port 7241)
- **FabrikamMcp**: Independent MCP server (port 5000)  
- **FabrikamContracts**: Shared DTOs (referenced by both)
- **FabrikamTests**: Tests both API and MCP projects

## 🎯 **Common Copilot Confusion Points & Solutions**

### ❌ **WRONG: Navigating to individual folders**
```powershell
# DON'T DO THIS
cd FabrikamApi\src
dotnet run  # This confuses the monorepo context
```

### ✅ **CORRECT: Always from workspace root**
```powershell
# DO THIS INSTEAD
# Stay in: c:\Users\davidb\1Repositories\Fabrikam-Project
dotnet run --project FabrikamApi\src\FabrikamApi.csproj
```

### ❌ **WRONG: Generic .NET commands**
```powershell
# DON'T DO THIS - too ambiguous in monorepo
dotnet build
dotnet run
```

### ✅ **CORRECT: Specific project commands**
```powershell
# DO THIS INSTEAD
dotnet build Fabrikam.sln                               # Build all projects
dotnet run --project FabrikamApi\src\FabrikamApi.csproj # Run specific project
dotnet test FabrikamTests\FabrikamTests.csproj          # Test specific project
```

## 🛠️ **Development Workflow Commands**

### 🏗️ **Building Projects**
```powershell
# Build entire solution
dotnet build Fabrikam.sln

# Build specific project
dotnet build FabrikamApi\src\FabrikamApi.csproj
dotnet build FabrikamMcp\src\FabrikamMcp.csproj
```

### 🚀 **Running Projects**
```powershell
# API Server (HTTPS on port 7241)
dotnet run --project FabrikamApi\src\FabrikamApi.csproj

# MCP Server (HTTP on port 5000)
dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj

# With specific launch profile
dotnet run --project FabrikamApi\src\FabrikamApi.csproj --launch-profile https
```

### 🧪 **Testing Projects**
```powershell
# Comprehensive testing (RECOMMENDED)
.\Test-Development.ps1 -Verbose

# Manual testing
dotnet test FabrikamTests\FabrikamTests.csproj

# Specific test category
.\Test-Development.ps1 -ApiOnly
.\Test-Development.ps1 -McpOnly
```

### 📦 **Package Management**
```powershell
# Add package to specific project
dotnet add FabrikamApi\src\FabrikamApi.csproj package Microsoft.EntityFrameworkCore
dotnet add FabrikamMcp\src\FabrikamMcp.csproj package Newtonsoft.Json

# Restore all packages
dotnet restore Fabrikam.sln
```

## 🔧 **VS Code Integration**

### 📁 **Workspace Settings**
- **Default Solution**: `Fabrikam.sln`
- **Working Directory**: Workspace root
- **Project Discovery**: Automatic via solution file

### 🎯 **Launch Configurations**
```json
// .vscode/launch.json should target specific projects:
{
    "name": "Launch API",
    "program": "${workspaceFolder}/FabrikamApi/src/bin/Debug/net9.0/FabrikamApi.dll",
    "cwd": "${workspaceFolder}"
}
```

## 🚨 **Important Notes for GitHub Copilot**

### 🎯 **Project Context Clues**
When working with this monorepo:

1. **File paths indicate project**:
   - `FabrikamApi/src/Controllers/` → API project
   - `FabrikamMcp/src/Tools/` → MCP project
   - `FabrikamTests/Api/` → API tests
   - `FabrikamTests/Mcp/` → MCP tests

2. **Always specify full project paths**:
   - Use `FabrikamApi\src\FabrikamApi.csproj`
   - Use `FabrikamMcp\src\FabrikamMcp.csproj`

3. **Multiple entry points**:
   - Each project has its own `Program.cs`
   - Each project has its own configuration
   - Each project runs independently

### 🔄 **Development Process**
1. **Start both servers**: API (7241) + MCP (5000)
2. **Test integration**: Use `Test-Development.ps1`
3. **Make changes**: Restart affected server only
4. **Verify**: Re-run tests

### 🏆 **Success Indicators**
- ✅ API responds on `https://localhost:7241`
- ✅ MCP responds on `http://localhost:5000`
- ✅ Tests pass: `.\Test-Development.ps1 -Quick`
- ✅ Both servers show in `Test-Development.ps1 -Verbose`

## 🚀 **Quick Reference Commands**

```powershell
# 🏠 FROM WORKSPACE ROOT ONLY!

# Start Development
dotnet run --project FabrikamApi\src\FabrikamApi.csproj  # Terminal 1
dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj  # Terminal 2

# Test Everything  
.\Test-Development.ps1 -Quick

# Build Solution
dotnet build Fabrikam.sln

# Stop Servers
Ctrl+C in each terminal
```

---

**🎯 CRITICAL FOR COPILOT**: This is a monorepo with multiple independent .NET projects. Always use `--project` flags and full paths from workspace root. Never navigate into individual project directories unless absolutely necessary.
