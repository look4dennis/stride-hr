using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StrideHR.Core.Entities;
using StrideHR.Core.Enums;
using StrideHR.Core.Interfaces.Repositories;
using StrideHR.Core.Interfaces.Services;
using StrideHR.Core.Models;
using Cronos;

namespace StrideHR.Infrastructure.Services;

public class ReportSchedulingService : IReportSchedulingService
{
    private readonly IReportScheduleRepository _scheduleRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IReportBuilderService _reportBuilderService;
    private readonly IReportExportService _exportService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReportSchedulingService> _logger;

    public ReportSchedulingService(
        IReportScheduleRepository scheduleRepository,
        IReportRepository reportRepository,
        IReportBuilderService reportBuilderService,
        IReportExportService exportService,
        IEmailService emailService,
        ILogger<ReportSchedulingService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _reportRepository = reportRepository;
        _reportBuilderService = reportBuilderService;
        _exportService = exportService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ReportSchedule> CreateScheduleAsync(ReportScheduleRequest request, int userId)
    {
        var report = await _reportRepository.GetByIdAsync(request.ReportId);
        if (report == null)
            throw new ArgumentException("Report not found");

        if (report.CreatedBy != userId)
            throw new UnauthorizedAccessException("User does not have permission to schedule this report");

        if (!await ValidateCronExpressionAsync(request.CronExpression))
            throw new ArgumentException("Invalid cron expression");

        var nextRunTime = await GetNextRunTimeAsync(request.CronExpression);

        var schedule = new ReportSchedule
        {
            ReportId = request.ReportId,
            Name = request.Name,
            CronExpression = request.CronExpression,
            IsActive = true,
            NextRunTime = nextRunTime,
            Parameters = request.Parameters != null ? JsonConvert.SerializeObject(request.Parameters) : null,
            Recipients = JsonConvert.SerializeObject(request.Recipients),
            ExportFormat = request.ExportFormat,
            EmailSubject = request.EmailSubject,
            EmailBody = request.EmailBody,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _scheduleRepository.AddAsync(schedule);
        await _scheduleRepository.SaveChangesAsync();

        _logger.LogInformation("Report schedule created: {ScheduleId} for report {ReportId}", schedule.Id, request.ReportId);
        return schedule;
    }

    public async Task<ReportSchedule> UpdateScheduleAsync(int scheduleId, ReportScheduleRequest request, int userId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
            throw new ArgumentException("Schedule not found");

        if (schedule.CreatedBy != userId)
            throw new UnauthorizedAccessException("User does not have permission to update this schedule");

        if (!await ValidateCronExpressionAsync(request.CronExpression))
            throw new ArgumentException("Invalid cron expression");

        var nextRunTime = await GetNextRunTimeAsync(request.CronExpression);

        schedule.Name = request.Name;
        schedule.CronExpression = request.CronExpression;
        schedule.NextRunTime = nextRunTime;
        schedule.Parameters = request.Parameters != null ? JsonConvert.SerializeObject(request.Parameters) : null;
        schedule.Recipients = JsonConvert.SerializeObject(request.Recipients);
        schedule.ExportFormat = request.ExportFormat;
        schedule.EmailSubject = request.EmailSubject;
        schedule.EmailBody = request.EmailBody;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateAsync(schedule);
        await _scheduleRepository.SaveChangesAsync();

        _logger.LogInformation("Report schedule updated: {ScheduleId}", scheduleId);
        return schedule;
    }

    public async Task<bool> DeleteScheduleAsync(int scheduleId, int userId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null)
            return false;

        if (schedule.CreatedBy != userId)
            throw new UnauthorizedAccessException("User does not have permission to delete this schedule");

        await _scheduleRepository.DeleteAsync(schedule);
        await _scheduleRepository.SaveChangesAsync();

        _logger.LogInformation("Report schedule deleted: {ScheduleId}", scheduleId);
        return true;
    }

    public async Task<ReportSchedule?> GetScheduleAsync(int scheduleId, int userId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null || schedule.CreatedBy != userId)
            return null;

        return schedule;
    }

    public async Task<IEnumerable<ReportSchedule>> GetReportSchedulesAsync(int reportId, int userId)
    {
        var report = await _reportRepository.GetByIdAsync(reportId);
        if (report == null || report.CreatedBy != userId)
            return Enumerable.Empty<ReportSchedule>();

        return await _scheduleRepository.GetSchedulesByReportAsync(reportId);
    }

