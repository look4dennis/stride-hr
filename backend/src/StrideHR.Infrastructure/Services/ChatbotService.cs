using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Chatbot;
using StrideHR.Core.Models.Notification;
using System.Text.Json;

namespace StrideHR.Infrastructure.Services;

public class ChatbotService : IChatbotService
{
    private readonly IChatbotConversationRepository _conversationRepository;
    private readonly IChatbotMessageRepository _messageRepository;
    private readonly INaturalLanguageProcessingService _nlpService;
    private readonly IChatbotKnowledgeBaseService _knowledgeBaseService;
    private readonly IChatbotLearningService _learningService;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ChatbotService(
        IChatbotConversationRepository conversationRepository,
        IChatbotMessageRepository messageRepository,
        INaturalLanguageProcessingService nlpService,
        IChatbotKnowledgeBaseService knowledgeBaseService,
        IChatbotLearningService learningService,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _nlpService = nlpService;
        _knowledgeBaseService = knowledgeBaseService;
        _learningService = learningService;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ChatbotResponseDto> ProcessMessageAsync(SendMessageDto dto, int employeeId)
    {
        // Get or create conversation
        var conversation = await _conversationRepository.GetBySessionIdAsync(dto.SessionId);
        if (conversation == null)
        {
            conversation = new ChatbotConversation
            {
                SessionId = dto.SessionId,
                EmployeeId = employeeId,
                Status = ChatbotConversationStatus.Active,
                StartedAt = DateTime.UtcNow,
                Topic = "General Inquiry"
            };
            await _conversationRepository.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();
        }

        // Save user message
        var userMessage = new ChatbotMessage
        {
            ConversationId = conversation.Id,
            Content = dto.Message,
            MessageType = dto.MessageType,
            Sender = ChatbotMessageSender.User,
            Timestamp = DateTime.UtcNow
        };
        await _messageRepository.AddAsync(userMessage);

        // Process message with NLP
        var intent = await _nlpService.DetectIntentAsync(dto.Message, dto.Context);
        var entities = await _nlpService.ExtractEntitiesAsync(dto.Message);
        var confidenceScore = await _nlpService.GetConfidenceScoreAsync(dto.Message, intent);

        // Update user message with NLP results
        userMessage.Intent = intent;
        userMessage.ConfidenceScore = confidenceScore;
        userMessage.Entities = JsonSerializer.Serialize(entities);

        // Check if escalation is needed
        var shouldEscalate = await _nlpService.ShouldEscalateAsync(dto.Message, intent, confidenceScore);

        string botResponse;
        var suggestedResponses = new List<string>();

        if (shouldEscalate)
        {
            botResponse = "I understand this might be a complex issue. Let me connect you with a human agent who can better assist you.";
            suggestedResponses.Add("Yes, escalate to human");
            suggestedResponses.Add("Let me try rephrasing");
        }
        else
        {
            // Try to get response from knowledge base first
            var relevantArticles = await _knowledgeBaseService.GetRelevantArticlesAsync(intent, entities);
            
            if (relevantArticles.Any())
            {
                var article = relevantArticles.First();
                botResponse = $"Based on our knowledge base: {article.Content.Substring(0, Math.Min(200, article.Content.Length))}...";
                suggestedResponses.Add("Show full article");
                suggestedResponses.Add("This was helpful");
                suggestedResponses.Add("I need more help");
            }
            else
            {
                // Generate response using NLP service
                botResponse = await _nlpService.GenerateResponseAsync(intent, entities, dto.Context);
                suggestedResponses = await _nlpService.GetSuggestedResponsesAsync(intent);
            }
        }

        // Save bot response
        var botMessage = new ChatbotMessage
        {
            ConversationId = conversation.Id,
            Content = botResponse,
            MessageType = ChatbotMessageType.Text,
            Sender = ChatbotMessageSender.Bot,
            Timestamp = DateTime.UtcNow,
            Intent = intent,
            ConfidenceScore = confidenceScore
        };
        await _messageRepository.AddAsync(botMessage);

        // Record interaction for learning
        await _learningService.RecordInteractionAsync(
            dto.Message, 
            botResponse, 
            intent, 
            confidenceScore, 
            employeeId, 
            dto.SessionId);

        await _unitOfWork.SaveChangesAsync();

        return new ChatbotResponseDto
        {
            SessionId = dto.SessionId,
            Response = botResponse,
            MessageType = ChatbotMessageType.Text,
            Intent = intent,
            ConfidenceScore = confidenceScore,
            Entities = entities,
            SuggestedResponses = suggestedResponses,
            ShouldEscalate = shouldEscalate,
            EscalationReason = shouldEscalate ? "Low confidence or complex issue detected" : null
        };
    }

    public async Task<ChatbotConversationDto> StartConversationAsync(int employeeId)
    {
        // Check if there's already an active conversation
        var existingConversation = await _conversationRepository.GetActiveConversationByEmployeeAsync(employeeId);
        if (existingConversation != null)
        {
            return _mapper.Map<ChatbotConversationDto>(existingConversation);
        }

        // Create new conversation
        var sessionId = Guid.NewGuid().ToString();
        var conversation = new ChatbotConversation
        {
            SessionId = sessionId,
            EmployeeId = employeeId,
            Status = ChatbotConversationStatus.Active,
            StartedAt = DateTime.UtcNow,
            Topic = "New Conversation"
        };

        await _conversationRepository.AddAsync(conversation);

        // Add welcome message
        var welcomeMessage = new ChatbotMessage
        {
            ConversationId = conversation.Id,
            Content = "Hello! I'm your HR assistant. How can I help you today?",
            MessageType = ChatbotMessageType.Text,
            Sender = ChatbotMessageSender.Bot,
            Timestamp = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(welcomeMessage);
        await _unitOfWork.SaveChangesAsync();

        var conversationDto = _mapper.Map<ChatbotConversationDto>(conversation);
        conversationDto.Messages.Add(_mapper.Map<ChatbotMessageDto>(welcomeMessage));

        return conversationDto;
    }

    public async Task<ChatbotConversationDto> GetConversationAsync(string sessionId)
    {
        var conversation = await _conversationRepository.GetBySessionIdAsync(sessionId);
        if (conversation == null)
            throw new ArgumentException("Conversation not found", nameof(sessionId));

        var conversationWithMessages = await _conversationRepository.GetWithMessagesAsync(conversation.Id);
        return _mapper.Map<ChatbotConversationDto>(conversationWithMessages);
    }

    public async Task<ChatbotConversationDto> EndConversationAsync(string sessionId, int? satisfactionRating = null, string? feedbackComments = null)
    {
        var conversation = await _conversationRepository.GetBySessionIdAsync(sessionId);
        if (conversation == null)
            throw new ArgumentException("Conversation not found", nameof(sessionId));

        conversation.Status = ChatbotConversationStatus.Completed;
        conversation.EndedAt = DateTime.UtcNow;
        conversation.SatisfactionRating = satisfactionRating;
        conversation.FeedbackComments = feedbackComments;

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ChatbotConversationDto>(conversation);
    }

    public async Task<ChatbotConversationDto> EscalateToHumanAsync(EscalateToHumanDto dto, int employeeId)
    {
        var conversation = await _conversationRepository.GetBySessionIdAsync(dto.SessionId);
        if (conversation == null)
            throw new ArgumentException("Conversation not found", nameof(dto.SessionId));

        conversation.Status = ChatbotConversationStatus.EscalatedToHuman;
        conversation.EscalatedToHuman = true;
        conversation.EscalationReason = dto.Reason;
        conversation.EscalatedToEmployeeId = dto.PreferredAgentId;

        // Add escalation message
        var escalationMessage = new ChatbotMessage
        {
            ConversationId = conversation.Id,
            Content = $"This conversation has been escalated to human support. Reason: {dto.Reason}",
            MessageType = ChatbotMessageType.Escalation,
            Sender = ChatbotMessageSender.System,
            Timestamp = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(escalationMessage);

        // Notify HR team about escalation
        var notificationDto = new CreateNotificationDto
        {
            Title = "Chatbot Escalation",
            Message = $"A chatbot conversation has been escalated. Employee: {conversation.Employee?.FirstName} {conversation.Employee?.LastName}, Reason: {dto.Reason}",
            Type = NotificationType.SecurityAlert,
            Priority = NotificationPriority.High,
            UserId = dto.PreferredAgentId ?? 1 // Default to HR admin if no preferred agent
        };
        await _notificationService.CreateNotificationAsync(notificationDto);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ChatbotConversationDto>(conversation);
    }

    public async Task<List<ChatbotConversationDto>> GetEmployeeConversationsAsync(int employeeId, int page = 1, int pageSize = 20)
    {
        var conversations = await _conversationRepository.GetConversationsByEmployeeAsync(employeeId, page, pageSize);
        return _mapper.Map<List<ChatbotConversationDto>>(conversations);
    }

    public async Task<List<ChatbotConversationDto>> GetEscalatedConversationsAsync()
    {
        var conversations = await _conversationRepository.GetEscalatedConversationsAsync();
        return _mapper.Map<List<ChatbotConversationDto>>(conversations);
    }

    public async Task<ChatbotAnalyticsDto> GetAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var conversationStats = await _conversationRepository.GetConversationStatisticsAsync(fromDate, toDate);
        var intentStats = await _messageRepository.GetIntentStatisticsAsync(fromDate, toDate);
        var avgConfidence = await _messageRepository.GetAverageConfidenceScoreAsync();

        var totalConversations = conversationStats.Values.Sum();
        var escalatedCount = conversationStats.ContainsKey("EscalatedToHuman") ? conversationStats["EscalatedToHuman"] : 0;

        return new ChatbotAnalyticsDto
        {
            TotalConversations = totalConversations,
            ActiveConversations = conversationStats.ContainsKey("Active") ? conversationStats["Active"] : 0,
            CompletedConversations = conversationStats.ContainsKey("Completed") ? conversationStats["Completed"] : 0,
            EscalatedConversations = escalatedCount,
            EscalationRate = totalConversations > 0 ? (decimal)escalatedCount / totalConversations * 100 : 0,
            AverageConversationDuration = 15.5m, // This would be calculated from actual data
            AverageMessagesPerConversation = 8.2m, // This would be calculated from actual data
            AverageSatisfactionRating = 4.2m, // This would be calculated from actual data
            TopIntents = intentStats.Take(10).ToDictionary(x => x.Key, x => x.Value),
            TopCategories = new Dictionary<string, int> { { "General", 45 }, { "Leave", 32 }, { "Payroll", 28 } },
            CommonIssues = new List<string> { "Leave balance inquiry", "Payslip access", "Password reset" },
            IntentConfidenceScores = intentStats.ToDictionary(x => x.Key, x => avgConfidence)
        };
    }
}