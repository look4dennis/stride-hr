using System.Text.Json;
using System.Text.RegularExpressions;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;

namespace StrideHR.Infrastructure.Services;

public class NaturalLanguageProcessingService : INaturalLanguageProcessingService
{
    private readonly Dictionary<string, List<string>> _intentPatterns;
    private readonly Dictionary<string, List<string>> _responses;
    private readonly Dictionary<string, List<string>> _suggestedResponses;

    public NaturalLanguageProcessingService()
    {
        _intentPatterns = InitializeIntentPatterns();
        _responses = InitializeResponses();
        _suggestedResponses = InitializeSuggestedResponses();
    }

    public async Task<string> DetectIntentAsync(string message, Dictionary<string, object>? context = null)
    {
        var normalizedMessage = message.ToLowerInvariant().Trim();
        
        // Check for exact matches first
        foreach (var intent in _intentPatterns.Keys)
        {
            foreach (var pattern in _intentPatterns[intent])
            {
                if (Regex.IsMatch(normalizedMessage, pattern, RegexOptions.IgnoreCase))
                {
                    return intent;
                }
            }
        }

        // Default intent for unrecognized messages
        return "general_inquiry";
    }

    public async Task<Dictionary<string, object>> ExtractEntitiesAsync(string message)
    {
        var entities = new Dictionary<string, object>();
        var normalizedMessage = message.ToLowerInvariant();

        // Extract common entities
        ExtractDateEntities(normalizedMessage, entities);
        ExtractNumberEntities(normalizedMessage, entities);
        ExtractEmailEntities(message, entities);
        ExtractEmployeeIdEntities(message, entities);

        return entities;
    }

    public async Task<decimal> GetConfidenceScoreAsync(string message, string intent)
    {
        var normalizedMessage = message.ToLowerInvariant().Trim();
        
        if (!_intentPatterns.ContainsKey(intent))
            return 0.1m;

        var patterns = _intentPatterns[intent];
        var maxScore = 0.0m;

        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(normalizedMessage, pattern, RegexOptions.IgnoreCase))
            {
                // Calculate confidence based on pattern specificity
                var patternWords = pattern.Split(new[] { ' ', '|', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                var messageWords = normalizedMessage.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                
                var matchingWords = patternWords.Intersect(messageWords).Count();
                var score = (decimal)matchingWords / Math.Max(patternWords.Length, messageWords.Length);
                
                maxScore = Math.Max(maxScore, score);
            }
        }

        return Math.Min(maxScore + 0.4m, 1.0m); // Add base confidence and cap at 1.0
    }

    public async Task<string> GenerateResponseAsync(string intent, Dictionary<string, object>? entities = null, Dictionary<string, object>? context = null)
    {
        if (!_responses.ContainsKey(intent))
            return "I'm sorry, I didn't understand that. Could you please rephrase your question?";

        var responses = _responses[intent];
        var random = new Random();
        var response = responses[random.Next(responses.Count)];

        // Replace placeholders with entity values
        if (entities != null)
        {
            foreach (var entity in entities)
            {
                response = response.Replace($"{{{entity.Key}}}", entity.Value?.ToString() ?? "");
            }
        }

        return response;
    }

    public async Task<List<string>> GetSuggestedResponsesAsync(string intent)
    {
        return _suggestedResponses.ContainsKey(intent) 
            ? _suggestedResponses[intent] 
            : new List<string> { "Tell me more", "I need help with something else", "Escalate to human support" };
    }

    public async Task<bool> ShouldEscalateAsync(string message, string intent, decimal confidenceScore)
    {
        // Escalate if confidence is too low
        if (confidenceScore < 0.5m)
            return true;

        // Escalate for specific intents that require human intervention
        var escalationIntents = new[] { "complaint", "urgent_issue", "complex_payroll", "legal_question", "disciplinary_action" };
        if (escalationIntents.Contains(intent))
            return true;

        // Escalate if message contains escalation keywords
        var escalationKeywords = new[] { "speak to manager", "manager", "human agent", "escalate", "supervisor", "urgent", "emergency" };
        var normalizedMessage = message.ToLowerInvariant();
        
        return escalationKeywords.Any(keyword => normalizedMessage.Contains(keyword));
    }

    public async Task TrainModelAsync(List<ChatbotLearningData> trainingData)
    {
        // In a real implementation, this would train an ML model
        // For now, we'll simulate training by updating our pattern matching
        
        foreach (var data in trainingData)
        {
            if (!_intentPatterns.ContainsKey(data.Intent))
            {
                _intentPatterns[data.Intent] = new List<string>();
            }

            // Add user input as a new pattern (simplified)
            var pattern = Regex.Escape(data.UserInput.ToLowerInvariant());
            if (!_intentPatterns[data.Intent].Contains(pattern))
            {
                _intentPatterns[data.Intent].Add(pattern);
            }
        }
    }

    public async Task<bool> IsModelTrainedAsync()
    {
        // For this simple implementation, consider the model always trained
        return true;
    }

