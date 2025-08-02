using AutoMapper;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models.Asset;

namespace StrideHR.Infrastructure.Services;

public class AssetAssignmentService : IAssetAssignmentService
{
    private readonly IAssetAssignmentRepository _assignmentRepository;
    private readonly IAssetRepository _assetRepository;
    private readonly IMapper _mapper;

    public AssetAssignmentService(
        IAssetAssignmentRepository assignmentRepository,
        IAssetRepository assetRepository,
        IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<AssetAssignmentDto> AssignAssetToEmployeeAsync(CreateAssetAssignmentDto assignmentDto)
    {
        if (!assignmentDto.EmployeeId.HasValue)
        {
            throw new ArgumentException("Employee ID is required for employee assignment.");
        }

        return await CreateAssignmentAsync(assignmentDto);
    }

    public async Task<AssetAssignmentDto> AssignAssetToProjectAsync(CreateAssetAssignmentDto assignmentDto)
    {
        if (!assignmentDto.ProjectId.HasValue)
        {
            throw new ArgumentException("Project ID is required for project assignment.");
        }

        return await CreateAssignmentAsync(assignmentDto);
    }

    public async Task<AssetAssignmentDto> ReturnAssetAsync(int assignmentId, ReturnAssetDto returnDto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, 
            aa => aa.Asset, 
            aa => aa.Employee, 
            aa => aa.Project, 
            aa => aa.AssignedByEmployee, 
            aa => aa.ReturnedByEmployee);

        if (assignment == null || assignment.IsDeleted)
        {
            throw new InvalidOperationException($"Assignment with ID {assignmentId} not found.");
        }

        if (!assignment.IsActive)
        {
            throw new InvalidOperationException("Assignment is already returned.");
        }

        // Update assignment
        assignment.IsActive = false;
        assignment.ReturnDate = DateTime.UtcNow;
        assignment.ReturnedCondition = returnDto.ReturnedCondition;
        assignment.ReturnNotes = returnDto.ReturnNotes;
        assignment.ReturnedBy = returnDto.ReturnedBy;
        assignment.UpdatedAt = DateTime.UtcNow;

        // Update asset status
        var asset = assignment.Asset;
        asset.Status = AssetStatus.Available;
        asset.Condition = returnDto.ReturnedCondition;
        asset.UpdatedAt = DateTime.UtcNow;

        await _assignmentRepository.UpdateAsync(assignment);
        await _assetRepository.UpdateAsync(asset);
        await _assignmentRepository.SaveChangesAsync();

        return _mapper.Map<AssetAssignmentDto>(assignment);
    }

    public async Task<AssetAssignmentDto?> GetAssignmentByIdAsync(int id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id, 
            aa => aa.Asset, 
            aa => aa.Employee, 
            aa => aa.Project, 
            aa => aa.AssignedByEmployee, 
            aa => aa.ReturnedByEmployee);

        return assignment != null ? _mapper.Map<AssetAssignmentDto>(assignment) : null;
    }

    public async Task<AssetAssignmentDto?> GetActiveAssignmentByAssetIdAsync(int assetId)
    {
        var assignment = await _assignmentRepository.GetActiveAssignmentByAssetIdAsync(assetId);
        return assignment != null ? _mapper.Map<AssetAssignmentDto>(assignment) : null;
    }

    public async Task<IEnumerable<AssetAssignmentDto>> GetAssignmentsByEmployeeIdAsync(int employeeId)
    {
        var assignments = await _assignmentRepository.GetAssignmentsByEmployeeIdAsync(employeeId);
        return _mapper.Map<IEnumerable<AssetAssignmentDto>>(assignments);
    }

    public async Task<IEnumerable<AssetAssignmentDto>> GetAssignmentsByProjectIdAsync(int projectId)
    {
        var assignments = await _assignmentRepository.GetAssignmentsByProjectIdAsync(projectId);
        return _mapper.Map<IEnumerable<AssetAssignmentDto>>(assignments);
    }

    public async Task<IEnumerable<AssetAssignmentDto>> GetAssignmentHistoryByAssetIdAsync(int assetId)
    {
        var assignments = await _assignmentRepository.GetAssignmentHistoryByAssetIdAsync(assetId);
        return _mapper.Map<IEnumerable<AssetAssignmentDto>>(assignments);
    }

    public async Task<IEnumerable<AssetAssignmentDto>> GetActiveAssignmentsAsync()
    {
        var assignments = await _assignmentRepository.GetActiveAssignmentsAsync();
        return _mapper.Map<IEnumerable<AssetAssignmentDto>>(assignments);
    }

    public async Task<IEnumerable<AssetAssignmentDto>> GetOverdueReturnsAsync()
    {
        var assignments = await _assignmentRepository.GetOverdueReturnsAsync();
        return _mapper.Map<IEnumerable<AssetAssignmentDto>>(assignments);
    }

    public async Task<bool> CanAssignAssetAsync(int assetId)
    {
        var asset = await _assetRepository.GetByIdAsync(assetId);
        if (asset == null || asset.IsDeleted)
        {
            return false;
        }

        // Asset must be available and not have active assignments
        return asset.Status == AssetStatus.Available && 
               !await _assignmentRepository.HasActiveAssignmentAsync(assetId);
    }

    private async Task<AssetAssignmentDto> CreateAssignmentAsync(CreateAssetAssignmentDto assignmentDto)
    {
        // Validate asset can be assigned
        if (!await CanAssignAssetAsync(assignmentDto.AssetId))
        {
            throw new InvalidOperationException("Asset is not available for assignment.");
        }

        // Check for existing active assignment
        if (await _assignmentRepository.HasActiveAssignmentAsync(assignmentDto.AssetId))
        {
            throw new InvalidOperationException("Asset already has an active assignment.");
        }

        var assignment = _mapper.Map<AssetAssignment>(assignmentDto);
        assignment.CreatedAt = DateTime.UtcNow;

        // Update asset status
        var asset = await _assetRepository.GetByIdAsync(assignmentDto.AssetId);
        if (asset != null)
        {
            asset.Status = AssetStatus.Assigned;
            asset.Condition = assignmentDto.AssignedCondition;
            asset.UpdatedAt = DateTime.UtcNow;
            await _assetRepository.UpdateAsync(asset);
        }

        await _assignmentRepository.AddAsync(assignment);
        await _assignmentRepository.SaveChangesAsync();

        // Return the assignment with all related data
        var createdAssignment = await _assignmentRepository.GetByIdAsync(assignment.Id, 
            aa => aa.Asset, 
            aa => aa.Employee, 
            aa => aa.Project, 
            aa => aa.AssignedByEmployee);

        return _mapper.Map<AssetAssignmentDto>(createdAssignment!);
    }
}