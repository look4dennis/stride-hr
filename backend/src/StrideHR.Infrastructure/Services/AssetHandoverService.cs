using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Asset;

namespace StrideHR.Infrastructure.Services;

public class AssetHandoverService : IAssetHandoverService
{
    private readonly IAssetHandoverRepository _handoverRepository;
    private readonly IAssetAssignmentRepository _assignmentRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IMapper _mapper;

    public AssetHandoverService(
        IAssetHandoverRepository handoverRepository,
        IAssetAssignmentRepository assignmentRepository,
        IAssetRepository assetRepository,
        IMapper mapper)
    {
        _handoverRepository = handoverRepository;
        _assignmentRepository = assignmentRepository;
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<AssetHandoverDto> InitiateHandoverAsync(CreateAssetHandoverDto handoverDto)
    {
        var asset = await _assetRepository.GetByIdAsync(handoverDto.AssetId);
        if (asset == null || asset.IsDeleted)
        {
            throw new InvalidOperationException($"Asset with ID {handoverDto.AssetId} not found.");
        }

        // Check if asset is assigned to the employee
        var activeAssignment = await _assignmentRepository.GetActiveAssignmentByAssetIdAsync(handoverDto.AssetId);
        if (activeAssignment == null || activeAssignment.EmployeeId != handoverDto.EmployeeId)
        {
            throw new InvalidOperationException("Asset is not currently assigned to the specified employee.");
        }

        // Check for existing pending handover
        if (await _handoverRepository.HasPendingHandoverAsync(handoverDto.AssetId))
        {
            throw new InvalidOperationException("Asset already has a pending handover.");
        }

        var handover = _mapper.Map<AssetHandover>(handoverDto);
        handover.CreatedAt = DateTime.UtcNow;

        await _handoverRepository.AddAsync(handover);
        await _handoverRepository.SaveChangesAsync();

        // Return the handover with all related data
        var createdHandover = await _handoverRepository.GetByIdAsync(handover.Id, 
            h => h.Asset, 
            h => h.Employee, 
            h => h.EmployeeExit, 
            h => h.InitiatedByEmployee);

        return _mapper.Map<AssetHandoverDto>(createdHandover!);
    }

    public async Task<AssetHandoverDto> CompleteHandoverAsync(int id, CompleteAssetHandoverDto completionDto)
    {
        var handover = await _handoverRepository.GetByIdAsync(id);
        if (handover == null || handover.IsDeleted)
        {
            throw new InvalidOperationException($"Handover with ID {id} not found.");
        }

        if (handover.Status != HandoverStatus.Pending)
        {
            throw new InvalidOperationException("Only pending handovers can be completed.");
        }

        // Update handover
        handover.Status = HandoverStatus.Completed;
        handover.CompletedDate = DateTime.UtcNow;
        handover.ReturnedCondition = completionDto.ReturnedCondition;
        handover.HandoverNotes = completionDto.HandoverNotes;
        handover.DamageNotes = completionDto.DamageNotes;
        handover.DamageCharges = completionDto.DamageCharges;
        handover.Currency = completionDto.Currency;
        handover.CompletedBy = completionDto.CompletedBy;
        handover.UpdatedAt = DateTime.UtcNow;

        // Return the asset (end the assignment)
        var activeAssignment = await _assignmentRepository.GetActiveAssignmentByAssetIdAsync(handover.AssetId);
        if (activeAssignment != null)
        {
            activeAssignment.IsActive = false;
            activeAssignment.ReturnDate = DateTime.UtcNow;
            activeAssignment.ReturnedCondition = completionDto.ReturnedCondition;
            activeAssignment.ReturnNotes = completionDto.HandoverNotes;
            activeAssignment.ReturnedBy = completionDto.CompletedBy;
            activeAssignment.UpdatedAt = DateTime.UtcNow;

            await _assignmentRepository.UpdateAsync(activeAssignment);
        }

        // Update asset status
        var asset = await _assetRepository.GetByIdAsync(handover.AssetId);
        if (asset != null)
        {
            asset.Status = AssetStatus.Available;
            asset.Condition = completionDto.ReturnedCondition;
            asset.UpdatedAt = DateTime.UtcNow;
            await _assetRepository.UpdateAsync(asset);
        }

        await _handoverRepository.UpdateAsync(handover);
        await _handoverRepository.SaveChangesAsync();

        return await GetHandoverByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve completed handover.");
    }

    public async Task<AssetHandoverDto> ApproveHandoverAsync(int id, ApproveAssetHandoverDto approvalDto)
    {
        var handover = await _handoverRepository.GetByIdAsync(id);
        if (handover == null || handover.IsDeleted)
        {
            throw new InvalidOperationException($"Handover with ID {id} not found.");
        }

        if (handover.Status != HandoverStatus.Completed)
        {
            throw new InvalidOperationException("Only completed handovers can be approved.");
        }

        if (handover.IsApproved)
        {
            throw new InvalidOperationException("Handover is already approved.");
        }

        handover.IsApproved = true;
        handover.ApprovedBy = approvalDto.ApprovedBy;
        handover.ApprovedDate = DateTime.UtcNow;
        handover.UpdatedAt = DateTime.UtcNow;

        await _handoverRepository.UpdateAsync(handover);
        await _handoverRepository.SaveChangesAsync();

        return await GetHandoverByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve approved handover.");
    }

    public async Task<AssetHandoverDto?> GetHandoverByIdAsync(int id)
    {
        var handover = await _handoverRepository.GetByIdAsync(id, 
            h => h.Asset, 
            h => h.Employee, 
            h => h.EmployeeExit, 
            h => h.InitiatedByEmployee, 
            h => h.CompletedByEmployee, 
            h => h.ApprovedByEmployee);

        return handover != null ? _mapper.Map<AssetHandoverDto>(handover) : null;
    }

    public async Task<IEnumerable<AssetHandoverDto>> GetHandoversByEmployeeIdAsync(int employeeId)
    {
        var handovers = await _handoverRepository.GetHandoversByEmployeeIdAsync(employeeId);
        return _mapper.Map<IEnumerable<AssetHandoverDto>>(handovers);
    }

    public async Task<IEnumerable<AssetHandoverDto>> GetHandoversByAssetIdAsync(int assetId)
    {
        var handovers = await _handoverRepository.GetHandoversByAssetIdAsync(assetId);
        return _mapper.Map<IEnumerable<AssetHandoverDto>>(handovers);
    }

    public async Task<IEnumerable<AssetHandoverDto>> GetHandoversByStatusAsync(HandoverStatus status)
    {
        var handovers = await _handoverRepository.GetHandoversByStatusAsync(status);
        return _mapper.Map<IEnumerable<AssetHandoverDto>>(handovers);
    }

    public async Task<IEnumerable<AssetHandoverDto>> GetPendingHandoversAsync()
    {
        var handovers = await _handoverRepository.GetPendingHandoversAsync();
        return _mapper.Map<IEnumerable<AssetHandoverDto>>(handovers);
    }

    public async Task<IEnumerable<AssetHandoverDto>> GetOverdueHandoversAsync()
    {
        var handovers = await _handoverRepository.GetOverdueHandoversAsync();
        return _mapper.Map<IEnumerable<AssetHandoverDto>>(handovers);
    }

    public async Task<IEnumerable<AssetHandoverDto>> GetHandoversByEmployeeExitAsync(int employeeExitId)
    {
        var handovers = await _handoverRepository.GetHandoversByEmployeeExitAsync(employeeExitId);
        return _mapper.Map<IEnumerable<AssetHandoverDto>>(handovers);
    }

    public async Task<IEnumerable<AssetHandoverDto>> GetHandoversRequiringApprovalAsync()
    {
        var handovers = await _handoverRepository.GetHandoversRequiringApprovalAsync();
        return _mapper.Map<IEnumerable<AssetHandoverDto>>(handovers);
    }

    public async Task<bool> CancelHandoverAsync(int id, int cancelledBy)
    {
        var handover = await _handoverRepository.GetByIdAsync(id);
        if (handover == null || handover.IsDeleted)
        {
            return false;
        }

        if (handover.Status != HandoverStatus.Pending)
        {
            throw new InvalidOperationException("Only pending handovers can be cancelled.");
        }

        handover.Status = HandoverStatus.Cancelled;
        handover.UpdatedAt = DateTime.UtcNow;

        await _handoverRepository.UpdateAsync(handover);
        return await _handoverRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<AssetHandoverDto>> InitiateEmployeeExitHandoversAsync(int employeeId, int employeeExitId, int initiatedBy)
    {
        // Get all active assignments for the employee
        var activeAssignments = await _assignmentRepository.GetAssignmentsByEmployeeIdAsync(employeeId);
        var activeAssetAssignments = activeAssignments.Where(aa => aa.IsActive).ToList();

        var handovers = new List<AssetHandover>();

        foreach (var assignment in activeAssetAssignments)
        {
            // Check if handover already exists
            if (await _handoverRepository.HasPendingHandoverAsync(assignment.AssetId))
            {
                continue;
            }

            var handover = new AssetHandover
            {
                AssetId = assignment.AssetId,
                EmployeeId = employeeId,
                EmployeeExitId = employeeExitId,
                Status = HandoverStatus.Pending,
                InitiatedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7), // Default 7 days to return
                InitiatedBy = initiatedBy,
                CreatedAt = DateTime.UtcNow
            };

            handovers.Add(handover);
        }

        if (handovers.Any())
        {
            await _handoverRepository.AddRangeAsync(handovers);
            await _handoverRepository.SaveChangesAsync();

            // Reload with related data
            var createdHandovers = new List<AssetHandover>();
            foreach (var handover in handovers)
            {
                var reloadedHandover = await _handoverRepository.GetByIdAsync(handover.Id, 
                    h => h.Asset, 
                    h => h.Employee, 
                    h => h.EmployeeExit, 
                    h => h.InitiatedByEmployee);
                
                if (reloadedHandover != null)
                {
                    createdHandovers.Add(reloadedHandover);
                }
            }

            return _mapper.Map<IEnumerable<AssetHandoverDto>>(createdHandovers);
        }

        return new List<AssetHandoverDto>();
    }
}