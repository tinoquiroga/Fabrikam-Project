# 🏗️ Fabrikam Modular Homes API

Welcome to the Fabrikam Modular Homes API project! This .NET-based API simulates the core business operations of a fictitious, innovative modular home builder—**Fabrikam**—designed to support storytelling, hands-on labs, and Copilot agent development for SME&C customers.

---

## 🎯 Project Purpose

This project enables immersive, hands-on experiences for:
- **Partner and customer go-to-market trainings**
- **Copilot and Copilot Chat agent development**
- **Demonstrating tangible business value to decision makers**

By simulating real-time business data and workflows, this API helps business users across roles—Sales, Customer Service, Inventory, Finance, and more—see how Copilot can drive productivity and insight.

---

## 💵 Why are we doing this?

We want to leverage this project to drive revenue for Microsoft & CSP partners by accelerating the time to value for customer adoption of AI.

The hands-on labs we have today do not have real world business context. For example, the typical experience uses Copilot to simply retrieves knowledge based on docs in a SharePoint library and this is unlikely to inspire a business decision maker such as a CEO or Marketing director to invest in AI.

 This project aims at delivering an easy to use, simple to deploy, repeatable, scalable example business back-end that’s Copilot ready. The users of this project will not need to know anything about APIs or Model Context Protocol (MCP). They will also not need to connect to their real production data. While realistic, everything is simulated providing a safe environment for labs and exploration of Agentic AI with M365 Copilot, Copilot Chat, and Copilot Studio.

### 💼 1. Impact to the Microsoft Business
 - Accelerates Copilot Adoption in SME&C
 - Demonstrates real-world, role-based value of Copilot and agents in a relatable, scalable way.
 - Equips field teams and partners with a ready-to-use, hands-on asset to drive customer engagement and solution envisioning.

### 🤝 2. Impact to the CSP Partner Business
 - Accelerates Time-to-Value
 - Partners can deploy the API and labs in minutes, with no custom setup required.
 - Enables partners to run impactful Copilot workshops without needing to build their own demo environments.
 - Builds credibility with customers through hands-on, business-relevant demos.

### 🏢 3. Impact to the End Customer Business
 - Makes Copilot Tangible and Relevant
 - Customers see how Copilot can solve real problems in Sales, Inventory, and Customer Service—without abstract demos.
 - Encourages customers to imagine how Copilot and agents could transform their own operations.


---

## 🧱 Fabrikam Business Context

**Fabrikam** is a fictional company that:
- Designs and sells modular homes online
- Offers rapid delivery and installation by homeowners or small crews
- Operates with a lean, tech-forward business model ideal for SMEs
- Fabrikam is embracing AI and expects to accelerate key business metrics

---
![Fabrikam Archtiecture Drawing](Docs/images/FabrikamArchitecture.png)
## 🧩 API Modules & Features
The API emulates key business functions relevant to companies with 300–2000 employees, avoiding enterprise-heavy complexity.

### 1. Sales
- Manage customer orders
- Track order status and history
- Analyze sales trends

### 2. Inventory
- View and manage modular home models and components
- Monitor stock levels and availability
- Simulate real-time inventory changes

### 3. Customer Service
- Create and manage support tickets
- Track resolution status
- Add internal notes and updates

---

## 🧠 Model Context Protocol (MCP)

The API is designed to be **Copilot-ready**, with a structured schema that defines:

- **Entities**: Customer, Order, Product, Ticket, etc.
- **Relationships**: e.g., Customers have Orders and Tickets
- **Metadata**: e.g., OrderDate, Region, Status

This enables natural language interactions like:
- “What are our top-selling models this quarter?”
- “Do we have enough stock to fulfill this order?”
- “Show me all open support tickets in the Northeast.”

---

## 🧪 Hands-On Lab Scenarios

| Role | Scenario | Copilot Agent Use |
|------|----------|-------------------|
| CEO | Analyze top-selling products | Strategic insights |
| Sales | Check inventory before confirming an order | Operational efficiency |
| Customer Service | Resolve a support ticket | Productivity boost |
| Finance | Review revenue by product line | Data-driven decisions |
| Marketing | Identify regional product trends | Campaign planning |

---

## 🔄 Real-Time Simulation

