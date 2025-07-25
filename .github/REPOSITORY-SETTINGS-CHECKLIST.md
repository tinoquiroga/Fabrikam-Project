# 🔧 GitHub Repository Settings Verification Checklist

Use this checklist to verify your repository is properly configured for collaboration.

## 📍 How to Access Settings
1. Go to your repository: `https://github.com/davebirr/Fabrikam-Project`
2. Click the **Settings** tab (you need admin access)
3. Use the left sidebar to navigate between sections

---

## ✅ Essential Settings Checklist

### 1. General Settings
**Location:** Settings → General

#### Repository Features
- [ ] **Issues** - ✅ Should be **ENABLED**
- [ ] **Projects** - ✅ Should be **ENABLED** (optional but recommended)
- [ ] **Discussions** - ✅ Should be **ENABLED** (for community Q&A)
- [ ] **Wiki** - ⚪ Optional (enable if you want collaborative documentation)

#### Pull Requests
- [ ] **Allow merge commits** - ✅ Recommended: **ENABLED**
- [ ] **Allow squash merging** - ✅ Recommended: **ENABLED**
- [ ] **Allow rebase merging** - ✅ Recommended: **ENABLED**
- [ ] **Always suggest updating pull request branches** - ✅ Recommended: **ENABLED**
- [ ] **Allow auto-merge** - ⚪ Optional
- [ ] **Automatically delete head branches** - ✅ Recommended: **ENABLED**

#### Archives
- [ ] **Include Git LFS objects in archives** - ⚪ Not needed for this project

---

### 2. Access & Security
**Location:** Settings → General → Danger Zone (at bottom)

#### Repository Visibility
- [ ] **Repository is Public** - ✅ Verify this matches your intention
- [ ] **Repository is Private** - ⚪ Or this, depending on your needs

---

### 3. Branch Protection Rules
**Location:** Settings → Branches

#### Main Branch Protection
- [ ] **Branch protection rule exists for `main`**
- [ ] **Require a pull request before merging** - ✅ Recommended: **ENABLED**
  - [ ] **Require approvals** - ✅ Recommended: 1 approval minimum
  - [ ] **Dismiss stale reviews** - ✅ Recommended: **ENABLED**
  - [ ] **Require review from code owners** - ⚪ Optional (if you have CODEOWNERS file)
- [ ] **Require status checks** - ✅ Recommended: **ENABLED**
  - [ ] **Require branches to be up to date** - ✅ Recommended: **ENABLED**
- [ ] **Require conversation resolution** - ✅ Recommended: **ENABLED**
- [ ] **Restrict pushes that create files** - ⚪ Optional
- [ ] **Restrict force pushes** - ✅ Recommended: **ENABLED**
- [ ] **Allow deletions** - ❌ Recommended: **DISABLED**

---

### 4. Collaborators & Teams
**Location:** Settings → Collaborators and teams

#### Access Control
- [ ] **Repository admins** - Verify correct people have admin access
- [ ] **Write access** - Add collaborators who should be able to push directly
- [ ] **Read access** - Add people who should only be able to view private repos

---

### 5. Actions (CI/CD)
**Location:** Settings → Actions → General

#### Actions Permissions
- [ ] **Allow all actions and reusable workflows** - ✅ Recommended for this project
- [ ] **Disable actions** - ❌ Don't select this
- [ ] **Allow select actions** - ⚪ Alternative if you want more control

#### Workflow Permissions
- [ ] **Read and write permissions** - ✅ Needed for your GitHub workflows
- [ ] **Allow GitHub Actions to create and approve pull requests** - ✅ Recommended: **ENABLED**

---

### 6. Webhooks & Integrations
**Location:** Settings → Webhooks

#### Current Webhooks
- [ ] **No unexpected webhooks** - Review any existing webhooks
- [ ] **GitHub Actions webhook** - Should exist automatically

---

### 7. Notifications
**Location:** Settings → Notifications

#### Email Notifications
- [ ] **Configure notification preferences** - Set according to your preference

---

## 🧪 Testing Your Configuration

### Test Issue Templates
1. Go to your repository → **Issues** tab
2. Click **New Issue**
3. Verify you see these templates:
   - [ ] 🐛 Bug Report
   - [ ] ✨ Feature Request  
   - [ ] 📚 Documentation
   - [ ] ❓ Question or Discussion
4. Test creating an issue with one template

### Test Pull Request Template
1. Create a test branch: `git checkout -b test-pr-template`
2. Make a small change and push
3. Create a pull request
4. Verify the PR template appears with the checklist

### Test Discussions (if enabled)
1. Go to your repository → **Discussions** tab
2. Verify discussions are working
3. Create a test discussion

### Test Branch Protection
1. Try to push directly to main (should be blocked if protection is enabled)
2. Create a PR and verify approval requirements work

---

## ⚡ Quick Status Check Commands

Run these commands in your repository to check local configuration:

```bash
# Check your repository URL
git remote -v

# Check current branch protection (if you have GitHub CLI)
gh repo view --json defaultBranchRef

# List GitHub CLI extensions (if installed)
gh extension list
```

---

## 🚨 Common Issues & Solutions

### Issue Templates Not Showing
- **Problem:** Templates don't appear when creating issues
- **Solution:** Ensure Issues are enabled in Settings → General → Features

### Branch Protection Not Working
- **Problem:** Can still push directly to main
- **Solution:** Check Settings → Branches and verify protection rules are active

### Workflows Not Running
- **Problem:** GitHub Actions not triggering
- **Solution:** Check Settings → Actions → General permissions

### Collaborators Can't Access
- **Problem:** Team members can't see/edit repository
- **Solution:** Check Settings → Collaborators and teams

---

## 📞 Need Help?

If you find any issues with these settings:

1. **Create an issue** using the Question template (once Issues are enabled)
2. **Check GitHub documentation:** https://docs.github.com/en/repositories
3. **Review the setup guide:** `.github/SETUP-GITHUB-COLLABORATION.md`

---

**Last Updated:** July 25, 2025  
**For Repository:** davebirr/Fabrikam-Project
