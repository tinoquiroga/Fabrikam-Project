# Contributing to Fabrikam Project

Thank you for your interest in contributing to the Fabrikam Project! This guide will help you get started with contributing to our .NET business simulation platform.

## 🚀 Quick Start

1. **Fork the repository** and clone your fork
2. **Set up your development environment** (see [Development Workflow](docs/development/DEVELOPMENT-WORKFLOW.md))
3. **Create a new branch** for your feature/fix
4. **Make your changes** following our coding standards
5. **Test thoroughly** using our testing tools
6. **Submit a pull request** with a clear description

## 🏗️ Project Structure

This is a **monorepo** with multiple .NET projects:

```
Fabrikam-Project/
├── FabrikamApi/          # ASP.NET Core Web API
├── FabrikamMcp/          # Model Context Protocol Server
├── FabrikamTests/        # Automated tests
├── FabrikamContracts/    # Shared contracts
└── .github/              # GitHub templates and workflows
```

**Important:** Always work from the repository root and use `--project` flags for .NET commands.

## 🎯 Types of Contributions

We welcome several types of contributions:

### 🐛 Bug Fixes
- Report bugs using our [bug report template](.github/ISSUE_TEMPLATE/bug_report.yml)
- Include reproduction steps and environment details
- Test your fix thoroughly before submitting

### ✨ New Features
- Propose features using our [feature request template](.github/ISSUE_TEMPLATE/feature_request.yml)
- Discuss the approach before implementing large features
- Ensure features align with the business simulation goals

### 📚 Documentation
- Improve existing documentation
- Add examples and clarifications
- Fix typos and broken links
- Use our [documentation template](.github/ISSUE_TEMPLATE/documentation.yml)

### 🧪 Testing
- Add test coverage for existing features
- Improve test reliability and performance
- Enhance our testing automation

## 🔧 Development Setup

### Prerequisites
- .NET 9.0 SDK
- VS Code with C# extension
- Git
- PowerShell (for testing scripts)

### Setup Steps
```powershell
# Clone your fork
git clone https://github.com/yourusername/Fabrikam-Project.git
cd Fabrikam-Project

# Build the solution
dotnet build Fabrikam.sln

# Run tests to ensure everything works
.\Test-Development.ps1 -Quick
```

### Development Workflow
```powershell
# Start both servers (in separate terminals)
dotnet run --project FabrikamApi\src\FabrikamApi.csproj
dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj

# Run tests frequently during development
.\Test-Development.ps1 -Quick      # Fast health check
.\Test-Development.ps1 -ApiOnly    # API testing
.\Test-Development.ps1 -McpOnly    # MCP testing
.\Test-Development.ps1 -Verbose    # Full integration testing
```

## 📋 Coding Standards

### General Principles
- ✅ Use async/await patterns consistently
- ✅ Implement comprehensive error handling
- ✅ Use structured logging with parameters
- ✅ Follow dependency injection patterns
- ✅ Write self-documenting code with clear names

### API Development
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<Product>> GetProduct(int id)
{
    try
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            return NotFound($"Product with ID {id} not found");
        }
        
        return Ok(product);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving product {ProductId}", id);
        return StatusCode(500, "An error occurred while retrieving the product");
    }
}
```

### MCP Tool Development
```csharp
[McpServerTool, Description("Clear description of what this tool does")]
public async Task<object> NewTool(string? parameter = null)
{
    try
    {
        var baseUrl = _configuration["FabrikamApi:BaseUrl"] ?? "https://localhost:7297";
        var response = await _httpClient.GetAsync($"{baseUrl}/api/endpoint");
        
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsStringAsync();
            return new
            {
                content = new object[]
                {
                    new { type = "text", text = FormatResponse(data) }
                },
                data = data
            };
        }
        
        return new
        {
            error = new { message = $"Error: {response.StatusCode}" }
        };
    }
    catch (Exception ex)
    {
        return new { error = new { message = $"Error: {ex.Message}" } };
    }
}
```

## 🧪 Testing Requirements

### Before Submitting a PR
1. **Run all automated tests:**
   ```powershell
   dotnet test FabrikamTests/
   .\Test-Development.ps1 -Verbose
   ```

2. **Manual testing:**
   - Test API endpoints via `api-tests.http`
   - Test MCP tools in development environment
   - Verify integration between components

3. **Test coverage:**
   - Add unit tests for new functionality
   - Ensure integration tests pass
   - Update test documentation as needed

### Testing Tools
- **Unit Tests:** `FabrikamTests/` directory
- **Integration Tests:** `Test-Development.ps1` script
- **Manual API Testing:** `api-tests.http` file
- **CI/CD Testing:** GitHub Actions workflows

## 📝 Commit Message Guidelines

Use clear, descriptive commit messages:

```
🐛 Fix order validation logic in OrdersController