    private Dictionary<string, List<string>> InitializeIntentPatterns()
    {
        return new Dictionary<string, List<string>>
        {
            ["greeting"] = new List<string> { @"\b(hi|hello|hey|good morning|good afternoon)\b" },
            ["leave_request"] = new List<string> { 
                @"\b(need|want|apply|request)\b.*\b(leave|vacation|time off|holiday)\b",
                @"\b(leave|vacation|time off|holiday)\b.*\b(request|apply|need)\b",
                @"\bi\s+(need|want)\s+to\s+(request|apply)\s+(for\s+)?(leave|vacation)\b"
            },
            ["leave_balance"] = new List<string> { @"\b(leave|vacation)\b.*\b(balance|remaining|left)\b" },
            ["payroll_inquiry"] = new List<string> { @"\b(salary|payroll|pay|payslip|wages)\b" },
            ["attendance_query"] = new List<string> { @"\b(attendance|check in|check out|working hours)\b" },
            ["project_status"] = new List<string> { @"\b(project|task|assignment)\b.*\b(status|progress|update)\b" },
            ["hr_policy"] = new List<string> { @"\b(policy|policies|rules|guidelines|handbook)\b" },
            ["benefits_inquiry"] = new List<string> { @"\b(benefits|insurance|medical|health|pf|provident fund)\b" },
            ["training_inquiry"] = new List<string> { @"\b(training|course|certification|learning)\b" },
            ["it_support"] = new List<string> { @"\b(password|login|access|computer|laptop|software)\b.*\b(issue|problem|help)\b" },
            ["complaint"] = new List<string> { @"\b(complaint|issue|problem|concern|grievance)\b" },
            ["general_inquiry"] = new List<string> { @".*" }
        };
    }

    private Dictionary<string, List<string>> InitializeResponses()
    {
        return new Dictionary<string, List<string>>
        {
            ["greeting"] = new List<string> 
            { 
                "Hello! I'm your HR assistant. How can I help you today?",
                "Hi there! What can I assist you with?",
                "Good day! I'm here to help with your HR questions."
            },
            ["leave_request"] = new List<string>
            {
                "I can help you with leave requests. You can apply for leave through the employee portal or I can guide you through the process. What type of leave do you need?",
                "To request leave, you'll need to specify the dates and type of leave. Would you like me to check your leave balance first?"
            },
            ["leave_balance"] = new List<string>
            {
                "I can help you check your leave balance. Let me fetch that information for you.",
                "Your leave balance information is available in your employee dashboard. Would you like me to guide you there?"
            },
            ["payroll_inquiry"] = new List<string>
            {
                "I can help with payroll-related questions. What specific information do you need about your salary or payslip?",
                "For payroll inquiries, I can provide information about pay dates, deductions, or help you access your payslip."
            },
            ["attendance_query"] = new List<string>
            {
                "I can help with attendance-related questions. Are you looking to check your attendance record or having issues with check-in/out?",
                "For attendance queries, I can help you view your records or troubleshoot check-in issues."
            },
            ["project_status"] = new List<string>
            {
                "I can help you check project status and assignments. Which project are you asking about?",
                "For project updates, you can check the project dashboard or I can help you find specific information."
            },
            ["hr_policy"] = new List<string>
            {
                "I can help you find HR policies and guidelines. What specific policy are you looking for?",
                "Our HR policies are available in the employee handbook. I can help you locate specific information."
            },
            ["benefits_inquiry"] = new List<string>
            {
                "I can provide information about employee benefits including insurance, PF, and other perks. What would you like to know?",
                "For benefits information, I can help explain coverage, enrollment, or claims processes."
            },
            ["training_inquiry"] = new List<string>
            {
                "I can help with training and development opportunities. Are you looking for available courses or certification programs?",
                "For training inquiries, I can show you available programs or help track your progress."
            },
            ["it_support"] = new List<string>
            {
                "For IT support issues, I can help with basic troubleshooting or connect you with the IT team. What's the problem you're experiencing?",
                "I can assist with common IT issues or create a support ticket for more complex problems."
            },
            ["complaint"] = new List<string>
            {
                "I understand you have a concern. For complaints or grievances, I can guide you through the proper channels or escalate to HR management.",
                "Your concerns are important. I can help you file a formal complaint or connect you with the appropriate HR representative."
            },
            ["general_inquiry"] = new List<string>
            {
                "I'm here to help with HR-related questions. Could you please be more specific about what you need assistance with?",
                "I can help with various HR topics including leave, payroll, policies, and more. What would you like to know?"
            }
        };
    }

    private Dictionary<string, List<string>> InitializeSuggestedResponses()
    {
        return new Dictionary<string, List<string>>
        {
            ["greeting"] = new List<string> { "Check leave balance", "Request time off", "View payslip", "HR policies" },
            ["leave_request"] = new List<string> { "Check leave balance", "View leave policy", "Contact HR" },
            ["leave_balance"] = new List<string> { "Request leave", "View leave history", "Leave policy" },
            ["payroll_inquiry"] = new List<string> { "Download payslip", "Tax information", "Contact payroll team" },
            ["attendance_query"] = new List<string> { "View attendance report", "Check-in help", "Contact supervisor" },
            ["project_status"] = new List<string> { "View project dashboard", "Update task status", "Contact project manager" },
            ["hr_policy"] = new List<string> { "Employee handbook", "Specific policy search", "Contact HR" },
            ["benefits_inquiry"] = new List<string> { "Benefits enrollment", "Claims process", "Contact benefits team" },
            ["training_inquiry"] = new List<string> { "Available courses", "My certifications", "Training calendar" },
            ["it_support"] = new List<string> { "Create IT ticket", "Password reset", "Contact IT team" },
            ["complaint"] = new List<string> { "File formal complaint", "Anonymous feedback", "Speak to HR manager" }
        };
    }

