using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Chatbot;
using System.Security.Claims;

namespace StrideHR.API.Controllers;

[Authorize]
public class ChatbotController : BaseController
{
    private readonly IChatbotService _chatbotService;
    private readonly IChatbotKnowledgeBaseService _knowledgeBaseService;
    private readonly IChatbotLearningService _learningService;

    public ChatbotController(
        IChatbotService chatbotService,
        IChatbotKnowledgeBaseService knowledgeBaseService,
        IChatbotLearningService learningService)
    {
        _chatbotService = chatbotService;
        _knowledgeBaseService = knowledgeBaseService;
        _learningService = learningService;
    }

    /// <summary>
    /// Start a new chatbot conversation
    /// </summary>
    [HttpPost("conversations/start")]
    public async Task<IActionResult> StartConversation()
    {
        var employeeId = GetCurrentEmployeeId();
        var conversation = await _chatbotService.StartConversationAsync(employeeId);
        return Success(conversation, "Conversation started successfully");
    }

    /// <summary>
    /// Send a message to the chatbot
    /// </summary>
    [HttpPost("conversations/message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var employeeId = GetCurrentEmployeeId();
        var response = await _chatbotService.ProcessMessageAsync(dto, employeeId);
        return Success(response, "Message processed successfully");
    }

    /// <summary>
    /// Get conversation by session ID
    /// </summary>
    [HttpGet("conversations/{sessionId}")]
    public async Task<IActionResult> GetConversation(string sessionId)
    {
        var conversation = await _chatbotService.GetConversationAsync(sessionId);
        return Success(conversation, "Conversation retrieved successfully");
    }

    /// <summary>
    /// End a conversation
    /// </summary>
    [HttpPost("conversations/{sessionId}/end")]
    public async Task<IActionResult> EndConversation(string sessionId, [FromBody] EndConversationDto dto)
    {
        var conversation = await _chatbotService.EndConversationAsync(sessionId, dto.SatisfactionRating, dto.FeedbackComments);
        return Success(conversation, "Conversation ended successfully");
    }

    /// <summary>
    /// Escalate conversation to human support
    /// </summary>
    [HttpPost("conversations/escalate")]
    public async Task<IActionResult> EscalateToHuman([FromBody] EscalateToHumanDto dto)
    {
        var employeeId = GetCurrentEmployeeId();
        var conversation = await _chatbotService.EscalateToHumanAsync(dto, employeeId);
        return Success(conversation, "Conversation escalated to human support");
    }

    /// <summary>
    /// Get employee's conversation history
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetMyConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var employeeId = GetCurrentEmployeeId();
        var conversations = await _chatbotService.GetEmployeeConversationsAsync(employeeId, page, pageSize);
        return Success(conversations, "Conversations retrieved successfully");
    }

