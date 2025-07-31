# Test-ServerManagement.ps1 - Server Lifecycle Management for Modular Testing
# Extracted from the monolithic Test-Development.ps1 to support the modular architecture

param(
    [string]$ApiProject = "FabrikamApi\src\FabrikamApi.csproj",
    [string]$McpProject = "FabrikamMcp\src\FabrikamMcp.csproj",
    [string]$SolutionFile = "Fabrikam.sln",
    [switch]$Visible,
    [switch]$CleanBuild,
    [switch]$CleanArtifacts,
    [int]$StartupTimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

# Color functions for consistent output
function Write-Success($message) { Write-Host "✅ $message" -ForegroundColor Green }
function Write-Error($message) { Write-Host "❌ $message" -ForegroundColor Red }
function Write-Warning($message) { Write-Host "⚠️ $message" -ForegroundColor Yellow }
function Write-Info($message) { Write-Host "ℹ️ $message" -ForegroundColor Cyan }
function Write-Debug($message) { if ($Verbose) { Write-Host "🔍 $message" -ForegroundColor Gray } }
function Write-Header($message) { 
    Write-Host "`n🚀 $message" -ForegroundColor Yellow
    Write-Host "=" * ($message.Length + 4) -ForegroundColor Yellow
}

function Get-ProcessStatus {
    $apiProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
    Where-Object { 
        try {
            $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
            $cmdLine -like "*FabrikamApi*"
        }
        catch { $false }
    }
    
    $mcpProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
    Where-Object { 
        try {
            $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($_.Id)" -ErrorAction SilentlyContinue).CommandLine
            $cmdLine -like "*FabrikamMcp*"
        }
        catch { $false }
    }
    
    return @{
        ApiRunning = $null -ne $apiProcess
        McpRunning = $null -ne $mcpProcess
        ApiProcess = $apiProcess
        McpProcess = $mcpProcess
    }
}

function Stop-AllServers {
    Write-Header "Stopping All Running Servers"
    
    $status = Get-ProcessStatus
    $stopped = $false
    
    if ($status.ApiRunning) {
        Write-Info "🌐 Stopping API Server (PID: $($status.ApiProcess.Id))"
        try {
            Stop-Process -Id $status.ApiProcess.Id -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
            Write-Success "API Server stopped"
            $stopped = $true
        }
        catch {
            Write-Warning "Error stopping API server: $($_.Exception.Message)"
        }
    }
    
    if ($status.McpRunning) {
        Write-Info "🤖 Stopping MCP Server (PID: $($status.McpProcess.Id))"
        try {
            Stop-Process -Id $status.McpProcess.Id -Force -ErrorAction SilentlyContinue
            Start-Sleep -Seconds 2
            Write-Success "MCP Server stopped"
            $stopped = $true
        }
        catch {
            Write-Warning "Error stopping MCP server: $($_.Exception.Message)"
        }
    }
    
    if (-not $stopped) {
        Write-Info "No servers were running"
    }
    
    # Wait a bit for ports to be released
    Start-Sleep -Seconds 3
}

function Build-Solution {
    param([switch]$Clean)
    
    if ($Clean) {
        Write-Header "Performing Clean Build"
        Write-Info "🧹 Cleaning solution..."
        
        try {
            dotnet clean $SolutionFile --verbosity quiet
            Write-Success "Solution cleaned"
            
            # Also clean build artifacts to prevent VS Code memory issues
            if ($CleanArtifacts) {
                Write-Info "🧹 Cleaning build artifacts..."
                Remove-BuildArtifacts | Out-Null
            }
        }
        catch {
            Write-Error "Clean failed: $($_.Exception.Message)"
            return $false
        }
    }
    else {
        Write-Header "Building Solution"
    }
    
    Write-Info "🔨 Building solution..."
    
    try {
        $buildOutput = dotnet build $SolutionFile --verbosity minimal --nologo 2>&1
        
        # Check if build succeeded
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Solution built successfully"
            return $true
        }
        else {
            Write-Error "Build failed"
            Write-Host "Build output:" -ForegroundColor Gray
            $buildOutput | Write-Host
            return $false
        }
    }
    catch {
        Write-Error "Build error: $($_.Exception.Message)"
        return $false
    }
}

