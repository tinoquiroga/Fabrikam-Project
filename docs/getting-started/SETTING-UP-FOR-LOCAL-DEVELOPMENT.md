# 🚀 Setting Up for Local Development

This guide provides step-by-step instructions for setting up your development environment to contribute to the Fabrikam modular home business platform. Whether you're a first-time contributor or setting up a new workstation, this guide will get you up and running quickly.

## 📋 **Prerequisites Overview**

The Fabrikam project is a **.NET 9.0** business platform with **Azure integration** and **AI-powered tools**. You'll need:

- **Development Environment**: Visual Studio Code with .NET development tools
- **Runtime & SDK**: .NET 9.0 SDK and supporting tools
- **Source Control**: Git and GitHub CLI for repository management
- **AI Integration**: GitHub Copilot and Model Context Protocol (MCP) servers
- **Azure Tools**: Azure CLI and development tools for cloud integration
- **Package Management**: PowerShell 7 for automation scripts

## 🛠️ **Required Software Installation**

### **Core Development Tools**

#### **1. Visual Studio Code**

```powershell
# Install VS Code
winget install --exact Microsoft.VisualStudioCode

# Alternative: Download from https://code.visualstudio.com/
```

#### **2. .NET 9.0 SDK** (Primary requirement)

```powershell
# Install .NET 9.0 SDK
winget install --exact Microsoft.DotNet.SDK.9

# Verify installation
dotnet --version
# Should show: 9.0.x
```

#### **2a. HTTPS Development Certificate Setup** (Required for HTTPS endpoints)

The Fabrikam API runs on HTTPS by default (`https://localhost:7297`). You need to trust the ASP.NET Core development certificate to avoid browser warnings and connection issues.

**🔐 Trust the Development Certificate:**

```powershell
# Trust the HTTPS development certificate
dotnet dev-certs https --trust

# You'll see a Windows security dialog - click "Yes" to trust the certificate
```

**✅ Verify HTTPS Certificate:**

```powershell
# Check certificate status
dotnet dev-certs https --check --trust

# Should show: "A trusted https certificate is available"
```

**🔧 Troubleshooting Certificate Issues:**

If you see certificate warnings or connection errors:

```powershell
# Clear existing certificates and create new ones
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# For persistent issues, reset completely:
dotnet dev-certs https --clean
dotnet dev-certs https --trust --verbose
```

**🌐 Browser Testing:**

After trusting the certificate, test in your browser:

1. Navigate to: `https://localhost:7297` (when API server is running)
2. Should show no security warnings
3. Browser should show a "lock" icon indicating secure connection

**⚠️ Common Certificate Issues:**

- **"The ASP.NET Core developer certificate is not trusted"** - Run `dotnet dev-certs https --trust`
- **Browser security warnings** - Certificate wasn't properly trusted
- **API calls fail with SSL errors** - MCP server can't connect to API due to untrusted certificate
- **VS Code REST client errors** - Certificate trust issues in development

**💡 Why This Matters:**

- **API Security**: Fabrikam API uses HTTPS for secure communication
- **MCP Integration**: MCP server needs to trust the API certificate for tool calls
- **Development Experience**: Eliminates browser warnings and connection errors
- **Production Readiness**: Ensures HTTPS works correctly before deployment

#### **3. Development Directory Setup**

```powershell
# Create a dedicated development directory
New-Item -ItemType Directory -Path "C:\Dev" -Force

# Navigate to your development directory
cd "C:\Dev"

# Configure the development directory as a safe directory for git
# This prevents git security warnings and ownership issues
git config --global --add safe.directory "C:\Dev"
git config --global --add safe.directory "C:\Dev/*"

# Verify safe directory configuration
git config --global --get-all safe.directory
```

**📁 Directory Structure Best Practices:**

- **`C:\Dev`** - Main development folder (avoid spaces in path)
- **Short path** - Prevents Windows path length issues
- **Dedicated location** - Separate from user documents/desktop
- **Safe directory** - Allows git operations without ownership warnings

#### **4. Git & GitHub CLI**

```powershell
# Install Git
winget install --exact Git.Git

# Install GitHub CLI (required for issue management)
winget install --exact GitHub.cli

# Verify installations
git --version
gh --version
```