The API supports dynamic data:
- Orders placed and fulfilled in real time
- Inventory levels updated with each transaction
- Support tickets created and resolved during labs

Seed scripts will reset the database for each lab session to ensure consistency and repeatability.

---

## 🧪 Real-World Simulation Engine (Optional)

To enhance realism and demonstrate Copilot’s value in dynamic environments, this project includes an **optional simulation engine** that mimics real-world business activity.

### 🔄 Simulation Features
When enabled, the simulator will:
- Automatically **place new customer orders** at random intervals
- **Fulfill orders** using available inventory
- **Open support tickets** for delayed or failed orders
- **Create random operational issues**, such as:
  - Inventory miscounts
  - Delivery delays
  - Customer complaints
- **Trigger security incidents** that simulate partial system outages or data access issues

### 🧰 Use Cases
This simulation is ideal for:
- **Advanced Copilot labs** that test agent adaptability
- **Demonstrating business continuity scenarios**
- **Training sessions** on risk, compliance, and customer service

### ⚙️ Configuration
- The simulator can be toggled on/off via environment variables or deployment parameters
- Frequency and intensity of events can be customized
- Logs and telemetry are available for analysis and debugging

### 🧪 Example Scenarios
| Event | Copilot Prompt |
|-------|----------------|
| Order delay | “Why is order #1234 late?” |
| Inventory issue | “Why can’t I fulfill this order?” |
| Security incident | “What systems are currently offline?” |
| Surge in orders | “Summarize today’s order volume and fulfillment status.” |

This feature helps participants **see Copilot in action** during unexpected events—making the value of AI agents even more tangible.

---

## 🔒 Security Scenarios
- Payments diverted
- Orders down
- Ransomware
  
This API will support security scenarios in the future by demonstrating the business relevance of security issues

---

## 🚀 Deployment Goals

This project is designed to be **easy to deploy, use, and customize** for both internal Microsoft labs and partner-led workshops.

### ✅ Default Deployment (Microsoft-hosted)
- The API will be deployed as a **shared Azure resource** for hands-on labs.
- **No setup required** for lab participants—just connect and start using.
- Ideal for guided sessions, demos, and Copilot agent development.

### 🔧 Partner Deployment Kit
Partners can fork this project and deploy their own instance with minimal effort:
- **One-click deployment** via GitHub Actions or Azure Resource Manager (ARM) templates
- **Authentication-based setup**—just sign in to your Azure tenant
- **Preconfigured workflows** and automation scripts included
- **Optional customization**: Partners can tailor the API, data, and labs to their needs, but everything works out-of-the-box

### 🧰 Deployment Artifacts (Planned)
- `deploy/azure-template.json` – ARM template for infrastructure
- `.github/workflows/deploy.yml` – GitHub Actions workflow for CI/CD
- `scripts/seed-data.ps1` – PowerShell script to seed lab data
- `docs/deployment-guide.md` – Step-by-step instructions for partners

This approach ensures that **anyone can get started quickly**, while still supporting deeper customization for advanced use cases.

---

## 🚀 Next Steps

- [ ] Finalize API endpoints and data contracts
- [ ] Define MCP schema in JSON
- [ ] Build seed scripts for lab scenarios
- [ ] Create sample Copilot prompts and agent flows
- [ ] Write developer and facilitator guides

---

## 🤝 Target Audience

This project is designed for:
- Microsoft partners and field teams
- SME&C customers exploring Copilot adoption
- Business decision makers in Sales, Finance, Legal, Risk, and Marketing

---

## 📂 Project Structure (Planned)

🏗️ Fabrikam .NET API – Core Modules
1. Sales
```
GET /orders – List all customer orders
POST /orders – Create a new order
GET /orders/{id} – Get order details
GET /customers – List customers
GET /customers/{id} – Customer profile and order history
```
2. Inventory
```
GET /products – List available modular home models and components
GET /products/{id} – Product details (e.g., dimensions, price, availability)
GET /inventory – Current stock levels
PATCH /inventory/{id} – Update stock after order fulfillment
```
3. Customer Service
```GET /tickets – List support tickets
POST /tickets – Create a new support request
GET /tickets/{id} – View ticket status and history
POST /tickets/{id}/notes – Add internal notes or updates
```
