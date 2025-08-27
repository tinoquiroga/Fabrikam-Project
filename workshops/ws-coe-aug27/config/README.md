# ⚙️ Workshop Configuration

Configuration files and templates for workshop setup.

## 📁 File Types

### **User Configuration**
- `coe-users-template.csv` - Template for participant user list
- `coe-users.csv` - Active participant user list (workshop-specific)
- `coe-users-actual.csv` - Actual provisioned users (generated)

### **COE Configuration**
- `coe-config-template.json` - Template for COE settings
- `coe-config.json` - Active COE configuration (workshop-specific)

## 🔧 Usage

### Setting Up Users
1. Copy `coe-users-template.csv` to `coe-users.csv`
2. Fill in participant information
3. Run provisioning scripts from `../scripts/`

### Configuring COE
1. Copy `coe-config-template.json` to `coe-config.json`
2. Update settings for your workshop environment
3. Deploy using organizer scripts

## 📋 CSV Format (Users)

```csv
Email,DisplayName,Department,Role
participant1@company.com,John Doe,IT,Developer
participant2@company.com,Jane Smith,Marketing,Analyst
```

## 🔒 Security Notes

- Do not commit actual user data to version control
- Use templates for documentation
- Keep actual configuration files local to workshop environment

---

**Note**: These configuration files are managed by workshop organizers.
