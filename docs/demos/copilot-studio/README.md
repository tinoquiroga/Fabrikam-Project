# 🤖 Copilot Studio Integration with Fabrikam MCP

Welcome to the complete guide for integrating Microsoft Copilot Studio with your Fabrikam Modular Homes MCP server. Choose the authentication approach that matches your needs.

## 🎯 Quick Start Decision Tree

| Use Case | Authentication Mode | Guide | Setup Time |
|----------|-------------------|-------|------------|
| **Demo & Development** | Disabled | [No Authentication Setup](#-no-authentication-setup) | 15 minutes |
| **Production & Security** | JWT | [JWT Authentication Setup](#-jwt-authentication-setup) | 25 minutes |
| **Enterprise SSO** | Entra External ID | [Enterprise Setup](#-enterprise-setup-coming-soon) | Coming Soon |

## 🔍 Determine Your Authentication Mode

Check your Azure deployment output or use this API call:

```bash
curl -k https://your-api-app-name.azurewebsites.net/api/info
```

Look for the `authentication.mode` field in the response:
- `"Disabled"` → Use No Authentication Setup
- `"BearerToken"` → Use JWT Authentication Setup  
- `"EntraExternalId"` → Use Enterprise Setup (coming soon)

## 📚 Setup Guides

### 🔓 No Authentication Setup

Perfect for demos, development, and proof-of-concept scenarios where you need quick setup without authentication complexity.

**[→ Complete No Authentication Guide](./NO-AUTH-SETUP.md)**

**Quick Preview:**
- Generate a GUID for session tracking
- Create agent with business-focused instructions
- Configure custom connector with MCP endpoint
- Test with sample business queries

**Expected Setup Time:** 15 minutes

### 🔐 JWT Authentication Setup

Production-ready security with JWT tokens, demo user accounts, and role-based access control.

**[→ Complete JWT Authentication Guide](./JWT-AUTH-SETUP.md)**

**Quick Preview:**
- Get JWT token from demo user accounts
- Create agent with authenticated business instructions
- Configure secure custom connector with token headers
- Test with authenticated business queries

**Expected Setup Time:** 25 minutes

### 🛡️ Enterprise Setup (Coming Soon)

Enterprise-grade OAuth 2.0 authentication with Microsoft Entra External ID, providing SSO and advanced security controls.

**[→ Enterprise Authentication Guide](./ENTRA-AUTH-SETUP.md)**

**Planned Features:**
- OAuth 2.0 / OpenID Connect authentication
- Single Sign-On (SSO) with Microsoft Entra External ID
- Role-based access control (RBAC)
- Multi-factor authentication (MFA) support

**Status:** In Development

## 🔧 Common Troubleshooting

For issues common across all authentication modes:

**[→ Troubleshooting Guide](./TROUBLESHOOTING.md)**

**Common Solutions:**
- MCP connection issues
- Connector configuration problems
- Agent behavior tuning
- Authentication token problems

## 🚀 Advanced Configuration

### Workshop Integration

If you're using these guides for workshop or training purposes:

**[→ Workshop Organizer Guide](../../workshops/README.md)**

**Workshop Benefits:**
- Disable web search for clear failure modes
- Configure focused business behavior
- Provide participant guidance materials
- Include troubleshooting scenarios

### Custom Business Instructions

All setup guides include comprehensive business instructions that:
- Keep agents focused on Fabrikam business operations
- Redirect off-topic questions professionally
- Provide contextual help for business scenarios
- Maintain consistent branding and behavior

### Demo Scenarios

Ready-made demo scenarios for each authentication mode:

**[→ Demo Prompts Library](../prompts/README.md)**

**Available Scenarios:**
- Sales analytics demonstrations
- Customer service interactions
- Inventory management queries
- Executive dashboard presentations

## 💡 Best Practices

### Security Considerations

1. **Development vs Production**
   - Use disabled auth for development only
   - Always use JWT or Entra for production
   - Never expose production tokens in demos

2. **Token Management**
   - JWT tokens expire and need refresh
   - Store tokens securely in production
   - Use environment-specific configurations

3. **Agent Behavior**
   - Configure focused business instructions
   - Disable web search for controlled responses
   - Test thoroughly before workshops/demos

### Performance Optimization

1. **Connection Management**
   - Test connector connections before demos
   - Have backup plans for connection issues
   - Monitor MCP server availability

2. **Response Quality**
   - Train users on effective business queries
   - Provide sample questions for demos
   - Configure clear error handling

## 📞 Support

### Quick Help

1. **Check Authentication Mode**: Use `/api/info` endpoint
2. **Validate MCP Connection**: Test connector in Power Apps
3. **Review Agent Instructions**: Ensure business focus is configured
4. **Test Sample Queries**: Use provided demo prompts

### Additional Resources

- **[Main Documentation](../../README.md)**: Complete project overview
- **[Authentication Guide](../../architecture/AUTHENTICATION.md)**: Detailed auth architecture
- **[Workshop Materials](../../workshops/README.md)**: Training and workshop guides
- **[API Documentation](../../../FabrikamApi/README.md)**: Complete API reference

### Community

- **GitHub Issues**: Report bugs and request features
- **Workshop Feedback**: Share workshop experiences and improvements
- **Demo Stories**: Contribute successful demo scenarios

---

**Choose your setup guide above and get started! Each guide is self-contained with step-by-step instructions and troubleshooting help.**
