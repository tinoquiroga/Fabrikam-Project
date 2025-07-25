# Pull Request

## 📋 Description
<!-- Provide a brief description of the changes in this PR -->

## 🔗 Related Issues
<!-- Link to related issues using "Fixes #issue-number" or "Relates to #issue-number" -->
- Fixes #
- Relates to #

## 🎯 Type of Change
<!-- Mark the relevant option with an "x" -->
- [ ] 🐛 Bug fix (non-breaking change which fixes an issue)
- [ ] ✨ New feature (non-breaking change which adds functionality)
- [ ] 💥 Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] 📚 Documentation update
- [ ] 🔧 Refactoring (no functional changes)
- [ ] ⚡ Performance improvement
- [ ] 🧪 Test additions or improvements
- [ ] 🚀 CI/CD improvements
- [ ] 🏗️ Infrastructure changes

## 🧪 Testing
<!-- Describe the tests you ran to verify your changes -->

### Test Results
- [ ] ✅ All existing tests pass (`dotnet test FabrikamTests/`)
- [ ] ✅ New tests added for new functionality
- [ ] ✅ Manual testing completed
- [ ] ✅ API endpoints tested (via `api-tests.http`)
- [ ] ✅ MCP tools tested (via `Test-Development.ps1 -McpOnly`)
- [ ] ✅ Integration tests pass (`Test-Development.ps1 -Verbose`)

### Test Commands Run
```powershell
# List the commands you used to test
.\Test-Development.ps1 -Quick
.\Test-Development.ps1 -ApiOnly
.\Test-Development.ps1 -McpOnly
dotnet test FabrikamTests/
```

## 📸 Screenshots/Demos
<!-- If applicable, add screenshots or demo links to help explain your changes -->

## 🔧 Technical Details

### API Changes
<!-- If you modified the FabrikamApi -->
- [ ] New endpoints added
- [ ] Existing endpoints modified
- [ ] Database schema changes
- [ ] DTO/model changes
- [ ] Configuration changes

### MCP Changes
<!-- If you modified the FabrikamMcp -->
- [ ] New MCP tools added
- [ ] Existing tools modified
- [ ] Tool descriptions updated
- [ ] Error handling improved

### Infrastructure Changes
<!-- If you modified deployment/infrastructure -->
- [ ] Azure resource changes
- [ ] GitHub Actions workflow changes
- [ ] Environment configuration changes
- [ ] Port/networking changes

## 📚 Documentation
- [ ] Code is self-documenting with clear method/class names
- [ ] XML documentation comments added/updated
- [ ] README.md updated (if needed)
- [ ] API documentation updated (if needed)
- [ ] MCP tool descriptions are clear and helpful

## ✅ Quality Checklist
- [ ] Code follows the project's coding standards
- [ ] Async/await patterns used consistently
- [ ] Proper error handling implemented
- [ ] Structured logging added where appropriate
- [ ] No hardcoded values (using configuration)
- [ ] Security considerations addressed
- [ ] Performance impact considered
- [ ] Backward compatibility maintained (or breaking changes documented)

## 🚀 Deployment Considerations
- [ ] Changes are compatible with Azure App Service deployment
- [ ] Environment variables/configuration documented
- [ ] Database migration needed (if applicable)
- [ ] No impact on existing production data
- [ ] Rollback plan considered

## 👥 Review Notes
<!-- Any specific areas you'd like reviewers to focus on -->

## 📋 Post-Merge Tasks
<!-- List any tasks that need to be completed after merging -->
- [ ] Update production configuration
- [ ] Monitor deployment logs
- [ ] Update project documentation
- [ ] Notify stakeholders

---

**Reviewer Guidelines:**
- 🔍 Check that all tests pass
- 📖 Verify code follows project standards
- 🧪 Test the changes locally if possible
- 💭 Consider the impact on other developers
- 🚀 Ensure production readiness
