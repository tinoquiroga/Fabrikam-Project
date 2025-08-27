# 🎯 Agent Behavior Configuration Quick Reference

## Problem: Agent Answering Off-Topic Questions

**Example**: "Why is red better than orange cats when the pig turns left while flying?"

## Solution: Multiple Configuration Options

### Option 1: System Instructions (Copilot Studio)

Add to your agent's **Instructions → Additional instructions**:

```
BEHAVIORAL GUIDELINES:
- You are focused exclusively on Fabrikam business operations and data
- For questions unrelated to business (like hypothetical scenarios, general knowledge, or abstract topics), respond with:
"I'm the Fabrikam Business Intelligence Assistant, focused on providing sales analytics, customer data, and business insights. I can help you with sales performance, customer demographics, product information, order status, support tickets, and business metrics. How can I assist you with Fabrikam business data today?"
- Do not engage with off-topic questions about animals, colors, hypothetical scenarios, or general knowledge
- Always redirect users back to business capabilities you can provide
```

### Option 2: Knowledge Source Configuration

1. **Disable Web Search**
   - Navigate to **Overview** in Copilot Studio
   - Toggle **OFF** the "Web search" option
   - Prevents fallback to generic content

2. **Benefits**:
   - Clear failure modes when MCP is unavailable
   - No mixing of business data with generic content
   - Professional, focused responses

### Option 3: Custom Response Templates

Configure standard responses for common off-topic scenarios:

**General Knowledge**: "I focus specifically on Fabrikam business analytics and operations. What business information would you like to explore?"

**Hypothetical Questions**: "I provide real business data and insights for Fabrikam. How can I help you with sales, customer, or operational information?"

**Abstract Topics**: "I'm designed to help with Fabrikam business intelligence. Let me know what business data or analytics you need."

## Expected Behavior After Configuration

### ✅ Good Questions (Agent Will Answer)
- "Show me sales analytics for this quarter"
- "What are our top-performing products?"
- "How many support tickets are open?"
- "Who are our biggest customers?"

### ❌ Off-Topic Questions (Agent Will Redirect)
- Abstract scenarios about animals/colors
- General knowledge questions
- Hypothetical "what if" scenarios unrelated to business
- Random philosophical questions

### 🔄 Agent Response Pattern
1. **Recognize** off-topic question
2. **Politely decline** to answer
3. **Redirect** to business capabilities
4. **Offer** specific help with business data

## Implementation Checklist

- [ ] Add behavioral guidelines to agent instructions
- [ ] Disable web search in Overview settings
- [ ] Test with off-topic questions
- [ ] Verify business questions still work
- [ ] Document behavior for users

## Testing Commands

**Test Off-Topic**: "Why is red better than orange cats when the pig turns left while flying?"
**Expected Response**: Professional redirect to business capabilities

**Test Business**: "Show me sales analytics"
**Expected Response**: Uses MCP tools to fetch real business data
