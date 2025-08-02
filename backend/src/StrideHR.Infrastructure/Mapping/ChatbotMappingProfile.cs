using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Models.Chatbot;
using System.Text.Json;

namespace StrideHR.Infrastructure.Mapping;

public class ChatbotMappingProfile : Profile
{
    public ChatbotMappingProfile()
    {
        CreateMap<ChatbotConversation, ChatbotConversationDto>()
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => $"{src.Employee.FirstName} {src.Employee.LastName}"))
            .ForMember(dest => dest.EscalatedToEmployeeName, opt => opt.MapFrom(src => src.EscalatedToEmployee != null ? $"{src.EscalatedToEmployee.FirstName} {src.EscalatedToEmployee.LastName}" : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.MessageCount, opt => opt.MapFrom(src => src.Messages.Count))
            .ForMember(dest => dest.Messages, opt => opt.MapFrom(src => src.Messages));

        CreateMap<ChatbotMessage, ChatbotMessageDto>()
            .ForMember(dest => dest.MessageTypeName, opt => opt.MapFrom(src => src.MessageType.ToString()))
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender.ToString()))
            .ForMember(dest => dest.Entities, opt => opt.MapFrom(src => DeserializeEntities(src.Entities)))
            .ForMember(dest => dest.ActionData, opt => opt.MapFrom(src => DeserializeActionData(src.ActionData)));

        CreateMap<ChatbotKnowledgeBase, KnowledgeBaseDto>()
            .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => $"{src.UpdatedByEmployee.FirstName} {src.UpdatedByEmployee.LastName}"))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RelatedArticleIds, opt => opt.MapFrom(src => DeserializeRelatedArticleIds(src.RelatedArticleIds)))
            .ForMember(dest => dest.HelpfulnessRatio, opt => opt.MapFrom(src => CalculateHelpfulnessRatio(src.HelpfulCount, src.NotHelpfulCount)));

        CreateMap<CreateKnowledgeBaseDto, ChatbotKnowledgeBase>()
            .ForMember(dest => dest.RelatedArticleIds, opt => opt.MapFrom(src => SerializeRelatedArticleIds(src.RelatedArticleIds)));
    }

    private static Dictionary<string, object>? DeserializeEntities(string? entitiesJson)
    {
        if (string.IsNullOrEmpty(entitiesJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(entitiesJson);
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, object>? DeserializeActionData(string? actionDataJson)
    {
        if (string.IsNullOrEmpty(actionDataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(actionDataJson);
        }
        catch
        {
            return null;
        }
    }

    private static List<int>? DeserializeRelatedArticleIds(string? relatedArticleIdsJson)
    {
        if (string.IsNullOrEmpty(relatedArticleIdsJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<int>>(relatedArticleIdsJson);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializeRelatedArticleIds(List<int>? relatedArticleIds)
    {
        if (relatedArticleIds == null || !relatedArticleIds.Any())
            return null;

        return JsonSerializer.Serialize(relatedArticleIds);
    }

    private static decimal CalculateHelpfulnessRatio(int helpfulCount, int notHelpfulCount)
    {
        var totalFeedback = helpfulCount + notHelpfulCount;
        if (totalFeedback == 0)
            return 0;

        return (decimal)helpfulCount / totalFeedback * 100;
    }
}