- Add null checks for customer validation
- Improve error messages for stock validation
- Add unit tests for edge cases
- Resolves #123
```

**Commit Types:**
- 🐛 `:bug:` - Bug fixes
- ✨ `:sparkles:` - New features
- 📚 `:books:` - Documentation
- 🧪 `:test_tube:` - Testing
- ⚡ `:zap:` - Performance improvements
- 🔧 `:wrench:` - Configuration/tooling
- 🚀 `:rocket:` - Deployment/CI/CD

## 🔄 Pull Request Process

### 1. Before Creating a PR
- [ ] Create an issue to discuss the change (for large features)
- [ ] Fork the repository and create a feature branch
- [ ] Follow our coding standards
- [ ] Write/update tests
- [ ] Update documentation

### 2. Creating the PR
- [ ] Use our [PR template](.github/PULL_REQUEST_TEMPLATE.md)
- [ ] Link to related issues
- [ ] Provide clear description of changes
- [ ] Include test results
- [ ] Add screenshots/demos if applicable

### 3. Review Process
- [ ] All CI checks must pass
- [ ] At least one approval required
- [ ] Address all review feedback
- [ ] Rebase/squash commits if requested

### 4. After Merge
- [ ] Delete feature branch
- [ ] Monitor deployment (if applicable)
- [ ] Update related documentation

## 🌟 Recognition

Contributors will be recognized in:
- Repository contributors list
- Release notes for significant contributions
- Project documentation acknowledgments

## ❓ Getting Help

### Questions & Discussions
- 💬 [GitHub Discussions](../../discussions) - General questions and ideas
- ❓ [Issue Templates](.github/ISSUE_TEMPLATE/) - Specific problems
- 📖 [Documentation](README.md) - Project overview and guides

### Development Help
- 🚀 [Development Workflow](docs/development/DEVELOPMENT-WORKFLOW.md) - Daily development process
- 🧪 [Testing Strategy](docs/development/TESTING-STRATEGY.md) - Comprehensive testing guide
- 🏗️ [Monorepo Guide](.github/MONOREPO-GUIDE.md) - Project structure details

### Quick References
- API Development: See [copilot-instructions.md](.github/copilot-instructions.md)
- CI/CD Pipeline: See [copilot-cicd-context.md](.github/copilot-cicd-context.md)
- Project Status: Run `.\Manage-Project.ps1 status`

## 🤝 Code of Conduct

We are committed to providing a welcoming and inspiring community for all. Please read and follow our Code of Conduct:

### Our Standards
- **Be respectful** and inclusive
- **Be constructive** in feedback and discussions
- **Be patient** with newcomers and different experience levels
- **Be collaborative** and help others learn
- **Be professional** in all interactions

### Unacceptable Behavior
- Harassment, discrimination, or offensive comments
- Personal attacks or inflammatory language
- Spam, trolling, or disruptive behavior
- Sharing private information without permission
- Any other conduct that would be inappropriate in a professional setting

### Reporting
Report any unacceptable behavior by:
- Creating a private issue with the `conduct` label
- Contacting project maintainers directly
- Using GitHub's reporting features

## 📄 License

By contributing to this project, you agree that your contributions will be licensed under the same license as the project (see [LICENSE](LICENSE) file).

---

**Thank you for contributing to the Fabrikam Project!** 🎉

Your contributions help make this project a valuable learning resource for the .NET community. Whether you're fixing a small bug or adding a major feature, every contribution is appreciated and helps improve the project for everyone.

For questions about contributing, please don't hesitate to ask in [Discussions](../../discussions) or create an issue using our templates.
