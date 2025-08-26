# 🧹 Build Artifact Management

## Overview

Build artifacts (files in `bin` and `obj` directories) can cause VS Code performance issues by consuming excessive memory when monitoring hundreds of small files. This guide explains how to manage these artifacts to maintain optimal development performance.

## The Problem

When .NET projects are built, they create extensive build artifacts:
- **Entity Framework DLLs** (2.57MB each)
- **Swashbuckle UI assets** (2.15MB each) 
- **Hundreds of .NET runtime DLLs**
- **Multiple copies** across Debug/Release builds
- **Test framework artifacts**

VS Code's file watcher monitors these files, causing:
- **1520MB memory spikes** in renderer process
- **5-second UI freezes** during indexing
- **Keyboard input buffering** when memory allocation occurs

## VS Code Configuration

The `.vscode/settings.json` has been configured to exclude build artifacts:

```json
{
  "files.exclude": {
    "**/bin": true,
    "**/obj": true,
    "**/.vs": true
  },
  "files.watcherExclude": {
    "**/bin/**": true,
    "**/obj/**": true,
    "**/.vs/**": true,
    "**/node_modules/**": true,
    "**/.git/**": true
  },
  "search.exclude": {
    "**/bin": true,
    "**/obj": true,
    "**/.vs": true,
    "**/node_modules": true
  }
}
```

**Key Setting**: `files.watcherExclude` prevents VS Code from monitoring build artifacts, eliminating memory spikes.

## Enhanced Test-Development.ps1

The testing script now includes build artifact management:

### New Parameter
```powershell
-CleanArtifacts    # Clean build artifacts after testing
```

### Usage Examples
```powershell
# Clean artifacts after testing
.\Test-Development.ps1 -CleanArtifacts

# Clean build + clean artifacts (recommended)
.\Test-Development.ps1 -CleanBuild

# Quick test with cleanup
.\Test-Development.ps1 -Quick -CleanArtifacts
```

### Automatic Behavior
- **Clean Build** (`-CleanBuild`): Automatically cleans artifacts before building
- **Manual Cleanup** (`-CleanArtifacts`): Cleans artifacts in the finally block
- **Production Testing**: Never cleans artifacts (preserves deployment state)

## Manual Cleanup

If you need to clean artifacts manually:

```powershell
# Remove all bin and obj directories
Get-ChildItem -Path . -Recurse -Directory -Name "bin" | Remove-Item -Recurse -Force
Get-ChildItem -Path . -Recurse -Directory -Name "obj" | Remove-Item -Recurse -Force
```

## Benefits

### Development Performance
- **Eliminates VS Code memory spikes**
- **Prevents 5-second UI freezes**
- **Maintains responsive typing experience**
- **Reduces overall VS Code memory usage**

### Development Workflow
- **No impact on functionality**: Build artifacts are recreated on next build/run
- **Faster file operations**: Less files for VS Code to monitor
- **Cleaner workspace**: Only source code and configuration visible
- **Better Git performance**: Fewer files to check for changes

## When Artifacts Are Recreated

Build artifacts are automatically recreated when you:
- Run `dotnet build`
- Run `dotnet run`
- Use F5 to debug in VS Code
- Run the test suite (which builds before testing)

## Best Practices

1. **Use `-CleanBuild` regularly**: Ensures fresh environment and artifact cleanup
2. **Add `-CleanArtifacts` to daily workflow**: Prevents accumulation of artifacts
3. **Monitor VS Code memory usage**: Should stay under 2GB after cleanup
4. **Check for performance improvements**: Typing should be responsive after cleanup

## Troubleshooting

If VS Code is still slow after artifact cleanup:
1. Restart VS Code to clear any cached file monitoring
2. Check for other large directories in the workspace
3. Verify exclusions are properly configured in `.vscode/settings.json`
4. Run the performance monitoring script from the troubleshooting guide

---

**💡 Pro Tip**: Use `.\Test-Development.ps1 -CleanBuild` as your primary development testing command to maintain optimal performance.