    /// <summary>
    /// Get escalated conversations (HR only)
    /// </summary>
    [HttpGet("conversations/escalated")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> GetEscalatedConversations()
    {
        var conversations = await _chatbotService.GetEscalatedConversationsAsync();
        return Success(conversations, "Escalated conversations retrieved successfully");
    }

    /// <summary>
    /// Get chatbot analytics (HR/Admin only)
    /// </summary>
    [HttpGet("analytics")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> GetAnalytics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        var analytics = await _chatbotService.GetAnalyticsAsync(fromDate, toDate);
        return Success(analytics, "Analytics retrieved successfully");
    }

    /// <summary>
    /// Search knowledge base articles
    /// </summary>
    [HttpGet("knowledge-base/search")]
    public async Task<IActionResult> SearchKnowledgeBase([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Error("Search term is required");

        var articles = await _knowledgeBaseService.SearchArticlesAsync(searchTerm);
        return Success(articles, "Knowledge base articles retrieved successfully");
    }

    /// <summary>
    /// Get knowledge base articles by category
    /// </summary>
    [HttpGet("knowledge-base/category/{category}")]
    public async Task<IActionResult> GetKnowledgeBaseByCategory(string category)
    {
        var articles = await _knowledgeBaseService.GetArticlesByCategoryAsync(category);
        return Success(articles, "Knowledge base articles retrieved successfully");
    }

    /// <summary>
    /// Get most viewed knowledge base articles
    /// </summary>
    [HttpGet("knowledge-base/most-viewed")]
    public async Task<IActionResult> GetMostViewedArticles([FromQuery] int count = 10)
    {
        var articles = await _knowledgeBaseService.GetMostViewedArticlesAsync(count);
        return Success(articles, "Most viewed articles retrieved successfully");
    }

    /// <summary>
    /// Get most helpful knowledge base articles
    /// </summary>
    [HttpGet("knowledge-base/most-helpful")]
    public async Task<IActionResult> GetMostHelpfulArticles([FromQuery] int count = 10)
    {
        var articles = await _knowledgeBaseService.GetMostHelpfulArticlesAsync(count);
        return Success(articles, "Most helpful articles retrieved successfully");
    }

    /// <summary>
    /// Provide feedback on knowledge base article
    /// </summary>
    [HttpPost("knowledge-base/{articleId}/feedback")]
    public async Task<IActionResult> ProvideArticleFeedback(int articleId, [FromBody] ArticleFeedbackDto dto)
    {
        var employeeId = GetCurrentEmployeeId();
        var success = await _knowledgeBaseService.ProvideArticleFeedbackAsync(articleId, employeeId, dto.IsHelpful, dto.Comments);
        
        if (success)
            return Success("Feedback provided successfully");
        else
            return Error("Failed to provide feedback");
    }

    /// <summary>
    /// Create knowledge base article (HR/Admin only)
    /// </summary>
    [HttpPost("knowledge-base")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> CreateKnowledgeBaseArticle([FromBody] CreateKnowledgeBaseDto dto)
    {
        var employeeId = GetCurrentEmployeeId();
        var article = await _knowledgeBaseService.CreateArticleAsync(dto, employeeId);
        return Success(article, "Knowledge base article created successfully");
    }

    /// <summary>
    /// Update knowledge base article (HR/Admin only)
    /// </summary>
    [HttpPut("knowledge-base/{id}")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> UpdateKnowledgeBaseArticle(int id, [FromBody] CreateKnowledgeBaseDto dto)
    {
        var employeeId = GetCurrentEmployeeId();
        var article = await _knowledgeBaseService.UpdateArticleAsync(id, dto, employeeId);
        return Success(article, "Knowledge base article updated successfully");
    }

    /// <summary>
    /// Delete knowledge base article (HR/Admin only)
    /// </summary>
    [HttpDelete("knowledge-base/{id}")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> DeleteKnowledgeBaseArticle(int id)
    {
        var success = await _knowledgeBaseService.DeleteArticleAsync(id);
        
        if (success)
            return Success("Knowledge base article deleted successfully");
        else
            return Error("Failed to delete knowledge base article");
    }

    /// <summary>
    /// Get learning insights (HR/Admin only)
    /// </summary>
    [HttpGet("learning/insights")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> GetLearningInsights()
    {
        var lowConfidenceInteractions = await _learningService.GetLowConfidenceInteractionsAsync();
        var unhelpfulInteractions = await _learningService.GetUnhelpfulInteractionsAsync();
        var accuracyMetrics = await _learningService.GetIntentAccuracyMetricsAsync();
        var suggestions = await _learningService.GetImprovementSuggestionsAsync();

        var insights = new
        {
            LowConfidenceInteractions = lowConfidenceInteractions.Count,
            UnhelpfulInteractions = unhelpfulInteractions.Count,
            IntentAccuracyMetrics = accuracyMetrics,
            ImprovementSuggestions = suggestions
        };

        return Success(insights, "Learning insights retrieved successfully");
    }

    /// <summary>
    /// Retrain chatbot model (HR/Admin only)
    /// </summary>
    [HttpPost("learning/retrain")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> RetrainModel()
    {
        await _learningService.RetrainModelAsync();
        return Success("Model retrained successfully");
    }

    /// <summary>
    /// Process learning data (HR/Admin only)
    /// </summary>
    [HttpPost("learning/process")]
    [Authorize(Roles = "HR,Admin")]
    public async Task<IActionResult> ProcessLearningData()
    {
        await _learningService.ProcessLearningDataAsync();
        return Success("Learning data processed successfully");
    }

    private int GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(employeeIdClaim, out var employeeId))
            return employeeId;
        
        throw new UnauthorizedAccessException("Employee ID not found in token");
    }
}

// Additional DTOs for the controller
public class EndConversationDto
{
    public int? SatisfactionRating { get; set; }
    public string? FeedbackComments { get; set; }
}

public class ArticleFeedbackDto
{
    public bool IsHelpful { get; set; }
    public string? Comments { get; set; }
}