# 🚀 Performance Troubleshooting Guide

## 📋 Overview

This guide captures the comprehensive troubleshooting steps used to diagnose and resolve VS Code performance issues, including keyboard delays, memory usage, and system freezing.

## 🔍 Quick Diagnosis Commands

### Memory Analysis
```powershell
# VS Code Memory Usage (detailed breakdown)
Get-Process -Name "Code" -ErrorAction SilentlyContinue | 
    Sort-Object WorkingSet -Descending | 
    ForEach-Object {
        $memMB = [math]::Round($_.WorkingSet / 1MB, 1)
        Write-Host "$($_.ProcessName) (PID: $($_.Id)): $memMB MB" -ForegroundColor Cyan
    }

# VS Code Total Memory Summary
$vscode = Get-Process -Name "Code" -ErrorAction SilentlyContinue
if ($vscode) {
    $totalMemory = ($vscode | Measure-Object WorkingSet -Sum).Sum / 1MB
    $processCount = $vscode.Count
    Write-Host "VS Code: $processCount processes using $([math]::Round($totalMemory, 2)) MB"
}

# System Memory Overview
$memory = Get-WmiObject -Class Win32_OperatingSystem
$totalGB = [math]::Round($memory.TotalVisibleMemorySize / 1MB, 2)
$freeGB = [math]::Round($memory.FreePhysicalMemory / 1MB, 2)
$usedGB = $totalGB - $freeGB
$usagePercent = [math]::Round(($usedGB / $totalGB) * 100, 1)
Write-Host "System Memory: $usagePercent% used ($usedGB GB / $totalGB GB)"
```

### Top Memory Consumers
```powershell
# Top 10 processes by memory usage
Get-Process | Sort-Object WorkingSet -Descending | Select-Object -First 10 | 
    Format-Table Name, Id, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet / 1MB, 1)}} -AutoSize

# Memory-intensive background processes
Get-Process | Where-Object {$_.WorkingSet -gt 100MB} | 
    Sort-Object WorkingSet -Descending |
    Format-Table Name, Id, @{Name="Memory(MB)";Expression={[math]::Round($_.WorkingSet / 1MB, 1)}} -AutoSize
```

### Extension Analysis
```powershell
# List all VS Code extensions
code --list-extensions

# Count extensions by category
$extensions = code --list-extensions
Write-Host "Total Extensions: $($extensions.Count)"
$extensions | Where-Object {$_ -like "*azure*"} | Measure-Object | ForEach-Object {Write-Host "Azure Extensions: $($_.Count)"}
$extensions | Where-Object {$_ -like "*python*"} | Measure-Object | ForEach-Object {Write-Host "Python Extensions: $($_.Count)"}
$extensions | Where-Object {$_ -like "*github*"} | Measure-Object | ForEach-Object {Write-Host "GitHub Extensions: $($_.Count)"}
```

## 🔧 Windows Defender Analysis

### Real-time Protection Status
```powershell
# Windows Defender preferences and status
Get-MpPreference | Select-Object DisableRealtimeMonitoring, 
    @{Name="ExclusionPaths";Expression={$_.ExclusionPath -join ", "}},
    @{Name="RealTimeEnabled";Expression={-not $_.DisableRealtimeMonitoring}}

# Defender process memory usage
Get-Process -Name "*Defender*", "*MsMpEng*", "*SecurityHealthService*" -ErrorAction SilentlyContinue | 
    ForEach-Object {
        $memMB = [math]::Round($_.WorkingSet / 1MB, 1)
        Write-Host "$($_.ProcessName): $memMB MB" -ForegroundColor Yellow
    }
```

### Adding Performance Exclusions
```powershell
# Add VS Code and development folders to exclusions (requires admin)
Add-MpPreference -ExclusionPath "C:\Users\$env:USERNAME\AppData\Local\Programs\Microsoft VS Code" -Force
Add-MpPreference -ExclusionPath "C:\Users\$env:USERNAME\1Repositories" -Force

# Verify exclusions were added
Get-MpPreference | Select-Object -ExpandProperty ExclusionPath
```