#### **4a. Git Configuration** (First-time setup)

**Configure your identity:**

```powershell
# Set your name and email (use your GitHub account email)
git config --global user.name "Your Full Name"
git config --global user.email "your.email@domain.com"

# Verify configuration
git config --global --list
```

**Configure essential settings:**

```powershell
# Set default branch name to 'main'
git config --global init.defaultBranch main

# Enable credential storage (Windows Credential Manager)
git config --global credential.helper manager

# Set line ending handling (important for cross-platform work)
git config --global core.autocrlf true

# Set default pull behavior (recommended)
git config --global pull.rebase false

# Set default push behavior
git config --global push.default simple

# Enable colored output
git config --global color.ui auto
```

**GitHub CLI Authentication:**

```powershell
# Authenticate with GitHub (opens browser)
gh auth login

# Follow prompts:
# - Choose GitHub.com
# - Choose HTTPS
# - Authenticate via web browser
# - Choose your preferred editor (VS Code)

# Verify authentication
gh auth status
```

**SSH Key Setup (Optional but Recommended):**

```powershell
# Generate SSH key (if you don't have one)
ssh-keygen -t ed25519 -C "your.email@domain.com"

# Start SSH agent
Start-Service ssh-agent

# Add SSH key to agent
ssh-add ~/.ssh/id_ed25519

# Copy public key to clipboard
Get-Content ~/.ssh/id_ed25519.pub | Set-Clipboard

# Then add the key to GitHub:
# 1. Go to GitHub.com → Settings → SSH and GPG keys
# 2. Click "New SSH key"
# 3. Paste the key from clipboard
# 4. Give it a descriptive title

# Test SSH connection
ssh -T git@github.com
```

#### **5. PowerShell 7**

```powershell
# Install PowerShell 7 (for project automation scripts)
winget install --exact Microsoft.PowerShell

# Verify installation
pwsh --version
```

### **Azure Development Tools**

#### **6. Azure CLI**

```powershell
# Install Azure CLI
winget install --exact Microsoft.AzureCLI

# Verify installation
az --version
```

#### **7. Azure Developer CLI**

```powershell
# Install Azure Developer CLI (for azd deployment)
winget install --exact Microsoft.Azd

# Verify installation
azd version
```

## 🔌 **VS Code Extensions Setup**

### **Essential Extensions** (Required)

Install these extensions for core .NET development:

```powershell
# Core .NET Development
code --install-extension ms-dotnettools.csdevkit --force
code --install-extension ms-dotnettools.csharp --force
code --install-extension ms-dotnettools.vscode-dotnet-runtime --force

# GitHub Copilot (Required for MCP development)
code --install-extension github.copilot --force
code --install-extension github.copilot-chat --force

# GitHub Integration
code --install-extension github.vscode-pull-request-github --force
code --install-extension github.vscode-github-actions --force

# API Testing
code --install-extension humao.rest-client --force

# Azure Development
code --install-extension ms-azuretools.azure-dev --force
code --install-extension ms-azuretools.vscode-bicep --force
code --install-extension ms-azuretools.vscode-azure-github-copilot --force
```

### **Configuration Support** (Recommended)

```powershell
# Development Quality of Life
code --install-extension editorconfig.editorconfig --force
code --install-extension ms-vscode.powershell --force
code --install-extension redhat.vscode-yaml --force
```

**💡 Extension Installation Notes:**

- **`--force` flag**: Ensures latest version installation and overwrites any existing versions
- **Auto-updates**: VS Code updates extensions automatically, but `--force` guarantees fresh installation
- **Setup scenarios**: Especially useful for new workstations, corrupted extensions, or version conflicts
- **CI/CD compatibility**: Makes setup scripts more reliable and reproducible

### **⚠️ Extensions to AVOID**

The project has identified extensions that cause UI freezes and performance issues. **Do NOT install these**:

```text
❌ yzhang.markdown-all-in-one     # Creates phantom files
❌ eamodio.gitlens                # Heavy git processing causes freezes
❌ usernamehw.errorlens           # Constant error processing
❌ ms-python.python               # Not needed for .NET development
❌ Heavy Azure extensions         # Only install specific ones listed above
```