function Remove-BuildArtifacts {
    Write-Header "Cleaning Build Artifacts"
    Write-Info "🧹 Removing bin and obj directories to prevent VS Code memory issues..."
    
    try {
        $binDirs = Get-ChildItem -Path . -Recurse -Directory -Name "bin" -ErrorAction SilentlyContinue
        $objDirs = Get-ChildItem -Path . -Recurse -Directory -Name "obj" -ErrorAction SilentlyContinue
        
        $totalDirs = $binDirs.Count + $objDirs.Count
        
        if ($totalDirs -eq 0) {
            Write-Info "No build artifacts found to clean"
            return $true
        }
        
        Write-Info "📁 Found $($binDirs.Count) bin directories and $($objDirs.Count) obj directories"
        
        # Remove bin directories
        foreach ($binDir in $binDirs) {
            $fullPath = Join-Path $PWD $binDir
            if (Test-Path $fullPath) {
                Remove-Item $fullPath -Recurse -Force -ErrorAction SilentlyContinue
            }
        }
        
        # Remove obj directories  
        foreach ($objDir in $objDirs) {
            $fullPath = Join-Path $PWD $objDir
            if (Test-Path $fullPath) {
                Remove-Item $fullPath -Recurse -Force -ErrorAction SilentlyContinue
            }
        }
        
        Write-Success "Cleaned $totalDirs build artifact directories"
        Write-Info "💡 This prevents VS Code from monitoring hundreds of build files"
        
        return $true
    }
    catch {
        Write-Error "Error cleaning build artifacts: $($_.Exception.Message)"
        return $false
    }
}

function Wait-ForServerStartup {
    param($Url, $ServerName, $MaxWaitSeconds = 30)
    
    Write-Info "⏳ Waiting for $ServerName to start..."
    
    $elapsed = 0
    while ($elapsed -lt $MaxWaitSeconds) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 5 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Success "$ServerName is ready"
                return $true
            }
        }
        catch {
            # Expected during startup
        }
        
        Start-Sleep -Seconds 2
        $elapsed += 2
    }
    
    Write-Warning "$ServerName did not respond within $MaxWaitSeconds seconds"
    return $false
}