    public async Task<IEnumerable<ReportSchedule>> GetUserSchedulesAsync(int userId)
    {
        var allSchedules = await _scheduleRepository.GetAllAsync();
        return allSchedules.Where(s => s.CreatedBy == userId);
    }

    public async Task<bool> ActivateScheduleAsync(int scheduleId, int userId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null || schedule.CreatedBy != userId)
            return false;

        schedule.IsActive = true;
        schedule.NextRunTime = await GetNextRunTimeAsync(schedule.CronExpression);
        schedule.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateAsync(schedule);
        await _scheduleRepository.SaveChangesAsync();

        _logger.LogInformation("Report schedule activated: {ScheduleId}", scheduleId);
        return true;
    }

    public async Task<bool> DeactivateScheduleAsync(int scheduleId, int userId)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId);
        if (schedule == null || schedule.CreatedBy != userId)
            return false;

        schedule.IsActive = false;
        schedule.UpdatedAt = DateTime.UtcNow;

        await _scheduleRepository.UpdateAsync(schedule);
        await _scheduleRepository.SaveChangesAsync();

        _logger.LogInformation("Report schedule deactivated: {ScheduleId}", scheduleId);
        return true;
    }

    public async Task ExecuteScheduledReportsAsync()
    {
        var dueSchedules = await _scheduleRepository.GetSchedulesDueForExecutionAsync();
        
        foreach (var schedule in dueSchedules)
        {
            try
            {
                await ExecuteScheduledReportAsync(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute scheduled report: {ScheduleId}", schedule.Id);
            }
        }
    }

    public async Task<DateTime?> GetNextRunTimeAsync(string cronExpression)
    {
        try
        {
            var cron = CronExpression.Parse(cronExpression);
            return cron.GetNextOccurrence(DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse cron expression: {CronExpression}", cronExpression);
            return null;
        }
    }

    public async Task<bool> ValidateCronExpressionAsync(string cronExpression)
    {
        try
        {
            CronExpression.Parse(cronExpression);
            return await Task.FromResult(true);
        }
        catch
        {
            return await Task.FromResult(false);
        }
    }

    private async Task ExecuteScheduledReportAsync(ReportSchedule schedule)
    {
        _logger.LogInformation("Executing scheduled report: {ScheduleId}", schedule.Id);

        try
        {
            // Parse parameters
            var parameters = !string.IsNullOrEmpty(schedule.Parameters) 
                ? JsonConvert.DeserializeObject<Dictionary<string, object>>(schedule.Parameters)
                : null;

            // Execute the report
            var reportData = await _reportBuilderService.ExecuteReportAsync(
                schedule.ReportId, 
                schedule.CreatedBy, 
                parameters);

            // Export the report
            var exportData = await _exportService.ExportReportDataAsync(
                reportData, 
                schedule.ExportFormat, 
                schedule.Report?.Name ?? "Scheduled Report");

            // Parse recipients
            var recipients = JsonConvert.DeserializeObject<List<string>>(schedule.Recipients) ?? new List<string>();

            // Send email with attachment
            if (recipients.Any())
            {
                var fileName = $"{schedule.Report?.Name ?? "Report"}_{DateTime.Now:yyyyMMdd_HHmmss}";
                var fileExtension = await _exportService.GetExportFileExtensionAsync(schedule.ExportFormat);
                
                await _emailService.SendEmailWithAttachmentAsync(
                    recipients,
                    schedule.EmailSubject ?? $"Scheduled Report: {schedule.Report?.Name}",
                    schedule.EmailBody ?? "Please find the attached scheduled report.",
                    exportData,
                    $"{fileName}{fileExtension}",
                    await _exportService.GetExportMimeTypeAsync(schedule.ExportFormat));
            }

            // Update next run time
            var nextRunTime = await GetNextRunTimeAsync(schedule.CronExpression);
            if (nextRunTime.HasValue)
            {
                await _scheduleRepository.UpdateNextRunTimeAsync(schedule.Id, nextRunTime.Value);
            }

            _logger.LogInformation("Scheduled report executed successfully: {ScheduleId}", schedule.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute scheduled report: {ScheduleId}", schedule.Id);
            throw;
        }
    }
}