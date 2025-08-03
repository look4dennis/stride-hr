using Microsoft.EntityFrameworkCore;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Models.KnowledgeBase;
using StrideHR.Infrastructure.Data;

namespace StrideHR.Infrastructure.Repositories;

public class KnowledgeBaseDocumentRepository : Repository<KnowledgeBaseDocument>, IKnowledgeBaseDocumentRepository
{
    public KnowledgeBaseDocumentRepository(StrideHRDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> SearchDocumentsAsync(KnowledgeBaseSearchDto searchDto)
    {
        var query = _dbSet.AsQueryable()
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Include(d => d.Reviewer)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchDto.Query))
        {
            var searchTerm = searchDto.Query.ToLower();
            query = query.Where(d => 
                d.Title.ToLower().Contains(searchTerm) ||
                d.Content.ToLower().Contains(searchTerm) ||
                d.Summary.ToLower().Contains(searchTerm) ||
                d.Tags.Any(t => t.ToLower().Contains(searchTerm)) ||
                d.Keywords.Any(k => k.ToLower().Contains(searchTerm)));
        }

        if (searchDto.CategoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == searchDto.CategoryId.Value);
        }

        if (searchDto.Tags != null && searchDto.Tags.Any())
        {
            query = query.Where(d => searchDto.Tags.Any(tag => d.Tags.Contains(tag)));
        }

        if (searchDto.Status.HasValue)
        {
            query = query.Where(d => d.Status == searchDto.Status.Value);
        }

        if (searchDto.AuthorId.HasValue)
        {
            query = query.Where(d => d.AuthorId == searchDto.AuthorId.Value);
        }

        if (searchDto.CreatedFrom.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= searchDto.CreatedFrom.Value);
        }

        if (searchDto.CreatedTo.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= searchDto.CreatedTo.Value);
        }

        if (searchDto.UpdatedFrom.HasValue)
        {
            query = query.Where(d => d.UpdatedAt >= searchDto.UpdatedFrom.Value);
        }

        if (searchDto.UpdatedTo.HasValue)
        {
            query = query.Where(d => d.UpdatedAt <= searchDto.UpdatedTo.Value);
        }

        if (searchDto.IsPublic.HasValue)
        {
            query = query.Where(d => d.IsPublic == searchDto.IsPublic.Value);
        }

        if (searchDto.IsFeatured.HasValue)
        {
            query = query.Where(d => d.IsFeatured == searchDto.IsFeatured.Value);
        }

        if (searchDto.IsExpired.HasValue)
        {
            if (searchDto.IsExpired.Value)
            {
                query = query.Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < DateTime.UtcNow);
            }
            else
            {
                query = query.Where(d => !d.ExpiryDate.HasValue || d.ExpiryDate.Value >= DateTime.UtcNow);
            }
        }

        // Apply sorting
        query = searchDto.SortBy.ToLower() switch
        {
            "title" => searchDto.SortDirection.ToUpper() == "ASC" 
                ? query.OrderBy(d => d.Title) 
                : query.OrderByDescending(d => d.Title),
            "createdat" => searchDto.SortDirection.ToUpper() == "ASC" 
                ? query.OrderBy(d => d.CreatedAt) 
                : query.OrderByDescending(d => d.CreatedAt),
            "viewcount" => searchDto.SortDirection.ToUpper() == "ASC" 
                ? query.OrderBy(d => d.ViewCount) 
                : query.OrderByDescending(d => d.ViewCount),
            "priority" => searchDto.SortDirection.ToUpper() == "ASC" 
                ? query.OrderBy(d => d.Priority) 
                : query.OrderByDescending(d => d.Priority),
            _ => searchDto.SortDirection.ToUpper() == "ASC" 
                ? query.OrderBy(d => d.UpdatedAt) 
                : query.OrderByDescending(d => d.UpdatedAt)
        };

        // Apply pagination
        var skip = (searchDto.Page - 1) * searchDto.PageSize;
        query = query.Skip(skip).Take(searchDto.PageSize);

        return await query.ToListAsync();
    }

    public async Task<int> GetSearchResultCountAsync(KnowledgeBaseSearchDto searchDto)
    {
        var query = _dbSet.AsQueryable();

        // Apply the same filters as SearchDocumentsAsync (without includes for performance)
        if (!string.IsNullOrWhiteSpace(searchDto.Query))
        {
            var searchTerm = searchDto.Query.ToLower();
            query = query.Where(d => 
                d.Title.ToLower().Contains(searchTerm) ||
                d.Content.ToLower().Contains(searchTerm) ||
                d.Summary.ToLower().Contains(searchTerm) ||
                d.Tags.Any(t => t.ToLower().Contains(searchTerm)) ||
                d.Keywords.Any(k => k.ToLower().Contains(searchTerm)));
        }

        if (searchDto.CategoryId.HasValue)
        {
            query = query.Where(d => d.CategoryId == searchDto.CategoryId.Value);
        }

        if (searchDto.Tags != null && searchDto.Tags.Any())
        {
            query = query.Where(d => searchDto.Tags.Any(tag => d.Tags.Contains(tag)));
        }

        if (searchDto.Status.HasValue)
        {
            query = query.Where(d => d.Status == searchDto.Status.Value);
        }

        if (searchDto.AuthorId.HasValue)
        {
            query = query.Where(d => d.AuthorId == searchDto.AuthorId.Value);
        }

        if (searchDto.CreatedFrom.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= searchDto.CreatedFrom.Value);
        }

        if (searchDto.CreatedTo.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= searchDto.CreatedTo.Value);
        }

        if (searchDto.UpdatedFrom.HasValue)
        {
            query = query.Where(d => d.UpdatedAt >= searchDto.UpdatedFrom.Value);
        }

        if (searchDto.UpdatedTo.HasValue)
        {
            query = query.Where(d => d.UpdatedAt <= searchDto.UpdatedTo.Value);
        }

        if (searchDto.IsPublic.HasValue)
        {
            query = query.Where(d => d.IsPublic == searchDto.IsPublic.Value);
        }

        if (searchDto.IsFeatured.HasValue)
        {
            query = query.Where(d => d.IsFeatured == searchDto.IsFeatured.Value);
        }

        if (searchDto.IsExpired.HasValue)
        {
            if (searchDto.IsExpired.Value)
            {
                query = query.Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < DateTime.UtcNow);
            }
            else
            {
                query = query.Where(d => !d.ExpiryDate.HasValue || d.ExpiryDate.Value >= DateTime.UtcNow);
            }
        }

        return await query.CountAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetDocumentsByAuthorAsync(int authorId)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Where(d => d.AuthorId == authorId)
            .OrderByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetDocumentsByCategoryAsync(int categoryId)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Where(d => d.CategoryId == categoryId && d.Status == DocumentStatus.Published)
            .OrderByDescending(d => d.Priority)
            .ThenByDescending(d => d.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetFeaturedDocumentsAsync()
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Where(d => d.IsFeatured && d.Status == DocumentStatus.Published && d.IsPublic)
            .OrderByDescending(d => d.Priority)
            .ThenByDescending(d => d.ViewCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetRecentDocumentsAsync(int count = 10)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Where(d => d.Status == DocumentStatus.Published && d.IsPublic)
            .OrderByDescending(d => d.PublishedAt ?? d.UpdatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetPopularDocumentsAsync(int count = 10)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Where(d => d.Status == DocumentStatus.Published && d.IsPublic)
            .OrderByDescending(d => d.ViewCount)
            .ThenByDescending(d => d.UpdatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetDocumentVersionsAsync(int documentId)
    {
        var document = await _dbSet.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document == null) return new List<KnowledgeBaseDocument>();

        var parentId = document.ParentDocumentId ?? documentId;
        
        return await _dbSet
            .Include(d => d.Author)
            .Where(d => d.Id == parentId || d.ParentDocumentId == parentId)
            .OrderByDescending(d => d.Version)
            .ToListAsync();
    }

    public async Task<KnowledgeBaseDocument?> GetCurrentVersionAsync(int parentDocumentId)
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Include(d => d.Reviewer)
            .Where(d => (d.Id == parentDocumentId || d.ParentDocumentId == parentDocumentId) && d.IsCurrentVersion)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetPendingApprovalDocumentsAsync()
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Where(d => d.Status == DocumentStatus.PendingReview || d.Status == DocumentStatus.UnderReview)
            .OrderBy(d => d.UpdatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetExpiredDocumentsAsync()
    {
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Where(d => d.ExpiryDate.HasValue && d.ExpiryDate.Value < DateTime.UtcNow && d.Status != DocumentStatus.Expired)
            .ToListAsync();
    }

    public async Task<IEnumerable<KnowledgeBaseDocument>> GetDocumentsExpiringInDaysAsync(int days)
    {
        var expiryDate = DateTime.UtcNow.AddDays(days);
        return await _dbSet
            .Include(d => d.Category)
            .Include(d => d.Author)
            .Where(d => d.ExpiryDate.HasValue && 
                       d.ExpiryDate.Value <= expiryDate && 
                       d.ExpiryDate.Value > DateTime.UtcNow &&
                       d.Status == DocumentStatus.Published)
            .ToListAsync();
    }

    public async Task IncrementViewCountAsync(int documentId)
    {
        var document = await _dbSet.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document != null)
        {
            document.ViewCount++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task IncrementDownloadCountAsync(int documentId)
    {
        var document = await _dbSet.FirstOrDefaultAsync(d => d.Id == documentId);
        if (document != null)
        {
            document.DownloadCount++;
            await _context.SaveChangesAsync();
        }
    }
}