function Start-ServersForTesting {
    param(
        [string]$ApiBaseUrl = "https://localhost:7297",
        [string]$McpBaseUrl = "https://localhost:5001",
        [switch]$ApiOnly,
        [switch]$McpOnly
    )
    
    Write-Header "Starting Servers for Testing"
    
    $jobs = @{}
    
    if ($Visible) {
        # Start servers in visible terminals
        if (-not $ApiOnly) {
            Write-Info "🤖 Starting MCP Server in visible terminal..."
            try {
                $mcpProcess = Start-Process -FilePath "powershell" -ArgumentList @(
                    "-NoExit", 
                    "-Command", 
                    "Set-Location '$PWD'; Write-Host '🤖 Starting MCP Server...' -ForegroundColor Cyan; dotnet run --project $McpProject --launch-profile https"
                ) -PassThru
                
                Start-Sleep -Seconds 3
                
                if (Wait-ForServerStartup "$McpBaseUrl/status" "MCP Server" $StartupTimeoutSeconds) {
                    Write-Success "MCP Server started in visible terminal (PID: $($mcpProcess.Id))"
                }
                
                $jobs.McpProcess = $mcpProcess
            }
            catch {
                Write-Error "Failed to start MCP Server: $($_.Exception.Message)"
            }
        }
        
        if (-not $McpOnly) {
            Write-Info "🌐 Starting API Server in visible terminal..."
            try {
                $apiProcess = Start-Process -FilePath "powershell" -ArgumentList @(
                    "-NoExit", 
                    "-Command", 
                    "Set-Location '$PWD'; `$env:ASPNETCORE_ENVIRONMENT = 'Development'; Write-Host '🌐 Starting API Server...' -ForegroundColor Green; dotnet run --project $ApiProject --launch-profile https"
                ) -PassThru
                
                Start-Sleep -Seconds 5
                
                if (Wait-ForServerStartup "$ApiBaseUrl/api/info" "API Server" $StartupTimeoutSeconds) {
                    Write-Success "API Server started in visible terminal (PID: $($apiProcess.Id))"
                }
                
                $jobs.ApiProcess = $apiProcess
            }
            catch {
                Write-Error "Failed to start API Server: $($_.Exception.Message)"
            }
        }
    }
    else {
        # Use background jobs
        if (-not $ApiOnly) {
            Write-Info "🤖 Starting MCP Server in background..."
            try {
                $mcpJob = Start-Job -ScriptBlock {
                    param($Project, $WorkingDir)
                    Set-Location $WorkingDir
                    dotnet run --project $Project --launch-profile https --verbosity quiet
                } -ArgumentList $McpProject, $PWD.Path
                
                if (Wait-ForServerStartup "$McpBaseUrl/status" "MCP Server" $StartupTimeoutSeconds) {
                    Write-Success "MCP Server started (Job ID: $($mcpJob.Id))"
                }
                
                $jobs.McpJob = $mcpJob
            }
            catch {
                Write-Error "Failed to start MCP Server: $($_.Exception.Message)"
            }
        }
        
        if (-not $McpOnly) {
            Write-Info "🌐 Starting API Server in background..."
            try {
                $apiJob = Start-Job -ScriptBlock {
                    param($Project, $WorkingDir)
                    Set-Location $WorkingDir
                    $env:ASPNETCORE_ENVIRONMENT = "Development"
                    dotnet run --project $Project --launch-profile https --verbosity quiet
                } -ArgumentList $ApiProject, $PWD.Path
                
                if (Wait-ForServerStartup "$ApiBaseUrl/api/info" "API Server" $StartupTimeoutSeconds) {
                    Write-Success "API Server started (Job ID: $($apiJob.Id))"
                }
                
                $jobs.ApiJob = $apiJob
            }
            catch {
                Write-Error "Failed to start API Server: $($_.Exception.Message)"
            }
        }
    }
    
    return $jobs
}

function Stop-TestJobs {
    param($Jobs)
    
    Write-Header "Cleaning Up Test Environment"
    
    # Handle background jobs
    if ($Jobs.ApiJob) {
        Write-Info "🌐 Stopping API Server job..."
        Stop-Job -Job $Jobs.ApiJob -ErrorAction SilentlyContinue
        Remove-Job -Job $Jobs.ApiJob -Force -ErrorAction SilentlyContinue
    }
    
    if ($Jobs.McpJob) {
        Write-Info "🤖 Stopping MCP Server job..."
        Stop-Job -Job $Jobs.McpJob -ErrorAction SilentlyContinue
        Remove-Job -Job $Jobs.McpJob -Force -ErrorAction SilentlyContinue
    }
    
    # Handle visible processes
    if ($Jobs.ApiProcess -and -not $Jobs.ApiProcess.HasExited) {
        Write-Info "🌐 Stopping API Server process..."
        try {
            $Jobs.ApiProcess.CloseMainWindow()
            if (-not $Jobs.ApiProcess.WaitForExit(5000)) {
                $Jobs.ApiProcess.Kill()
            }
        }
        catch {
            Write-Warning "Could not cleanly stop API process: $($_.Exception.Message)"
        }
    }
    
    if ($Jobs.McpProcess -and -not $Jobs.McpProcess.HasExited) {
        Write-Info "🤖 Stopping MCP Server process..."
        try {
            $Jobs.McpProcess.CloseMainWindow()
            if (-not $Jobs.McpProcess.WaitForExit(5000)) {
                $Jobs.McpProcess.Kill()
            }
        }
        catch {
            Write-Warning "Could not cleanly stop MCP process: $($_.Exception.Message)"
        }
    }
    
    # Also stop any lingering processes
    Stop-AllServers
    Write-Success "Test environment cleaned up"
}

# Functions are available for dot-sourcing by other scripts