## ⚡ **Performance Optimization**

### **Windows Defender Exclusions** (Highly Recommended)

Windows Defender can significantly slow down .NET builds by scanning every compiled file. Add these exclusions for better performance:

#### **Automatic Setup (Run as Administrator)**

```powershell
# Open PowerShell as Administrator and run:

# Exclude your development folder (adjust path as needed)
Add-MpPreference -ExclusionPath "C:\Dev"

# Exclude common .NET build folders globally
Add-MpPreference -ExclusionPath "$env:USERPROFILE\.nuget"
Add-MpPreference -ExclusionPath "$env:PROGRAMFILES\dotnet"
Add-MpPreference -ExclusionPath "$env:PROGRAMFILES(X86)\Microsoft Visual Studio"

# Exclude common development processes
Add-MpPreference -ExclusionProcess "dotnet.exe"
Add-MpPreference -ExclusionProcess "MSBuild.exe"
Add-MpPreference -ExclusionProcess "VBCSCompiler.exe"
Add-MpPreference -ExclusionProcess "node.exe"
Add-MpPreference -ExclusionProcess "Code.exe"

# Verify exclusions
Get-MpPreference | Select-Object -ExpandProperty ExclusionPath
Get-MpPreference | Select-Object -ExpandProperty ExclusionProcess
```

#### **Manual Setup via Windows Security**

If you prefer GUI setup:

1. **Open Windows Security** → Virus & threat protection
2. **Manage settings** under "Virus & threat protection settings"
3. **Add exclusions** → Choose exclusion type:

**Folder Exclusions:**

- `C:\Dev` (or your development folder)
- `%USERPROFILE%\.nuget`
- `%PROGRAMFILES%\dotnet`

**Process Exclusions:**

- `dotnet.exe`
- `MSBuild.exe`
- `VBCSCompiler.exe`
- `Code.exe`

#### **Project-Specific Exclusions**

For the Fabrikam project specifically:

```powershell
# Add Fabrikam project folder (adjust path)
Add-MpPreference -ExclusionPath "C:\Dev\Fabrikam-Project"

# Common .NET build patterns that get heavily scanned
Add-MpPreference -ExclusionPath "C:\Dev\Fabrikam-Project\*\bin"
Add-MpPreference -ExclusionPath "C:\Dev\Fabrikam-Project\*\obj"
```

**⚠️ Security Note:** Only exclude folders you trust. Development folders and .NET tools are generally safe, but be cautious with system-wide exclusions.

### **Additional Performance Tips**

- **SSD Storage**: Keep your development projects on SSD storage
- **RAM**: 16GB+ recommended for smooth .NET 9 development
- **Git**: Use Git for Windows with credential manager for faster operations

## 🏗️ **Model Context Protocol (MCP) Servers Setup**

The Fabrikam project integrates with multiple MCP servers for AI-enhanced development:

### **1. Microsoft Docs MCP**

```powershell
# This will be automatically available through GitHub Copilot
# when working with the project - no separate installation needed
```

### **2. Azure MCP Server**

```powershell
# Integrated with Azure CLI authentication
# Configure after repository setup (covered below)
```

### **3. GitHub MCP Server**

```powershell
# Integrated with GitHub CLI
# Configure after repository setup (covered below)
```

### **4. Custom Fabrikam MCP Server**

```powershell
# This is the project's own MCP server (FabrikamMcp)
# Runs locally during development at http://localhost:5000
# Configuration handled through GitHub Copilot or Claude Desktop
```

> **Important**: MCP servers are **NOT** configured in VS Code settings. They integrate through:
>
> - **GitHub Copilot**: Automatically detects available MCP servers
> - **Claude Desktop**: Configured in Claude's settings (if using Claude)
> - **Direct Integration**: The project's FabrikamMcp server runs independently

## 📁 **Repository Setup**

### **Fork and Clone the Repository**

#### **1. Fork the Repository**

