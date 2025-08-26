# 🔐 Security Notice - Personal Data Protection

## 📋 Overview

This repository has been configured to protect personal data from being committed to source control, especially important for public repositories.

## 🛡️ Protected Files

The following patterns are automatically excluded from git commits:

```gitignore
# Personal/sensitive data files
docs/demo-coe/coe-users-actual.csv
docs/demo-coe/*-actual.csv
**/personal-data.csv
**/sensitive-*.csv
```

## 📂 File Structure

| File Type | Example | Purpose | Source Control |
|-----------|---------|---------|----------------|
| **Template** | `coe-users.csv` | Safe sample data for format reference | ✅ Committed |
| **Actual Data** | `coe-users-actual.csv` | Real personal information | ❌ Gitignored |

## 🔄 Workflow for Handling Personal Data

### For Repository Maintainers:

1. **Never commit real personal data**
2. **Always use template files** with sample data in the repository
3. **Document the process** for creating actual data files
4. **Use descriptive gitignore patterns** to catch sensitive files

### For Users Setting Up Demo:

1. **Copy template to actual file**:
   ```powershell
   Copy-Item "coe-users.csv" "coe-users-actual.csv"
   ```

2. **Edit actual file** with real user information:
   ```powershell
   notepad "coe-users-actual.csv"
   ```

3. **Use actual file** in scripts and provisioning:
   ```powershell
   .\Provision-COE-Users.ps1 -UserDataFile "coe-users-actual.csv"
   ```

4. **Verify protection**:
   ```powershell
   git status  # Should not show *-actual.csv files
   ```

## ✅ Benefits of This Approach

- **🔒 Privacy Protected**: Personal email addresses and names never exposed in public repo
- **📋 Format Clear**: Template shows exact format needed for actual data
- **🔄 Easy Setup**: Simple copy-and-edit process for users
- **⚡ Automated**: Gitignore patterns prevent accidental commits
- **📚 Documented**: Clear instructions for safe handling

## 🚨 What NOT to Do

❌ **Don't** put real email addresses in template files
❌ **Don't** commit files with actual personal data
❌ **Don't** share actual data files via email or chat
❌ **Don't** include personal data in commit messages
❌ **Don't** push personal data even to private branches

## ✅ What TO Do

✅ **Do** use descriptive template data (e.g., "Sample User One")
✅ **Do** document the file creation process clearly
✅ **Do** use gitignore patterns to prevent accidents
✅ **Do** regularly review what's being committed
✅ **Do** educate team members on data protection practices

## 🔍 Verification Commands

Check what files are tracked by git:
```powershell
git ls-files docs/demo-coe/
```

Verify sensitive files are ignored:
```powershell
git check-ignore docs/demo-coe/coe-users-actual.csv
```

Check gitignore is working:
```powershell
git status --ignored
```

## 📞 Questions?

If you're unsure whether a file contains sensitive data:
1. **Ask yourself**: "Would I be comfortable with this data being public?"
2. **When in doubt**: Use the template approach
3. **Review regularly**: Check what's being committed before pushing

---

**🎯 Remember: It's much easier to prevent sensitive data from entering source control than to remove it after the fact!**
