# 🔐 Security and Privacy Guide for COE User Data

## 🎯 Overview

This guide explains how personal data is protected in the COE provisioning system while maintaining usability for workshop organizers.

## 🛡️ Security Approach

### Personal Data Protection

- ✅ **No personal data in source control**: Actual user CSV files are gitignored
- ✅ **Template-based workflow**: Safe templates in source control, actual data stays local
- ✅ **Multiple workshop support**: Easy to create new user lists for different events
- ✅ **Local-only sensitive files**: Personal information never leaves your machine

### File Structure

```
workshops/ws-coe-aug27/
├── coe-users-template.csv     # ✅ Safe template (in source control)
├── coe-users.csv              # ❌ Actual user data (gitignored)
├── Provision-COE-Users.ps1    # ✅ Provisioning script (in source control)
└── Run-COE-Provisioning.ps1   # ✅ Helper script (in source control)
```

## 📝 Workflow for Workshop Organizers

### Setting Up for a New Workshop

1. **Copy the template**:
   ```powershell
   Copy-Item "coe-users-template.csv" "coe-users.csv"
   ```

2. **Edit with actual participant data**:
   ```powershell
   notepad coe-users.csv
   ```

3. **Run the provisioning**:
   ```powershell
   .\Run-COE-Provisioning.ps1
   ```

### For Multiple Workshops

Create workshop-specific files (all gitignored):

```powershell
# Workshop 1
Copy-Item "coe-users-template.csv" "coe-users-workshop1.csv"
# Edit with Workshop 1 participants

# Workshop 2  
Copy-Item "coe-users-template.csv" "coe-users-workshop2.csv"
# Edit with Workshop 2 participants

# Run provisioning for specific workshop
.\Provision-COE-Users.ps1 -UserDataFile "coe-users-workshop1.csv" -TenantDomain "..." -AzureSubscriptionId "..."
```

## 🚫 What's Protected by .gitignore

The following patterns are automatically ignored:

```gitignore
# Personal/sensitive data files
workshops/ws-coe-aug27/coe-users-actual.csv
workshops/ws-coe-aug27/coe-users.csv
workshops/ws-coe-aug27/*-actual.csv
**/personal-data.csv
**/sensitive-*.csv
```

This means:
- ✅ `coe-users.csv` - Your main user data file
- ✅ `coe-users-workshop1.csv` - Workshop-specific files  
- ✅ `coe-users-actual.csv` - Alternative naming
- ✅ Any file ending with `-actual.csv`
- ✅ Any `personal-data.csv` file
- ✅ Any file starting with `sensitive-`

## ✅ Best Practices

### For Workshop Organizers

1. **Keep actual data local**: Never commit files with real participant information
2. **Use descriptive names**: Create workshop-specific CSV files for different events
3. **Test first**: Always run with `-WhatIf` before actual provisioning
4. **Clean up after**: Remove old workshop files after events complete

### For Script Development

1. **Use templates**: Always provide safe template files in source control
2. **Document security**: Clear instructions about what's safe to commit
3. **Multiple patterns**: Use various gitignore patterns to catch different naming conventions
4. **Helpful errors**: Script should guide users to set up data files correctly

## 🔍 Verification Commands

### Check what's ignored
```powershell
git status --ignored
```

### Verify personal files aren't tracked
```powershell
git ls-files | findstr coe-users
# Should only show template files, not actual data
```

### Test gitignore patterns
```powershell
git check-ignore workshops/ws-coe-aug27/coe-users.csv
# Should return the file path if properly ignored
```

## 🆘 Troubleshooting

### "I accidentally committed personal data"

If personal data was committed to git:

1. **Remove from staging**:
   ```powershell
   git reset HEAD workshops/ws-coe-aug27/coe-users.csv
   ```

2. **Remove from git history** (if already committed):
   ```powershell
   git filter-branch --force --index-filter 'git rm --cached --ignore-unmatch workshops/ws-coe-aug27/coe-users.csv' --prune-empty --tag-name-filter cat -- --all
   ```

3. **Force push** (dangerous - coordinate with team):
   ```powershell
   git push origin --force --all
   ```

### "Script can't find user data file"

The script will guide you:
1. It checks for template files
2. Offers to copy template to working file
3. Provides clear instructions for manual setup

### "Want to use different file naming"

You can use any naming pattern that matches the gitignore rules:
- `coe-users-[event-name].csv`
- `[anything]-actual.csv`
- `personal-data.csv`
- `sensitive-[anything].csv`

## 📊 Workshop Management Examples

### Multiple Events
```powershell
# COE Workshop March 2025
Copy-Item "coe-users-template.csv" "coe-users-march2025.csv"
# Edit with March participants

# Security Workshop April 2025
Copy-Item "coe-users-template.csv" "coe-users-security-april.csv" 
# Edit with Security workshop participants

# Partner Event May 2025
Copy-Item "coe-users-template.csv" "coe-users-partners-may.csv"
# Edit with Partner participants
```

### Archive Management
```powershell
# After workshop completion, move to archive folder
mkdir archive-2025
Move-Item "coe-users-march2025.csv" "archive-2025/"
Move-Item "coe-users-security-april.csv" "archive-2025/"
```

---

**🔒 This approach ensures personal data protection while maintaining flexibility for multiple workshops and ease of use for organizers.**
