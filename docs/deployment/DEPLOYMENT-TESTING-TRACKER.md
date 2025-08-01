# 🧪 Deployment Testing Tracker

This document tracks our active Azure deployments for testing the three authentication modes.

## 📋 Active Test Deployments

### 🔓 **Disabled Mode** - rxnmcw (6-character suffix)
- **Suffix**: `rxnmcw` (6-character lowercase - new pattern)
- **Authentication Mode**: `Disabled`
- **Resource Group**: `rg-fabrikam-development-rxnmcw`
- **API URL**: `https://fabrikam-api-development-rxnmcw.azurewebsites.net`
- **MCP URL**: `https://fabrikam-mcp-development-rxnmcw.azurewebsites.net`
- **Status**: ✅ Fixed workflows, ready for testing
- **Notes**: 
  - Uses improved 6-character lowercase suffix
  - Workflows manually fixed with project paths and auth fallbacks
  - GUID-based user tracking for quick demos and POCs

### 🔐 **BearerToken Mode** - New 6-character suffix
- **Suffix**: `xmrbiq` (6-character lowercase - new pattern)
- **Authentication Mode**: `BearerToken`
- **Resource Group**: `rg-fabrikam-development-xmrbiq`
- **API URL**: `https://fabrikam-api-development-xmrbiq.azurewebsites.net`
- **MCP URL**: `https://fabrikam-mcp-development-xmrbiq.azurewebsites.net`
- **Status**: ✅ Fixed workflows, ready for testing
- **Notes**: 
  - Uses improved 6-character lowercase suffix
  - Workflows manually fixed with project paths and auth fallbacks
  - JWT authentication with Key Vault integration

### 🏢 **EntraExternalId Mode** - Planned
- **Suffix**: TBD (will use 6-character lowercase)
- **Authentication Mode**: `EntraExternalId`
- **Status**: 🔄 Not yet deployed
- **Notes**: Requires Entra External ID tenant setup

## 🔧 Workflow Status Summary

| Deployment | API Workflow | MCP Workflow | Issues Fixed |
|------------|--------------|--------------|---------------|
| **rxnmcw** (Disabled) | ✅ Fixed | ✅ Fixed | Project paths, auth fallbacks |
| **xmrbiq** (BearerToken) | ✅ Fixed | ✅ Fixed | Complete |
| **izbD** (Main Branch) | ✅ Active | ✅ Active | Production deployment |

**Cleaned up workflows**: Removed y32g, 2k1f, gcpm, nvxk - no longer needed

## � Merge-Ready Testing Status

**Goal**: Validate all three authentication modes work correctly before merging `feature/phase-1-authentication` → `main`

| Authentication Mode | Status | Deployment Ready | Testing Status |
|-------------------|---------|------------------|----------------|
| **Disabled** | ✅ Active | rxnmcw ready | Ready to test |
| **BearerToken** | ✅ Active | xmrbiq ready | Ready to test |
| **EntraExternalId** | 📋 Planned | TBD | Pending deployment |

## 🎯 Testing Priorities for Main Branch Merge

1. **Disabled Mode (rxnmcw)**: ✅ Ready for immediate testing
2. **BearerToken Mode (xmrbiq)**: ✅ Ready for immediate testing
3. **EntraExternalId Mode**: 📅 Deploy after Disabled/BearerToken validation

## 🛠️ Workflow Status Summary

| Deployment | API Workflow | MCP Workflow | Notes |
|------------|--------------|--------------|-------|
| **rxnmcw** (Disabled) | ✅ Fixed & Ready | ✅ Fixed & Ready | Merge-ready testing |
| **xmrbiq** (BearerToken) | ✅ Fixed & Ready | ✅ Fixed & Ready | Merge-ready testing |
| **izbD** (Main Branch) | ✅ Active | ✅ Active | Production deployment |

**Workflow Cleanup Complete**: 
- ✅ Kept: rxnmcw, xmrbiq (feature testing), izbD (main branch)
- 🗑️ Removed: y32g, 2k1f, gcpm, nvxk (no longer needed)
