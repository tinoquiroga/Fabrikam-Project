# 🎯 Fabrikam MCP Demonstrations

Welcome to the comprehensive demo resources for Fab```

### **Choose Your Authentication Mode**

First, determine which authentication mode your Fabrikam deployment uses:

```bash
# Check your deployment configuration
curl -k https://your-api-app-name.azurewebsites.net/api/info
```

Look for `authentication.mode`:
- `"Disabled"` → [No Authentication Setup](./copilot-studio/NO-AUTH-SETUP.md)
- `"BearerToken"` → [JWT Authentication Setup](./copilot-studio/JWT-AUTH-SETUP.md)
- `"EntraExternalId"` → [Enterprise Setup](./copilot-studio/ENTRA-AUTH-SETUP.md) (coming soon)

## 🎬 Demo Best Practices

### **Preparation**

1. **Validate Environment** - Run validation tools before every demo
2. **Test Demo Prompts** - Verify all scenarios work with current data
3. **Plan Timing** - Account for response times and interaction flow
4. **Backup Plans** - Have alternative prompts ready for technical issues

### **Presentation Tips**

1. **Start with Business Value** - Lead with executive dashboard or sales insights
2. **Show Real Data** - Use current business data, not generic examples
3. **Demonstrate Intelligence** - Show how the system understands business context
4. **Handle Questions** - Use off-topic redirects to maintain business focus

### **Technical Considerations**

1. **Authentication Tokens** - JWT tokens expire; refresh before long sessions
2. **Response Times** - MCP tools should respond within 2-3 seconds for demos
3. **Error Handling** - Test failure scenarios and recovery procedures
4. **Session Management** - Start new conversations to reset MCP context

## 📊 Success Metrics

Track these metrics to improve your demonstrations:

### **Engagement Metrics**
- Audience questions and interaction depth
- Follow-up meeting requests
- Business scenario exploration

### **Technical Metrics**  
- MCP tool response times
- Authentication success rates
- Demo environment stability

### **Business Metrics**
- Value propositions communicated
- Use cases demonstrated
- ROI scenarios presented

## 🔗 Integration with Workshops

Demos serve as building blocks for structured workshops:

**Demo Assets → Workshop Components:**
- Demo prompts become workshop exercises
- Authentication flows become hands-on labs
- Business scenarios become case studies
- Technical configurations become learning modules

**→ [Workshop Framework](../workshops/README.md)** - See how demos integrate into comprehensive training programs

## 📞 Support and Resources

### **Quick Help**

1. **Setup Issues**: Check [Troubleshooting Guide](./copilot-studio/TROUBLESHOOTING.md)
2. **Authentication Problems**: Review [Authentication Guide](./authentication/README.md)
3. **Demo Validation**: Use [Validation Tools](./validation/README.md)
4. **Workshop Planning**: See [Workshop Materials](../workshops/README.md)

### **Community Resources**

- **GitHub Issues** - Report bugs and request features
- **Discussion Forums** - Share successful demo stories
- **Workshop Feedback** - Contribute to workshop improvements
- **Demo Scenarios** - Submit new business use cases

### **Additional Documentation**

- **[Main Documentation](../README.md)** - Complete project overview
- **[Authentication Architecture](../architecture/AUTHENTICATION.md)** - Security implementation details
- **[API Documentation](../../FabrikamApi/README.md)** - Complete API reference
- **[MCP Documentation](../../FabrikamMcp/README.md)** - MCP server implementation

---

**🎯 Ready to demonstrate?** Choose your demo type above and showcase the power of natural language business intelligence with Fabrikam MCP!m MCP (Model Context Protocol) integration. This section provides everything you need for successful demonstrations, from quick 3-minute presentations to detailed technical workshops.

## � Quick Start

### **I want to demonstrate Fabrikam MCP in Copilot Studio**
**→ [Copilot Studio Integration Guide](./copilot-studio/README.md)**

Choose your authentication mode and get step-by-step setup instructions for Microsoft Copilot Studio integration.

### **I need demo prompts and scenarios**
**→ [Demo Prompts Library](./prompts/README.md)**

Ready-made prompts for executives, sales teams, technical audiences, and quick 3-minute demos.

### **I want to validate my demo setup**
**→ [Demo Validation Tools](./validation/README.md)**

Automated testing and validation scripts to ensure your demo environment is ready.

## 📚 Demo Resources

### 🤖 [Copilot Studio Integration](./copilot-studio/)
Complete guides for integrating Fabrikam MCP with Microsoft Copilot Studio.

| Authentication Mode | Setup Time | Use Case |
|---------------------|------------|----------|
| **[No Authentication](./copilot-studio/NO-AUTH-SETUP.md)** | 15 minutes | Quick demos, development |
| **[JWT Authentication](./copilot-studio/JWT-AUTH-SETUP.md)** | 25 minutes | Production demos, security |
| **[Enterprise SSO](./copilot-studio/ENTRA-AUTH-SETUP.md)** | Coming Soon | Enterprise integration |

### 🎭 [Demo Prompts Library](./prompts/)
Curated prompts for different audiences and scenarios.

| Prompt Category | Duration | Best For |
|-----------------|----------|----------|
| **[Quick Demo Scripts](./prompts/README.md#-quick-demo-scripts)** | 3-5 minutes | General audiences, videos |
| **[Business Value Prompts](./prompts/README.md#-business-value-prompts)** | 10-15 minutes | Executives, managers |
| **[Technical Prompts](./prompts/README.md#-technical-prompts)** | 15-30 minutes | Developers, architects |

### 🔐 [Authentication Demos](./authentication/)
User management and role-based demonstration scenarios.

- **Demo User Accounts** - Predefined users with different access levels
- **Authentication Flows** - JWT and enterprise authentication demos
- **Security Scenarios** - Role-based access control demonstrations

### � [Validation Tools](./validation/)
Automated tools to ensure your demo environment is ready.

- **Pre-Demo Validation** - Comprehensive environment checks
- **Performance Testing** - Response time and reliability validation  
- **Authentication Testing** - User account and token validation

## 🎯 Demo Concepts

### **Demos vs Workshops**

**Demos** are reusable assets for presentations and quick demonstrations:
- Focus on showing capabilities and business value
- Typically 5-30 minutes in duration
- Designed for broad audiences
- Emphasize visual impact and clear value proposition

**Workshops** are structured learning experiences:
- Focus on hands-on learning and skill development
- Typically 1-4 hours in duration  
- Designed for specific audiences with learning objectives
- Emphasize participation and knowledge transfer

**→ [Workshop Materials](../workshops/README.md)** - Structured learning sessions that build upon demo assets

## 🚀 Quick Start Decision Tree

### **Choose Your Demo Path**

```
What type of demonstration do you need?

├── 🎥 Quick Presentation (3-5 minutes)
│   └── Use: [Quick Demo Scripts](./prompts/README.md#-quick-demo-scripts)
│
├── 💰 Business Value Demo (10-15 minutes)  
│   └── Use: [Business Value Prompts](./prompts/README.md#-business-value-prompts)
│
├── 🔧 Technical Demo (15-30 minutes)
│   └── Use: [Technical Prompts](./prompts/README.md#-technical-prompts) 
│
└── 🎓 Learning Workshop (1-4 hours)
    └── Use: [Workshop Materials](../workshops/README.md)
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