Visit [https://github.com/davebirr/Fabrikam-Project](https://github.com/davebirr/Fabrikam-Project) and click **"Fork"** to create your own copy.

#### **2. Clone Your Fork**

```powershell
# Navigate to your development directory
cd "C:\Dev"  # or wherever you keep projects

# Clone your fork (replace YOUR-USERNAME)
git clone https://github.com/YOUR-USERNAME/Fabrikam-Project --origin fork
cd Fabrikam-Project

# Add the original repository as upstream
git remote add upstream https://github.com/davebirr/Fabrikam-Project

# Verify remotes
git remote -v
# Should show:
# fork     https://github.com/YOUR-USERNAME/Fabrikam-Project (fetch)
# fork     https://github.com/YOUR-USERNAME/Fabrikam-Project (push)
# upstream https://github.com/davebirr/Fabrikam-Project (fetch)
# upstream https://github.com/davebirr/Fabrikam-Project (push)
```

#### **3. Set Up Development Branch**

```powershell
# Switch to the main branch and ensure it's up to date
git checkout main
git pull upstream main

# Create your feature branch from main
git checkout -b feature/your-feature-name

# Example:
git checkout -b feature/add-customer-dashboard
```

## 🔐 **Authentication Setup**

### **1. GitHub CLI Authentication**

```powershell
# Authenticate with GitHub
gh auth login

# Choose:
# - GitHub.com
# - HTTPS
# - Yes (authenticate Git with GitHub credentials)
# - Login with web browser

# Verify authentication
gh auth status
```

### **2. Azure CLI Authentication** (Optional but recommended)

```powershell
# Login to Azure (for cloud development/deployment)
az login

# Set your subscription (replace with your subscription ID)
az account set --subscription "YOUR-SUBSCRIPTION-ID"

# Verify authentication
az account show
```

## 🧪 **Project Verification**

### **1. Build the Solution**

```powershell
# From the repository root
dotnet build Fabrikam.sln

# Should succeed with no errors
```

### **2. Run Tests**

```powershell
# Run the development test suite
.\Test-Development.ps1 -Quick

# Should show: All tests passing ✅
```

### **3. Start the Development Servers**

#### **Terminal 1: API Server**

```powershell
# Start the main API server
dotnet run --project FabrikamApi\src\FabrikamApi.csproj

# Should start on: https://localhost:7297
```

#### **Terminal 2: MCP Server**

```powershell
# Start the MCP server
dotnet run --project FabrikamMcp\src\FabrikamMcp.csproj

# Should start on: http://localhost:5000
```

### **4. Test API Endpoints**

Open `api-tests.http` in VS Code and test the endpoints:

```http
### Test basic API connectivity
GET https://localhost:7297/api/products
```

Click **"Send Request"** - you should see product data returned.

## 🔧 **Project Configuration**

### **VS Code Workspace Settings**

The project includes optimized VS Code settings in `.vscode/settings.json`. Key configurations:

```json
{
  "dotnet.defaultSolution": "Fabrikam.sln",
  "omnisharp.enableAsyncCompletion": false,
  "files.autoSave": "off",
  "extensions.autoUpdate": false
}
```

These settings are automatically applied when you open the project.

### **Environment Variables**

Copy the environment template:

```powershell
copy .env.example .env
```

Edit `.env` with your specific settings (most defaults work for local development).

## 🚀 **Development Workflow**

### **Daily Development Process**

```powershell
# 1. Update your local repository
git checkout main
git pull upstream main

# 2. Create/switch to your feature branch
git checkout feature/your-feature-name

# 3. Start development servers (use VS Code tasks)
# Ctrl+Shift+P → "Tasks: Run Task" → "🚀 Start API Server"
# Ctrl+Shift+P → "Tasks: Run Task" → "🤖 Start MCP Server"

# 4. Make your changes and test
.\Test-Development.ps1 -Quick

# 5. Commit and push
git add .
git commit -m "feat: add your feature description"
git push fork feature/your-feature-name
```

### **Creating Pull Requests**

```powershell
# Create PR using GitHub CLI
gh pr create --title "feat: Your Feature Description" --body "Detailed description of changes"

# Or use the GitHub web interface
```

## 🎯 **Project Structure Understanding**

```
Fabrikam-Project/
├── FabrikamApi/          # Main ASP.NET Core Web API
│   ├── src/              # Source code
│   │   ├── Controllers/  # API controllers
│   │   ├── Services/     # Business logic
│   │   └── Data/         # Data models and seed data
│   └── infra/            # Azure infrastructure (Bicep)
├── FabrikamMcp/          # Model Context Protocol server
│   ├── src/              # MCP server implementation
│   └── infra/            # MCP infrastructure
├── FabrikamTests/        # Comprehensive test suite
├── docs/                 # Documentation
└── scripts/              # Automation scripts
```

## 🔍 **Troubleshooting**

### **Common Issues**

#### **Build Errors**

```powershell
# Clear build artifacts
dotnet clean
dotnet restore
dotnet build
```

#### **VS Code Extension Issues**

```powershell
# Run the extension cleanup script
.\scripts\Disable-ProblematicExtensions.ps1
```

#### **Port Conflicts**

- API Server: `https://localhost:7297`
- MCP Server: `http://localhost:5000`

Kill any processes using these ports:

```powershell
# Find and kill processes on ports
netstat -ano | findstr 7297
taskkill /PID [PID_NUMBER] /F
```

#### **HTTPS Certificate Issues**

**Symptoms:**
- "The ASP.NET Core developer certificate is not trusted" warning
- Browser security warnings when accessing `https://localhost:7297`
- API calls failing with SSL/TLS errors
- MCP server unable to connect to API

**Solutions:**

```powershell
# Basic certificate trust (try this first)
dotnet dev-certs https --trust

# If that doesn't work, reset certificates completely
dotnet dev-certs https --clean
dotnet dev-certs https --trust

# Verify certificate status
dotnet dev-certs https --check --trust
# Should show: "A trusted https certificate is available"

# For persistent issues, use verbose mode
dotnet dev-certs https --clean
dotnet dev-certs https --trust --verbose
```

**Alternative Solution - Use HTTP Instead:**

If certificate issues persist, you can temporarily use HTTP:

```powershell
# Start API with HTTP profile instead
dotnet run --project FabrikamApi\src\FabrikamApi.csproj --launch-profile http

# Update MCP configuration to use HTTP
# Edit FabrikamMcp/src/appsettings.Development.json:
# "BaseUrl": "http://localhost:7296"  // Note port 7296 for HTTP
```

**Browser Testing:**
After fixing certificates, test in browser:
1. Navigate to `https://localhost:7297` (with API running)
2. Should show no security warnings
3. Lock icon should appear in address bar

#### **Git Authentication Issues**

```powershell
# Re-authenticate GitHub CLI
gh auth logout
gh auth login
```

### **Getting Help**

1. **Documentation**: Check `docs/` folder for detailed guides
2. **Issues**: Search existing GitHub issues
3. **Discussions**: Use GitHub Discussions for questions
4. **Test Suite**: Run `.\Test-Development.ps1 -Verbose` for diagnostics

## 🎉 **You're Ready!**

Once you've completed this setup:

✅ **Development Environment**: VS Code with optimized settings  
✅ **Build System**: .NET 9.0 with all dependencies  
✅ **HTTPS Certificate**: Developer certificate trusted for secure connections  
✅ **Source Control**: Git with proper remote configuration  
✅ **AI Tools**: GitHub Copilot with MCP integration  
✅ **Azure Tools**: CLI and development tools configured  
✅ **Testing**: Automated test suite running  
✅ **Project Understanding**: Familiar with structure and workflow

You're now ready to contribute to the Fabrikam modular home business platform!

**Next Steps**:

- Review the [Business Model Summary](../../BUSINESS-MODEL-SUMMARY.md)
- Explore the [API Documentation](https://localhost:7297/swagger)
- Check out [Issue #9](https://github.com/davebirr/Fabrikam-Project/issues/9) for the Business Simulator feature
- Join the development workflow with your first contribution!

---

> **💡 Pro Tip**: Use the VS Code tasks (Ctrl+Shift+P → "Tasks: Run Task") to quickly start servers, run tests, and manage the project. The project includes optimized tasks for common development operations.

**Happy Coding!** 🏗️✨
