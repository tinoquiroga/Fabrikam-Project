# 🎥 Fabrikam MCP Server - Copilot Studio Demo Prompts

> **3-Minute Demo Guide**: Showcasing Business Intelligence and Customer Service Capabilities

## 🎯 Demo Overview

This demo showcases how the Fabrikam MCP (Model Context Protocol) server transforms business data into actionable insights through natural language interactions. The demo highlights real-world business scenarios using our modular homes company data.

---

## 📊 **SECTION 1: Executive Dashboard & Business Intelligence** (60 seconds)

### 🏢 Executive Summary Prompt
```
Show me the current business dashboard for Fabrikam Modular Homes. I need to understand our overall performance, key metrics, and any areas that need attention.
```

**Expected Showcase:**
- Total revenue and order metrics
- Regional performance analysis
- Product category insights
- Key performance indicators

### 📈 Sales Performance Analysis
```
I'm preparing for our quarterly board meeting. Can you analyze our sales performance by region and identify our top-performing products? Also, what trends should I be aware of?
```

**Expected Showcase:**
- Regional sales breakdown
- Product performance rankings
- Trend analysis and insights
- Data-driven recommendations

---

## 🎫 **SECTION 2: Customer Service Intelligence** (90 seconds)

### 🚨 Support Ticket Crisis Management
```
We're having a leadership meeting about customer satisfaction. Show me our current support ticket situation - what are the major issues customers are facing, and are there any patterns I should be concerned about?
```

**Expected Showcase:**
- Support ticket volume and trends
- Issue categorization (Quality, Delivery, Technical)
- Priority distribution
- Long-running issues identification

### 🔍 Historical Issue Analysis
```
I'm seeing some quality issues mentioned in support tickets. Can you analyze our customer complaints from 2020-2021 versus recent years? Has our quality improved, and what specific problems have we solved?
```

**Expected Showcase:**
- Year-over-year comparison
- Issue resolution improvements
- Quality evolution narrative (startup → mature operations)
- Success stories and ongoing challenges

### 🏗️ Engineering Problem Investigation
```
I heard we have some ongoing HVAC issues with certain models. Can you investigate this for me - which customers are affected, how long has this been going on, and what's the business impact?
```

**Expected Showcase:**
- Long-running ticket analysis (4+ year HVAC case)
- Customer impact assessment
- Engineering escalation tracking
- Pattern recognition across similar issues

---

## 💼 **SECTION 3: Strategic Business Insights** (30 seconds)

### 🎯 Customer Success Stories
```
I want to showcase our customer success in the next marketing campaign. Can you find examples of customers who had early challenges but are now happy with our service? Show me the complete customer journey.
```

**Expected Showcase:**
- Customer satisfaction evolution
- Problem resolution success stories
- Referral and repeat business indicators
- Business growth narrative

### 🔮 Predictive Business Intelligence
```
Based on our current data trends, what recommendations would you make for our business strategy? Where should we focus our attention in the next quarter?
```

**Expected Showcase:**
- Data-driven insights
- Trend predictions
- Strategic recommendations
- Risk area identification

---

## 🚀 **Advanced Demo Scenarios** (Optional Extended Demos)

### 🔄 Cross-Functional Analysis
```
I need to prepare a comprehensive report that shows the relationship between our product quality issues and customer satisfaction. Connect the dots between support tickets, order data, and customer feedback for me.
```

### 📋 Operational Efficiency Review
```
Analyze our operational efficiency by looking at delivery performance, product defect rates, and customer service response times. Where are our biggest opportunities for improvement?
```

### 🏆 Competitive Advantage Assessment
```
Help me understand what makes Fabrikam successful. Based on our customer feedback and business metrics, what are our key differentiators and strengths?
```

---

## 🎬 **Demo Script Flow for 3-Minute Video**

### **Opening (15 seconds)**
"Let me show you how Fabrikam's MCP server transforms raw business data into actionable insights through natural conversation with Copilot."

### **Business Intelligence Demo (60 seconds)**
1. Start with executive dashboard prompt
2. Show real-time data visualization
3. Highlight regional performance insights
4. Demonstrate trend analysis

### **Customer Service Intelligence (90 seconds)**
1. Support ticket analysis prompt
2. Show issue categorization and patterns
3. Demonstrate historical improvement tracking
4. Highlight the long-running HVAC case study

### **Strategic Insights (30 seconds)**
1. Customer success journey prompt
2. Show data-driven recommendations
3. Demonstrate predictive capabilities

### **Closing (5 seconds)**
"From executive dashboards to customer service intelligence, Fabrikam's MCP server makes business data conversational and actionable."

---

## 📝 **Demo Preparation Checklist**

### ✅ **Before Recording:**
- [ ] Ensure both API and MCP servers are running
- [ ] Verify all 24 support tickets are loaded
- [ ] Confirm order and customer data is populated
- [ ] Test each prompt to ensure consistent responses
- [ ] Clear Copilot conversation history for clean demo

### ✅ **Demo Environment Setup:**
- [ ] Use `.\Test-Development.ps1 -Quick` to verify everything is working
- [ ] Check that support tickets show the business evolution (2020→2025)
- [ ] Verify regional data and product categories are displaying
- [ ] Ensure HVAC ongoing issue case is accessible

### ✅ **Recording Tips:**
- Speak clearly and at moderate pace
- Allow time for Copilot to process and respond
- Highlight specific data points that demonstrate business value
- Show the variety of insights possible with different prompt styles

---

## 🔧 **Troubleshooting Common Demo Issues**

### **MCP Server Not Responding:**
```powershell
# Quick fix - restart servers
.\Test-Development.ps1 -CleanBuild -Quick
```

### **Missing Support Ticket Data:**
```powershell
# Force reseed the database
# (Add this capability to your API if not already present)
```

### **Slow Response Times:**
- Ensure development environment is using local servers
- Close unnecessary applications
- Use `-Quick` flag for faster testing

---

## 🎯 **Key Business Value Points to Highlight**

1. **Real-time Business Intelligence**: Transform complex data into instant insights
2. **Customer Service Excellence**: Track and improve customer satisfaction systematically
3. **Operational Efficiency**: Identify patterns and optimize business processes
4. **Strategic Decision Making**: Data-driven insights for leadership decisions
5. **Historical Analysis**: Learn from past challenges to prevent future issues
6. **Predictive Capabilities**: Anticipate trends and make proactive decisions

---

## 📈 **Future Demo Enhancements**

As you develop more capabilities, consider adding these advanced scenarios:

### 🔮 **Predictive Analytics**
- Customer churn prediction
- Demand forecasting
- Quality issue prevention

### 🤖 **Automated Insights**
- Anomaly detection alerts
- Performance threshold monitoring
- Automated reporting generation

### 🎨 **Visual Intelligence**
- Interactive chart generation
- Trend visualization
- Geographic heat maps

### 🔗 **Integration Scenarios**
- CRM system connections
- ERP data integration
- External market data analysis

---

## 📞 **Contact & Feedback**

For questions about these demo prompts or to suggest improvements:
- Review the MCP tools in `FabrikamMcp/src/Tools/`
- Test scenarios using the comprehensive test suite
- Document new use cases in this file for team reference

**Last Updated:** July 2025
**Demo Duration:** 3 minutes
**Target Audience:** Business stakeholders, technical teams, potential customers
