# 🤖 Copilot Studio Setup Guides for Fabrikam MCP

Choose the setup guide that matches your Fabrikam deployment's authentication configuration.

## 📚 Available Guides

### 🔓 [Disabled Authentication Setup](./Copilot-Studio-Disabled-Setup-Guide.md)
**Perfect for: Demos, Development, Proof-of-Concept**

- ✅ **Fastest setup** - No authentication complexity
- ✅ **Ideal for demonstrations** and initial testing  
- ✅ **Quick prototyping** of agent capabilities
- ⚠️ **Not for production** - No security controls

**Use when**: Your Fabrikam deployment is configured with `Authentication.Mode: "Disabled"`

---

### 🔐 [JWT Authentication Setup](./Copilot-Studio-JWT-Setup-Guide.md)
**Perfect for: Production, Secure Demos, User Testing**

- ✅ **Production-ready security** with JWT tokens
- ✅ **Demo user accounts** with predefined roles
- ✅ **User tracking and auditing** capabilities
- ✅ **Role-based access control** ready for expansion

**Use when**: Your Fabrikam deployment is configured with `Authentication.Mode: "BearerToken"`

---

### 🛡️ [Entra External ID Setup](./Copilot-Studio-Entra-Setup-Guide.md)
**Perfect for: Enterprise, SSO Integration, Advanced Security**

- 🚧 **Coming Soon** - Currently in development
- 🎯 **Enterprise-grade** OAuth 2.0 authentication
- 🔒 **Single Sign-On** with Microsoft Entra External ID
- 🏢 **Full enterprise integration** with existing identity systems

**Use when**: Your organization requires enterprise identity integration

---

## 🚀 Quick Start Decision Tree

```
Do you need authentication?
├── No (Just testing/demoing)
│   └── 📖 Use: Disabled Authentication Setup Guide
│
└── Yes (Production or secure demo)
    ├── Simple JWT authentication needed?
    │   └── 📖 Use: JWT Authentication Setup Guide
    │
    └── Enterprise SSO integration needed?
        └── 📖 Coming Soon: Entra External ID Setup Guide
```

## 🔧 How to Check Your Authentication Mode

Not sure which authentication mode your Fabrikam deployment is using? Check with:

```bash
curl -X GET "https://your-api-app-name.azurewebsites.net/api/info"
```

Look for the `authenticationConfiguration` section in the response:

```json
{
  "authenticationConfiguration": {
    "mode": "Disabled",  // or "BearerToken" or "EntraExternalId"
    "features": ["..."]
  }
}
```

## 📞 Need Help?

- **Deployment Issues**: Check your Azure deployment documentation
- **Authentication Questions**: Review the specific setup guide for your mode
- **Feature Requests**: Create an issue in the project repository
- **General Support**: Consult the main Fabrikam documentation

---

**💡 Tip**: Start with the **Disabled Authentication** guide for initial testing, then move to **JWT Authentication** when you're ready for production security features.