## 💾 Disk Performance Analysis

### I/O Performance Monitoring
```powershell
# Disk read/write performance
Get-Counter "\LogicalDisk(*)\Avg. Disk sec/Read", "\LogicalDisk(*)\Avg. Disk sec/Write" -SampleInterval 1 -MaxSamples 5 |
    ForEach-Object {
        $_.CounterSamples | Where-Object {$_.InstanceName -eq "c:"} | 
        ForEach-Object {
            $metric = if ($_.Path -like "*Read*") {"Read"} else {"Write"}
            $timeMs = [math]::Round($_.CookedValue * 1000, 2)
            Write-Host "Disk $metric Time: $timeMs ms" -ForegroundColor $(if ($timeMs -gt 50) {"Red"} else {"Green"})
        }
    }

# Disk queue length (should be < 2 for good performance)
Get-Counter "\LogicalDisk(C:)\Current Disk Queue Length" -SampleInterval 1 -MaxSamples 3
```

### File System Performance Test
```powershell
# Quick file I/O test
$testFile = "$env:TEMP\perf-test.tmp"
$testData = "x" * 1MB
Measure-Command { 
    $testData | Out-File $testFile -Encoding UTF8
    Get-Content $testFile | Out-Null
    Remove-Item $testFile -Force
} | ForEach-Object { Write-Host "File I/O Test: $($_.TotalMilliseconds) ms" }
```

## 🎯 Performance Optimization Steps

### 1. VS Code Memory Optimization
```powershell
# Disable heavy extensions temporarily
code --disable-extension ms-python.python
code --disable-extension octref.vetur  
code --disable-extension ms-playwright.playwright

# List currently disabled extensions
code --list-extensions --show-versions | Where-Object {$_ -like "*disabled*"}
```

### 2. Process Cleanup
```powershell
# Find and close memory-heavy processes
$heavyProcesses = Get-Process | Where-Object {
    $_.WorkingSet -gt 200MB -and 
    $_.ProcessName -notin @("Code", "chrome", "firefox", "devenv")
}

$heavyProcesses | ForEach-Object {
    $memMB = [math]::Round($_.WorkingSet / 1MB, 1)
    Write-Host "Heavy process: $($_.ProcessName) - $memMB MB (PID: $($_.Id))"
}

# Safely close specific processes (example: SnagitCapture)
# Get-Process -Name "SnagitCapture" -ErrorAction SilentlyContinue | Stop-Process -Force
```

### 3. System Memory Optimization
```powershell
# Check memory compression status
Get-WmiObject -Class Win32_PerfRawData_PerfOS_Memory | 
    Select-Object @{Name="MemoryCompression(MB)";Expression={[math]::Round($_.PoolPagedBytes / 1MB, 1)}}

# Clear system memory caches (if available)
if (Get-Command "Clear-RecycleBin" -ErrorAction SilentlyContinue) {
    Clear-RecycleBin -Force -Confirm:$false
}
```

## 📊 Performance Benchmarks

### Baseline Measurements
- **Good VS Code Memory Usage**: < 2GB total across all processes
- **Good System Memory Usage**: < 80% total system memory
- **Good Disk Response Time**: < 20ms for read/write operations
- **Good Disk Queue Length**: < 2 average queue depth

### Red Flags
- **VS Code Memory**: > 3GB (indicates memory leak or too many extensions)
- **System Memory**: > 90% (indicates memory pressure/swapping)
- **Disk Response**: > 50ms (indicates disk bottleneck)
- **Process Count**: > 20 VS Code processes (indicates runaway processes)

## 🔄 Daily Health Check Script

