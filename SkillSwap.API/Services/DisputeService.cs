using Microsoft.AspNetCore.Identity;
using SkillSwap.Core.DTOs.Disputes;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Enums;
using SkillSwap.Core.Exceptions;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Core.Interfaces.Services;

namespace SkillSwap.API.Services;

public class DisputeService : IDisputeService
{
    private readonly IDisputeRepository _disputeRepository;
    private readonly ISwapRepository _swapRepository;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<AppUser> _userManager;

    public DisputeService(IDisputeRepository disputeRepository, ISwapRepository swapRepository, IUserRepository userRepository, UserManager<AppUser> userManager)
    {
        _disputeRepository = disputeRepository;
        _swapRepository = swapRepository;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    public async Task<DisputeDto> CreateDisputeAsync(string currentUserId, CreateDisputeDto dto)
    {
        var swap = await _swapRepository.GetByIdAsync(dto.SwapRequestId);
        if (swap == null)
            throw new NotFoundException("Swap request not found.");

        if (swap.RequesterId != currentUserId && swap.ReceiverId != currentUserId)
            throw new UnauthorizedException("You are not part of this swap.");

        var validStatuses = new[] 
        { 
            SwapStatus.Accepted, 
            SwapStatus.Scheduled, 
            SwapStatus.ValidatedByReceiver,
            SwapStatus.ValidatedByRequester,
            SwapStatus.Completed 
        };

        if (!validStatuses.Contains(swap.Status))
            throw new BadRequestException("You can only report active or completed swaps.");

        string reportedUserId = swap.RequesterId == currentUserId ? swap.ReceiverId : swap.RequesterId;

        // Create the dispute
        var dispute = new Dispute
        {
            SwapRequestId = swap.Id,
            ReporterId = currentUserId,
            ReportedUserId = reportedUserId,
            Reason = dto.Reason,
            Status = DisputeStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var createdDispute = await _disputeRepository.AddAsync(dispute);

        // Change swap status to Disputed
        swap.Status = SwapStatus.Disputed;
        swap.UpdatedAt = DateTime.UtcNow;
        await _swapRepository.UpdateAsync(swap);

        var reporter = await _userRepository.GetByIdAsync(currentUserId);
        var reported = await _userRepository.GetByIdAsync(reportedUserId);

        return new DisputeDto
        {
            Id = createdDispute.Id,
            SwapRequestId = createdDispute.SwapRequestId,
            ReporterId = reporter.Id,
            ReporterName = reporter.FullName,
            ReportedUserId = reported.Id,
            ReportedUserName = reported.FullName,
            Reason = createdDispute.Reason,
            Status = createdDispute.Status,
            CreatedAt = createdDispute.CreatedAt
        };
    }

    public async Task<IEnumerable<DisputeDto>> GetUserDisputesAsync(string userId)
    {
        var disputes = await _disputeRepository.GetDisputesByUserIdAsync(userId);
        return disputes.Select(d => new DisputeDto
        {
            Id = d.Id,
            SwapRequestId = d.SwapRequestId,
            ReporterId = d.ReporterId,
            ReporterName = d.Reporter.FullName,
            ReportedUserId = d.ReportedUserId,
            ReportedUserName = d.ReportedUser.FullName,
            Reason = d.Reason,
            Status = d.Status,
            CreatedAt = d.CreatedAt,
            ResolvedAt = d.ResolvedAt,
            AdminNotes = d.AdminNotes
        });
    }

    public async Task ResolveDisputeAsync(int disputeId, ResolveDisputeDto dto, string adminId)
    {
        var dispute = await _disputeRepository.GetByIdAsync(disputeId) 
            ?? throw new NotFoundException("Dispute not found.");

        if (dispute.Status != DisputeStatus.Pending)
            throw new BadRequestException("This dispute has already been resolved or dismissed.");

        var swap = await _swapRepository.GetByIdAsync(dispute.SwapRequestId);

        switch (dto.Action)
        {
            case DisputeAction.BanUser:
                var reportedUser = await _userManager.FindByIdAsync(dispute.ReportedUserId);
                if (reportedUser != null)
                {
                    await _userManager.SetLockoutEndDateAsync(reportedUser, DateTimeOffset.UtcNow.AddYears(100));
                }
                break;

            case DisputeAction.DeleteSwap:
                if (swap != null)
                {
                    swap.Status = SwapStatus.Cancelled; 
                    swap.UpdatedAt = DateTime.UtcNow;
                    await _swapRepository.UpdateAsync(swap);
                }
                break;

            case DisputeAction.Dismiss:
                break;
        }

        dispute.Status = dto.Action == DisputeAction.Dismiss ? DisputeStatus.Dismissed : DisputeStatus.Resolved; // [cite: 100]
        dispute.AdminNotes = dto.AdminNotes;
        dispute.ResolvedAt = DateTime.UtcNow;

        await _disputeRepository.UpdateAsync(dispute);
    }

    public async Task<IEnumerable<DisputeDto>> GetAllDisputesAsync()
{
    // Fetch all disputes. 
    // Ensure that your repository's GetAllAsync() includes the Reporter and ReportedUser entities.
    var disputes = await _disputeRepository.GetAllAsync();

    return disputes.Select(d => new DisputeDto
    {
        Id = d.Id,
        SwapRequestId = d.SwapRequestId,
        ReporterId = d.ReporterId,
        ReporterName = d.Reporter.FullName,
        ReportedUserId = d.ReportedUserId,
        ReportedUserName = d.ReportedUser.FullName,
        Reason = d.Reason,
        Status = d.Status,
        CreatedAt = d.CreatedAt,
        ResolvedAt = d.ResolvedAt,
        AdminNotes = d.AdminNotes
    });
}
}