    private void ExtractDateEntities(string message, Dictionary<string, object> entities)
    {
        var datePatterns = new[]
        {
            @"\b(\d{4}-\d{1,2}-\d{1,2})\b", // ISO format like 2025-01-15
            @"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})\b",
            @"\b(today|tomorrow|yesterday)\b",
            @"\b(monday|tuesday|wednesday|thursday|friday|saturday|sunday)\b",
            @"\b(january|february|march|april|may|june|july|august|september|october|november|december)\s+\d{1,2}\b"
        };

        foreach (var pattern in datePatterns)
        {
            var matches = Regex.Matches(message, pattern, RegexOptions.IgnoreCase);
            if (matches.Count > 0)
            {
                entities["date"] = matches[0].Value;
                break;
            }
        }
    }

    private void ExtractNumberEntities(string message, Dictionary<string, object> entities)
    {
        var numberPattern = @"\b(\d+(?:\.\d+)?)\b";
        var matches = Regex.Matches(message, numberPattern);
        
        if (matches.Count > 0)
        {
            entities["number"] = matches[0].Value;
        }
    }

    private void ExtractEmailEntities(string message, Dictionary<string, object> entities)
    {
        var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
        var matches = Regex.Matches(message, emailPattern);
        
        if (matches.Count > 0)
        {
            entities["email"] = matches[0].Value;
        }
    }

    private void ExtractEmployeeIdEntities(string message, Dictionary<string, object> entities)
    {
        var employeeIdPattern = @"\b[A-Z]{2,3}-[A-Z]{2,3}-\d{2,4}-\d{3}\b";
        var matches = Regex.Matches(message, employeeIdPattern);
        
        if (matches.Count > 0)
        {
            entities["employee_id"] = matches[0].Value;
        }
    }

    public async Task<SentimentScore> AnalyzeSentimentAsync(string text)
    {
        // Placeholder implementation for sentiment analysis
        // In a real implementation, this would use ML.NET, Azure Cognitive Services, or similar
        if (string.IsNullOrWhiteSpace(text))
            return SentimentScore.Neutral;

        var lowerText = text.ToLower();
        
        // Simple keyword-based sentiment analysis
        var positiveWords = new[] { "good", "great", "excellent", "amazing", "love", "like", "happy", "satisfied" };
        var negativeWords = new[] { "bad", "terrible", "awful", "hate", "dislike", "angry", "disappointed", "frustrated" };
        
        var positiveCount = positiveWords.Count(word => lowerText.Contains(word));
        var negativeCount = negativeWords.Count(word => lowerText.Contains(word));
        
        if (positiveCount > negativeCount)
            return positiveCount > 2 ? SentimentScore.VeryPositive : SentimentScore.Positive;
        else if (negativeCount > positiveCount)
            return negativeCount > 2 ? SentimentScore.VeryNegative : SentimentScore.Negative;
        else
            return SentimentScore.Neutral;
    }

    public async Task<List<string>> ExtractKeywordsAsync(string text)
    {
        // Placeholder implementation for keyword extraction
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();

        var words = text.ToLower()
            .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 3)
            .Where(word => !IsStopWord(word))
            .GroupBy(word => word)
            .OrderByDescending(group => group.Count())
            .Take(10)
            .Select(group => group.Key)
            .ToList();

        return words;
    }

    public async Task<List<string>> IdentifyThemesAsync(string text)
    {
        // Placeholder implementation for theme identification
        var keywords = await ExtractKeywordsAsync(text);
        
        // Group related keywords into themes (simplified approach)
        var themes = new List<string>();
        
        if (keywords.Any(k => new[] { "work", "job", "career", "position" }.Contains(k)))
            themes.Add("Work Environment");
            
        if (keywords.Any(k => new[] { "team", "colleague", "manager", "supervisor" }.Contains(k)))
            themes.Add("Team Dynamics");
            
        if (keywords.Any(k => new[] { "salary", "pay", "compensation", "benefits" }.Contains(k)))
            themes.Add("Compensation");
            
        if (keywords.Any(k => new[] { "training", "development", "learning", "growth" }.Contains(k)))
            themes.Add("Professional Development");
            
        return themes.Any() ? themes : keywords.Take(3).ToList();
    }

    private static bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> 
        { 
            "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by",
            "is", "are", "was", "were", "be", "been", "have", "has", "had", "do", "does", "did",
            "will", "would", "could", "should", "may", "might", "can", "this", "that", "these", "those"
        };
        
        return stopWords.Contains(word);
    }
}