```powershell
# Save as: Quick-PerformanceCheck.ps1
Write-Host "=== Daily Performance Health Check ===" -ForegroundColor Green

# VS Code Check
$vscode = Get-Process -Name "Code" -ErrorAction SilentlyContinue
if ($vscode) {
    $vsMemory = ($vscode | Measure-Object WorkingSet -Sum).Sum / 1MB
    $vsStatus = if ($vsMemory -lt 2048) {"✅ Good"} elseif ($vsMemory -lt 3072) {"⚠️ High"} else {"❌ Critical"}
    Write-Host "VS Code Memory: $([math]::Round($vsMemory, 1)) MB $vsStatus"
}

# System Memory Check  
$memory = Get-WmiObject Win32_OperatingSystem
$usagePercent = [math]::Round((1 - ($memory.FreePhysicalMemory / $memory.TotalVisibleMemorySize)) * 100, 1)
$memStatus = if ($usagePercent -lt 80) {"✅ Good"} elseif ($usagePercent -lt 90) {"⚠️ High"} else {"❌ Critical"}
Write-Host "System Memory: $usagePercent% $memStatus"

# Disk Performance Check
$diskTime = (Get-Counter "\LogicalDisk(C:)\Avg. Disk sec/Read" -SampleInterval 1 -MaxSamples 1).CounterSamples[0].CookedValue * 1000
$diskStatus = if ($diskTime -lt 20) {"✅ Good"} elseif ($diskTime -lt 50) {"⚠️ Slow"} else {"❌ Critical"}
Write-Host "Disk Response: $([math]::Round($diskTime, 1)) ms $diskStatus"

Write-Host "Health check complete!" -ForegroundColor Green
```

## 🚨 Emergency Performance Reset

### Nuclear Option: Complete Reset
```powershell
# 1. Close VS Code completely
Get-Process -Name "Code" -ErrorAction SilentlyContinue | Stop-Process -Force

# 2. Clear VS Code workspace cache
$vscodeCache = "$env:APPDATA\Code\User\workspaceStorage"
if (Test-Path $vscodeCache) {
    Write-Host "Clearing VS Code workspace cache..."
    Remove-Item "$vscodeCache\*" -Recurse -Force -ErrorAction SilentlyContinue
}

# 3. Restart with minimal extensions
code --disable-extensions

# 4. Re-enable essential extensions one by one
# code --enable-extension ms-vscode.vscode-github-pullrequest
# code --enable-extension github.copilot
```

## 📝 Troubleshooting Log Template

When experiencing performance issues, capture this information:

```
Date/Time: ___________
Issue: _______________

VS Code Memory: ______ MB (______ processes)
System Memory: ______% used  
Disk Response: ______ ms
Extensions: ______ total (______ Azure, ______ Python)

Windows Defender:
- Real-time protection: [ON/OFF]
- Exclusions configured: [YES/NO]

Background Processes > 200MB:
- Process 1: ____________
- Process 2: ____________

Resolution Steps Taken:
1. ________________________
2. ________________________
3. ________________________

Outcome: ___________________
```

## ✅ Success Metrics

After applying optimizations, you should see:
- **VS Code Memory**: Reduced to < 2GB
- **System Memory**: Usage < 80%
- **Keyboard Responsiveness**: No delays typing
- **File Operations**: Quick saves/loads
- **Extension Load Time**: < 5 seconds on startup

---

**💡 Pro Tips:**
- Run the daily health check script before starting development work
- Add development folders to Windows Defender exclusions immediately on new machines
- Disable unused extensions rather than uninstalling (easier to re-enable)
- Monitor VS Code process count - more than 15 processes usually indicates issues
- Keep system memory usage below 80% for optimal performance

**🔗 Related Documentation:**
- [Development Workflow](DEVELOPMENT-WORKFLOW.md)
- [Testing Strategy](../TESTING-STRATEGY.md)
- [GitHub Issues Setup](GITHUB-ISSUES-SETUP.md)
