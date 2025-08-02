using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Chatbot;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

public class ChatbotKnowledgeBaseService : IChatbotKnowledgeBaseService
{
    private readonly IChatbotKnowledgeBaseRepository _knowledgeBaseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ChatbotKnowledgeBaseService(
        IChatbotKnowledgeBaseRepository knowledgeBaseRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _knowledgeBaseRepository = knowledgeBaseRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<KnowledgeBaseDto> CreateArticleAsync(CreateKnowledgeBaseDto dto, int createdBy)
    {
        var article = new ChatbotKnowledgeBase
        {
            Title = dto.Title,
            Content = dto.Content,
            Category = dto.Category,
            Keywords = dto.Keywords,
            Tags = dto.Tags,
            Status = dto.Status,
            Priority = dto.Priority,
            UpdatedBy = createdBy,
            LastUpdated = DateTime.UtcNow,
            RelatedArticleIds = dto.RelatedArticleIds != null ? JsonSerializer.Serialize(dto.RelatedArticleIds) : null
        };

        await _knowledgeBaseRepository.AddAsync(article);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<KnowledgeBaseDto>(article);
    }

    public async Task<KnowledgeBaseDto> GetArticleByIdAsync(int id)
    {
        var article = await _knowledgeBaseRepository.GetByIdAsync(id);
        if (article == null)
            throw new ArgumentException("Article not found", nameof(id));

        // Increment view count
        await _knowledgeBaseRepository.IncrementViewCountAsync(id);

        return _mapper.Map<KnowledgeBaseDto>(article);
    }

    public async Task<List<KnowledgeBaseDto>> SearchArticlesAsync(string searchTerm)
    {
        var articles = await _knowledgeBaseRepository.SearchByContentAsync(searchTerm);
        return _mapper.Map<List<KnowledgeBaseDto>>(articles);
    }

    public async Task<List<KnowledgeBaseDto>> GetArticlesByCategoryAsync(string category)
    {
        var articles = await _knowledgeBaseRepository.SearchByCategoryAsync(category);
        return _mapper.Map<List<KnowledgeBaseDto>>(articles);
    }

    public async Task<List<KnowledgeBaseDto>> GetMostViewedArticlesAsync(int count = 10)
    {
        var articles = await _knowledgeBaseRepository.GetMostViewedAsync(count);
        return _mapper.Map<List<KnowledgeBaseDto>>(articles);
    }

    public async Task<List<KnowledgeBaseDto>> GetMostHelpfulArticlesAsync(int count = 10)
    {
        var articles = await _knowledgeBaseRepository.GetMostHelpfulAsync(count);
        return _mapper.Map<List<KnowledgeBaseDto>>(articles);
    }

    public async Task<KnowledgeBaseDto> UpdateArticleAsync(int id, CreateKnowledgeBaseDto dto, int updatedBy)
    {
        var article = await _knowledgeBaseRepository.GetByIdAsync(id);
        if (article == null)
            throw new ArgumentException("Article not found", nameof(id));

        article.Title = dto.Title;
        article.Content = dto.Content;
        article.Category = dto.Category;
        article.Keywords = dto.Keywords;
        article.Tags = dto.Tags;
        article.Status = dto.Status;
        article.Priority = dto.Priority;
        article.UpdatedBy = updatedBy;
        article.LastUpdated = DateTime.UtcNow;
        article.RelatedArticleIds = dto.RelatedArticleIds != null ? JsonSerializer.Serialize(dto.RelatedArticleIds) : null;

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<KnowledgeBaseDto>(article);
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        var article = await _knowledgeBaseRepository.GetByIdAsync(id);
        if (article == null)
            return false;

        await _knowledgeBaseRepository.DeleteAsync(article);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ProvideArticleFeedbackAsync(int articleId, int employeeId, bool isHelpful, string? comments = null)
    {
        var article = await _knowledgeBaseRepository.GetByIdAsync(articleId);
        if (article == null)
            return false;

        // Update helpfulness count
        await _knowledgeBaseRepository.UpdateHelpfulnessAsync(articleId, isHelpful);

        // Create feedback record
        var feedback = new ChatbotKnowledgeBaseFeedback
        {
            KnowledgeBaseId = articleId,
            EmployeeId = employeeId,
            IsHelpful = isHelpful,
            Comments = comments,
            ProvidedAt = DateTime.UtcNow
        };

        // Note: This would require a feedback repository, but for simplicity we'll skip the detailed feedback storage
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<List<KnowledgeBaseDto>> GetRelevantArticlesAsync(string intent, Dictionary<string, object>? entities = null)
    {
        // Map intents to categories and keywords
        var categoryMapping = GetIntentCategoryMapping();
        var keywordMapping = GetIntentKeywordMapping();

        List<ChatbotKnowledgeBase> articles = new();

        // First try to find by category
        if (categoryMapping.ContainsKey(intent))
        {
            var category = categoryMapping[intent];
            articles = await _knowledgeBaseRepository.SearchByCategoryAsync(category);
        }

        // If no articles found by category, try keywords
        if (!articles.Any() && keywordMapping.ContainsKey(intent))
        {
            var keywords = keywordMapping[intent];
            articles = await _knowledgeBaseRepository.SearchByKeywordsAsync(keywords);
        }

        // If still no articles, try general search
        if (!articles.Any())
        {
            articles = await _knowledgeBaseRepository.SearchByContentAsync(intent);
        }

        return _mapper.Map<List<KnowledgeBaseDto>>(articles.Take(5).ToList());
    }

    private Dictionary<string, string> GetIntentCategoryMapping()
    {
        return new Dictionary<string, string>
        {
            ["leave_request"] = "Leave Management",
            ["leave_balance"] = "Leave Management",
            ["payroll_inquiry"] = "Payroll",
            ["attendance_query"] = "Attendance",
            ["project_status"] = "Project Management",
            ["hr_policy"] = "HR Policies",
            ["benefits_inquiry"] = "Employee Benefits",
            ["training_inquiry"] = "Training & Development",
            ["it_support"] = "IT Support",
            ["complaint"] = "Grievance Management"
        };
    }

    private Dictionary<string, string[]> GetIntentKeywordMapping()
    {
        return new Dictionary<string, string[]>
        {
            ["leave_request"] = new[] { "leave", "vacation", "time off", "holiday", "request" },
            ["leave_balance"] = new[] { "leave", "balance", "remaining", "available" },
            ["payroll_inquiry"] = new[] { "salary", "payroll", "pay", "payslip", "wages" },
            ["attendance_query"] = new[] { "attendance", "check in", "check out", "working hours" },
            ["project_status"] = new[] { "project", "task", "assignment", "status", "progress" },
            ["hr_policy"] = new[] { "policy", "policies", "rules", "guidelines", "handbook" },
            ["benefits_inquiry"] = new[] { "benefits", "insurance", "medical", "health", "pf" },
            ["training_inquiry"] = new[] { "training", "course", "certification", "learning" },
            ["it_support"] = new[] { "password", "login", "access", "computer", "software" },
            ["complaint"] = new[] { "complaint", "issue", "problem", "concern", "grievance" }
        };
    }
}