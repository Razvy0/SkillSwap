using SkillSync.Core.DTOs.Swaps;
using SkillSync.Core.Entities;
using SkillSync.Core.Enums;
using SkillSync.Core.Exceptions;
using SkillSync.Core.Interfaces.Repositories;
using SkillSync.Core.Interfaces.Services;
using SkillSync.Infrastructure.Data;

namespace SkillSync.API.Services;

public class SwapService : ISwapService
{
    private const int SessionDurationMinutes = 60;
    private readonly ISwapRepository _swapRepo;
    private readonly ISkillRepository _skillRepo;
    private readonly IUserRepository _userRepo;
    private readonly AppDbContext _context;

    public SwapService(ISwapRepository swapRepo, ISkillRepository skillRepo, IUserRepository userRepo, AppDbContext context)
    {
        _swapRepo = swapRepo;
        _skillRepo = skillRepo;
        _userRepo = userRepo;
        _context = context;
    }

    public async Task<IEnumerable<SwapDto>> GetUserSwapsAsync(string userId)
    {
        var swaps = await _swapRepo.GetSwapsByUserIdAsync(userId);
        return swaps.Select(MapToDto);
    }

    public async Task<SwapDto> CreateSwapAsync(string requesterId, CreateSwapDto dto)
    {
        Skill? offeredSkill = null;
        if (dto.LessonType == LessonType.Exchange)
        {
            if (!dto.OfferedSkillId.HasValue)
                throw new BadRequestException("Select a skill to offer for a skill exchange.");

            offeredSkill = await _skillRepo.GetSkillWithDetailsAsync(dto.OfferedSkillId.Value)
                ?? throw new NotFoundException("Skill", dto.OfferedSkillId.Value);

            if (offeredSkill.UserId != requesterId)
                throw new BadRequestException("You can only offer your own skills.");
        }

        var requestedSkill = await _skillRepo.GetSkillWithDetailsAsync(dto.RequestedSkillId)
            ?? throw new NotFoundException("Skill", dto.RequestedSkillId);

        if (requestedSkill.UserId == requesterId)
            throw new BadRequestException("You cannot request your own skill.");

        var requester = await _userRepo.GetByIdAsync(requesterId)
            ?? throw new NotFoundException("User", requesterId);

        ValidateCadenceForSkill(requestedSkill, dto.RequestedCadence, "requested");
        var requestedSessionCount = requestedSkill.RequiredSessions;

        if (dto.LessonType == LessonType.Exchange)
        {
            if (dto.OfferedCadence == null)
                throw new BadRequestException("Select a cadence for the offered skill.");

            if (offeredSkill == null)
                throw new BadRequestException("Select a skill to offer for a skill exchange.");

            ValidateCadenceForSkill(offeredSkill, dto.OfferedCadence.Value, "offered");

            if (dto.TwoWayScheduleMode == TwoWayScheduleMode.Consecutive)
            {
                if (dto.OfferedCadence.Value != dto.RequestedCadence)
                    throw new BadRequestException("Consecutive exchanges require matching cadences.");

                if (offeredSkill.RequiredSessions != requestedSessionCount)
                    throw new BadRequestException("Consecutive exchanges require matching session counts.");
            }
        }
        else if (dto.OfferedCadence != null)
        {
            throw new BadRequestException("Offered cadence is only allowed for exchanges.");
        }

        if (dto.LessonType == LessonType.OneWay && requester.TimeBalance < requestedSessionCount)
            throw new BadRequestException("Insufficient time balance to request this lesson series.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var swap = new SwapRequest
            {
                RequesterId = requesterId,
                ReceiverId = requestedSkill.UserId,
                LessonType = dto.LessonType,
                RequestedCadence = dto.RequestedCadence,
                OfferedCadence = dto.OfferedCadence,
                TwoWayScheduleMode = dto.LessonType == LessonType.Exchange
                    ? dto.TwoWayScheduleMode
                    : TwoWayScheduleMode.Separate,
                TeacherId = dto.LessonType == LessonType.OneWay ? requestedSkill.UserId : null,
                LearnerId = dto.LessonType == LessonType.OneWay ? requesterId : null,
                OfferedSkillId = dto.LessonType == LessonType.Exchange ? dto.OfferedSkillId : null,
                RequestedSkillId = dto.RequestedSkillId,
                Status = SwapStatus.Pending
            };

            await _swapRepo.AddAsync(swap);

            await transaction.CommitAsync();

            var created = await _swapRepo.GetSwapWithDetailsAsync(swap.Id);
            return MapToDto(created!);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SwapDto> UpdateSwapStatusAsync(string userId, int swapId, UpdateSwapStatusDto dto)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        if (swap.Sessions.Any())
            throw new BadRequestException("This swap uses per-session scheduling. Invalidate sessions instead.");

        if (swap.Sessions.Any())
            throw new BadRequestException("This swap uses per-session scheduling. Validate sessions instead.");

        if (swap.ReceiverId != userId && swap.RequesterId != userId)
            throw new UnauthorizedException("You are not a participant in this swap.");

        if (dto.Status == SwapStatus.Accepted && swap.ReceiverId != userId)
            throw new BadRequestException("Only the receiver can accept a swap request.");

        if (dto.Status == SwapStatus.Accepted && swap.Status != SwapStatus.Pending)
            throw new BadRequestException("Only pending swaps can be accepted.");

        if (dto.Status == SwapStatus.Rejected && swap.Status != SwapStatus.Pending)
            throw new BadRequestException("Only pending swaps can be rejected.");

        if (dto.Status == SwapStatus.Cancelled && swap.Status != SwapStatus.Pending)
            throw new BadRequestException("Only pending swaps can be cancelled.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            swap.Status = dto.Status;
            swap.UpdatedAt = DateTime.UtcNow;
            await _swapRepo.UpdateAsync(swap);

            if ((dto.Status == SwapStatus.Rejected || dto.Status == SwapStatus.Cancelled)
                && swap.LessonType == LessonType.OneWay)
            {
                var requester = await _userRepo.GetByIdAsync(swap.RequesterId);
                if (requester != null)
                {
                    requester.TimeBalance += 1;
                    await _userRepo.UpdateAsync(requester);

                    var timeTx = new TimeTransaction
                    {
                        UserId = swap.RequesterId,
                        Amount = 1,
                        TransactionType = TransactionType.Refunded,
                        SwapRequestId = swap.Id
                    };
                    _context.TimeTransactions.Add(timeTx);
                    await _context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
            return MapToDto(swap);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SwapDto> ProposeTimeSlotAsync(string userId, int swapId, ProposeTimeSlotDto dto)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        if (swap.ReceiverId != userId)
            throw new BadRequestException("Only the receiver can propose a time slot.");

        if (swap.Status != SwapStatus.Accepted)
            throw new BadRequestException("Time slots can only be proposed for accepted swaps.");

        if (dto.TimeSlotEnd <= dto.TimeSlotStart)
            throw new BadRequestException("End time must be after start time.");

        swap.TimeSlotStart = dto.TimeSlotStart;
        swap.TimeSlotEnd = dto.TimeSlotEnd;
        swap.UpdatedAt = DateTime.UtcNow;
        await _swapRepo.UpdateAsync(swap);

        return MapToDto(swap);
    }

    public async Task<SwapDto> PickTimeAsync(string userId, int swapId, PickTimeDto dto)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        if (swap.RequesterId != userId)
            throw new BadRequestException("Only the requester can pick a meeting time.");

        if (swap.Status != SwapStatus.Accepted)
            throw new BadRequestException("Swap must be accepted to pick a time.");

        if (swap.TimeSlotStart == null || swap.TimeSlotEnd == null)
            throw new BadRequestException("The receiver must propose a time slot first.");

        if (dto.ScheduledDate < swap.TimeSlotStart || dto.ScheduledDate > swap.TimeSlotEnd)
            throw new BadRequestException("Selected time must be within the proposed time slot.");

        swap.ScheduledDate = dto.ScheduledDate;
        swap.Status = SwapStatus.Scheduled;
        swap.UpdatedAt = DateTime.UtcNow;
        await _swapRepo.UpdateAsync(swap);

        return MapToDto(swap);
    }

    public async Task<SwapDto> ProposeScheduleAsync(string userId, int swapId, ProposeScheduleDto dto)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        if (swap.ReceiverId != userId)
            throw new BadRequestException("Only the receiver can propose a schedule.");

        if (swap.Status != SwapStatus.Accepted)
            throw new BadRequestException("Schedules can only be proposed for accepted swaps.");

        if (dto.Tracks.Count == 0)
            throw new BadRequestException("Provide at least one schedule proposal.");

        var requestedSkill = swap.RequestedSkill;
        var offeredSkill = swap.OfferedSkill;

        var requestedProposal = dto.Tracks.FirstOrDefault(t => t.Track == SwapTrack.RequestedSkill);
        var offeredProposal = dto.Tracks.FirstOrDefault(t => t.Track == SwapTrack.OfferedSkill);

        if (swap.LessonType == LessonType.OneWay && offeredProposal != null)
            throw new BadRequestException("Only requested skill scheduling is allowed for one-way swaps.");

        if (swap.LessonType == LessonType.Exchange && swap.TwoWayScheduleMode == TwoWayScheduleMode.Separate)
        {
            if (requestedProposal == null || offeredProposal == null)
                throw new BadRequestException("Provide schedules for both skills in a separate exchange.");
        }

        if (requestedProposal == null)
            throw new BadRequestException("Provide a schedule for the requested skill.");

        ValidateProposalForTrack(swap, requestedProposal, requestedSkill, swap.RequestedCadence, "requested");

        if (swap.LessonType == LessonType.Exchange && offeredSkill != null)
        {
            if (swap.TwoWayScheduleMode == TwoWayScheduleMode.Separate)
            {
                ValidateProposalForTrack(swap, offeredProposal!, offeredSkill, swap.OfferedCadence ?? LessonCadence.Single, "offered");
            }
            else
            {
                if (swap.OfferedCadence != swap.RequestedCadence)
                    throw new BadRequestException("Consecutive scheduling requires matching cadences.");

                if (offeredSkill.RequiredSessions != requestedSkill.RequiredSessions)
                    throw new BadRequestException("Consecutive scheduling requires matching session counts.");
            }
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (swap.Schedules.Count > 0)
                _context.SwapSchedules.RemoveRange(swap.Schedules);

            if (swap.Sessions.Count > 0)
                _context.SwapSessions.RemoveRange(swap.Sessions);

            await _context.SaveChangesAsync();

            var schedules = new List<SwapSchedule>();
            var sessions = new List<SwapSession>();

            var requestedSchedule = BuildScheduleEntity(swap, requestedProposal, userId);
            schedules.Add(requestedSchedule);

            sessions.AddRange(BuildSessionsFromProposal(swap, requestedProposal, requestedSchedule));

            if (swap.LessonType == LessonType.Exchange && offeredSkill != null)
            {
                if (swap.TwoWayScheduleMode == TwoWayScheduleMode.Separate)
                {
                    var offeredSchedule = BuildScheduleEntity(swap, offeredProposal!, userId);
                    schedules.Add(offeredSchedule);
                    sessions.AddRange(BuildSessionsFromProposal(swap, offeredProposal!, offeredSchedule));
                }
                else
                {
                    var offeredSchedule = BuildScheduleEntity(swap, requestedProposal, userId);
                    offeredSchedule.Track = SwapTrack.OfferedSkill;
                    schedules.Add(offeredSchedule);

                    var offsetSessions = sessions
                        .Where(s => s.Track == SwapTrack.RequestedSkill)
                        .Select((s, index) => new SwapSession
                        {
                            SwapRequestId = swap.Id,
                            SwapSchedule = offeredSchedule,
                            Track = SwapTrack.OfferedSkill,
                            SessionOrder = index + 1,
                            StartTime = s.StartTime.AddMinutes(SessionDurationMinutes),
                            EndTime = s.EndTime.AddMinutes(SessionDurationMinutes),
                            Status = SwapSessionStatus.Proposed
                        })
                        .ToList();

                    sessions.AddRange(offsetSessions);
                }
            }

            _context.SwapSchedules.AddRange(schedules);
            _context.SwapSessions.AddRange(sessions);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var updated = await _swapRepo.GetSwapWithDetailsAsync(swapId);
        return MapToDto(updated!);
    }

    public async Task<SwapDto> ConfirmScheduleAsync(string userId, int swapId)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        if (swap.RequesterId != userId)
            throw new BadRequestException("Only the requester can confirm a schedule.");

        if (swap.Status != SwapStatus.Accepted)
            throw new BadRequestException("Schedules can only be confirmed for accepted swaps.");

        if (!swap.Schedules.Any() || !swap.Sessions.Any())
            throw new BadRequestException("No schedule proposal exists for this swap.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var schedule in swap.Schedules)
            {
                schedule.Status = SwapScheduleStatus.Confirmed;
            }

            foreach (var session in swap.Sessions)
            {
                session.Status = SwapSessionStatus.Scheduled;
            }

            swap.Status = SwapStatus.Scheduled;
            swap.UpdatedAt = DateTime.UtcNow;
            await _swapRepo.UpdateAsync(swap);

            if (swap.LessonType == LessonType.OneWay)
            {
                var requester = await _userRepo.GetByIdAsync(swap.RequesterId);
                if (requester == null)
                    throw new NotFoundException("User", swap.RequesterId);

                var sessionCount = swap.Sessions.Count(s => s.Track == SwapTrack.RequestedSkill);

                if (requester.TimeBalance < sessionCount)
                    throw new BadRequestException("Insufficient time balance to confirm this schedule.");

                requester.TimeBalance -= sessionCount;
                await _userRepo.UpdateAsync(requester);

                foreach (var session in swap.Sessions.Where(s => s.Track == SwapTrack.RequestedSkill))
                {
                    var timeTransaction = new TimeTransaction
                    {
                        UserId = requester.Id,
                        Amount = -1,
                        TransactionType = TransactionType.EscrowHold,
                        SwapRequestId = swap.Id,
                        SwapSessionId = session.Id
                    };
                    _context.TimeTransactions.Add(timeTransaction);
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var updated = await _swapRepo.GetSwapWithDetailsAsync(swapId);
        return MapToDto(updated!);
    }

    public async Task<SwapDto> RequestScheduleChangeAsync(string userId, int swapId, RequestScheduleChangeDto dto)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        if (swap.RequesterId != userId)
            throw new BadRequestException("Only the requester can request a schedule change.");

        if (swap.Status != SwapStatus.Accepted)
            throw new BadRequestException("Schedule changes can only be requested for accepted swaps.");

        if (swap.LessonType != LessonType.OneWay || swap.RequestedCadence != LessonCadence.Single)
            throw new BadRequestException("Schedule change requests are only supported for one-way single sessions.");

        if (dto.Track != SwapTrack.RequestedSkill)
            throw new BadRequestException("Only the requested skill schedule can be changed.");

        if (string.IsNullOrWhiteSpace(dto.Note) || dto.Note.Length > 100)
            throw new BadRequestException("Please provide a short note (max 100 characters).");

        var schedule = swap.Schedules.FirstOrDefault(s => s.Track == dto.Track)
            ?? throw new BadRequestException("No schedule proposal exists for this swap.");

        schedule.ChangeRequestNote = dto.Note.Trim();
        schedule.ChangeRequestTime = dto.SuggestedTime;
        schedule.ChangeRequestedById = userId;
        schedule.ChangeRequestedAt = DateTime.UtcNow;

        swap.UpdatedAt = DateTime.UtcNow;
        await _swapRepo.UpdateAsync(swap);

        var updated = await _swapRepo.GetSwapWithDetailsAsync(swapId);
        return MapToDto(updated!);
    }

    public async Task<SwapDto> ValidateSessionAsync(string userId, int swapId, int sessionId)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        var session = swap.Sessions.FirstOrDefault(s => s.Id == sessionId)
            ?? throw new NotFoundException("SwapSession", sessionId);

        if (swap.RequesterId != userId && swap.ReceiverId != userId)
            throw new UnauthorizedException("You are not a participant in this swap.");

        if (session.Status != SwapSessionStatus.Scheduled
            && session.Status != SwapSessionStatus.Completed)
            throw new BadRequestException("Session must be scheduled before it can be validated.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (swap.RequesterId == userId)
                session.RequesterValidated = true;
            else
                session.ReceiverValidated = true;

            var wasCompleted = session.Status == SwapSessionStatus.Completed;
            if (session.RequesterValidated && session.ReceiverValidated)
            {
                session.Status = SwapSessionStatus.Completed;
                session.ValidatedAt = DateTime.UtcNow;
            }

            var allCompleted = swap.Sessions.All(s => s.Status == SwapSessionStatus.Completed);
            if (allCompleted)
            {
                swap.Status = SwapStatus.Completed;
            }

            swap.UpdatedAt = DateTime.UtcNow;
            await _swapRepo.UpdateAsync(swap);

            if (!wasCompleted && session.Status == SwapSessionStatus.Completed && swap.LessonType == LessonType.OneWay)
            {
                var receiver = await _userRepo.GetByIdAsync(swap.ReceiverId);
                if (receiver != null)
                {
                    receiver.TimeBalance += 1;
                    await _userRepo.UpdateAsync(receiver);

                    var earnedTx = new TimeTransaction
                    {
                        UserId = swap.ReceiverId,
                        Amount = 1,
                        TransactionType = TransactionType.Earned,
                        SwapRequestId = swap.Id,
                        SwapSessionId = session.Id
                    };

                    var spentTx = new TimeTransaction
                    {
                        UserId = swap.RequesterId,
                        Amount = 0,
                        TransactionType = TransactionType.Spent,
                        SwapRequestId = swap.Id,
                        SwapSessionId = session.Id
                    };

                    _context.TimeTransactions.Add(earnedTx);
                    _context.TimeTransactions.Add(spentTx);
                    await _context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var updated = await _swapRepo.GetSwapWithDetailsAsync(swapId);
        return MapToDto(updated!);
    }

    public async Task<SwapDto> InvalidateSessionAsync(string userId, int swapId, int sessionId)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        var session = swap.Sessions.FirstOrDefault(s => s.Id == sessionId)
            ?? throw new NotFoundException("SwapSession", sessionId);

        if (swap.RequesterId != userId && swap.ReceiverId != userId)
            throw new UnauthorizedException("You are not a participant in this swap.");

        if (session.Status != SwapSessionStatus.Scheduled)
            throw new BadRequestException("Session must be scheduled before it can be invalidated.");

        session.RequesterValidated = false;
        session.ReceiverValidated = false;
        session.Status = SwapSessionStatus.Invalidated;

        swap.Status = SwapStatus.Accepted;
        swap.UpdatedAt = DateTime.UtcNow;
        await _swapRepo.UpdateAsync(swap);

        return MapToDto(swap);
    }

    public async Task<SwapDto> ValidateSwapAsync(string userId, int swapId)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        if (swap.RequesterId != userId && swap.ReceiverId != userId)
            throw new UnauthorizedException("You are not a participant in this swap.");

        if (swap.Status != SwapStatus.Scheduled
            && swap.Status != SwapStatus.ValidatedByRequester
            && swap.Status != SwapStatus.ValidatedByReceiver)
            throw new BadRequestException("Swap must be scheduled before it can be validated.");

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (swap.RequesterId == userId)
                swap.RequesterValidated = true;
            else
                swap.ReceiverValidated = true;

            bool isNewlyCompleted = false;

            if (swap.RequesterValidated && swap.ReceiverValidated)
            {
                swap.Status = SwapStatus.Completed;
                isNewlyCompleted = true;
            }
            else if (swap.RequesterValidated)
                swap.Status = SwapStatus.ValidatedByRequester;
            else
                swap.Status = SwapStatus.ValidatedByReceiver;

            swap.UpdatedAt = DateTime.UtcNow;
            await _swapRepo.UpdateAsync(swap);

            if (isNewlyCompleted && swap.LessonType == LessonType.OneWay)
            {
                var receiver = await _userRepo.GetByIdAsync(swap.ReceiverId);
                if (receiver != null)
                {
                    receiver.TimeBalance += 1;
                    await _userRepo.UpdateAsync(receiver);

                    var earnedTx = new TimeTransaction
                    {
                        UserId = swap.ReceiverId,
                        Amount = 1,
                        TransactionType = TransactionType.Earned,
                        SwapRequestId = swap.Id
                    };
                    
                    var spentTx = new TimeTransaction
                    {
                        UserId = swap.RequesterId,
                        Amount = 0, // Keep 0 to signify completion, Escrow is already deducted
                        TransactionType = TransactionType.Spent,
                        SwapRequestId = swap.Id
                    };

                    _context.TimeTransactions.Add(earnedTx);
                    _context.TimeTransactions.Add(spentTx);
                    await _context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();
            return MapToDto(swap);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SwapDto> InvalidateSwapAsync(string userId, int swapId)
    {
        var swap = await _swapRepo.GetSwapWithDetailsAsync(swapId)
            ?? throw new NotFoundException("SwapRequest", swapId);

        if (swap.RequesterId != userId && swap.ReceiverId != userId)
            throw new UnauthorizedException("You are not a participant in this swap.");

        if (swap.Status != SwapStatus.Scheduled
            && swap.Status != SwapStatus.ValidatedByRequester
            && swap.Status != SwapStatus.ValidatedByReceiver)
            throw new BadRequestException("Swap must be scheduled before it can be invalidated.");

        // Reset validation and go back to Accepted so they can reschedule
        swap.RequesterValidated = false;
        swap.ReceiverValidated = false;
        swap.ScheduledDate = null;
        swap.TimeSlotStart = null;
        swap.TimeSlotEnd = null;
        swap.Status = SwapStatus.Accepted;
        swap.UpdatedAt = DateTime.UtcNow;
        await _swapRepo.UpdateAsync(swap);

        return MapToDto(swap);
    }

    private static void ValidateCadenceForSkill(Skill skill, LessonCadence cadence, string label)
    {
        if (skill.LessonMode == LessonMode.SingleOnly && cadence != LessonCadence.Single)
            throw new BadRequestException($"The {label} skill only allows single sessions.");

        if (skill.LessonMode == LessonMode.RecurringOnly && cadence != LessonCadence.Weekly)
            throw new BadRequestException($"The {label} skill only allows weekly recurring sessions.");

        if (cadence == LessonCadence.Single && skill.RequiredSessions != 1)
            throw new BadRequestException($"The {label} skill requires {skill.RequiredSessions} sessions and cannot be single.");

        if (cadence == LessonCadence.Weekly && skill.RequiredSessions < 2)
            throw new BadRequestException($"The {label} skill must be single because it requires 1 session.");
    }

    private static void ValidateProposalForTrack(
        SwapRequest swap,
        ScheduleTrackProposalDto proposal,
        Skill skill,
        LessonCadence expectedCadence,
        string label)
    {
        if (proposal.Cadence != expectedCadence)
            throw new BadRequestException($"The {label} schedule cadence does not match the swap settings.");

        if (proposal.SessionCount != skill.RequiredSessions)
            throw new BadRequestException($"The {label} schedule must include {skill.RequiredSessions} sessions.");

        if (proposal.Cadence == LessonCadence.Single)
        {
            if (proposal.SessionCount != 1 || proposal.SingleSessionStart == null)
                throw new BadRequestException($"The {label} schedule requires a single session start time.");
        }
        else
        {
            if (proposal.SessionCount < 2)
                throw new BadRequestException($"The {label} schedule requires at least two sessions.");

            if (proposal.StartDate == null || proposal.TimeOfDayMinutes == null)
                throw new BadRequestException($"The {label} weekly schedule requires a start date and time of day.");

            if (proposal.WeekDays == null || proposal.WeekDays.Count == 0)
                throw new BadRequestException($"The {label} weekly schedule requires at least one weekday.");
        }
    }

    private static SwapSchedule BuildScheduleEntity(SwapRequest swap, ScheduleTrackProposalDto proposal, string userId)
    {
        var weekDays = proposal.WeekDays != null
            ? string.Join(",", proposal.WeekDays.Distinct().OrderBy(d => d))
            : null;

        return new SwapSchedule
        {
            SwapRequestId = swap.Id,
            Track = proposal.Track,
            Cadence = proposal.Cadence,
            SessionCount = proposal.SessionCount,
            WeekDays = weekDays,
            TimeOfDayMinutes = proposal.TimeOfDayMinutes,
            SingleSessionStart = proposal.SingleSessionStart,
            StartDate = proposal.StartDate,
            Status = SwapScheduleStatus.Proposed,
            ProposedById = userId,
            ProposedAt = DateTime.UtcNow
        };
    }

    private static List<SwapSession> BuildSessionsFromProposal(
        SwapRequest swap,
        ScheduleTrackProposalDto proposal,
        SwapSchedule schedule)
    {
        if (proposal.Cadence == LessonCadence.Single)
        {
            var start = proposal.SingleSessionStart!.Value;
            return new List<SwapSession>
            {
                new()
                {
                    SwapRequestId = swap.Id,
                    SwapSchedule = schedule,
                    Track = proposal.Track,
                    SessionOrder = 1,
                    StartTime = start,
                    EndTime = start.AddMinutes(SessionDurationMinutes),
                    Status = SwapSessionStatus.Proposed
                }
            };
        }

        var sessionStarts = BuildWeeklySessionStarts(
            proposal.StartDate!.Value,
            proposal.TimeOfDayMinutes!.Value,
            proposal.WeekDays!.Distinct().ToList(),
            proposal.SessionCount);

        return sessionStarts.Select((start, index) => new SwapSession
        {
            SwapRequestId = swap.Id,
            SwapSchedule = schedule,
            Track = proposal.Track,
            SessionOrder = index + 1,
            StartTime = start,
            EndTime = start.AddMinutes(SessionDurationMinutes),
            Status = SwapSessionStatus.Proposed
        }).ToList();
    }

    private static List<DateTime> BuildWeeklySessionStarts(
        DateTime startDate,
        int timeOfDayMinutes,
        ICollection<DayOfWeek> weekDays,
        int sessionCount)
    {
        var normalizedDays = new HashSet<DayOfWeek>(weekDays);
        var results = new List<DateTime>();
        var cursor = startDate.Date;

        while (results.Count < sessionCount)
        {
            if (normalizedDays.Contains(cursor.DayOfWeek))
            {
                var start = cursor.AddMinutes(timeOfDayMinutes);
                results.Add(start);
            }

            cursor = cursor.AddDays(1);
        }

        return results;
    }

    private static SwapDto MapToDto(SwapRequest s) => new()
    {
        Id = s.Id,
        RequesterId = s.RequesterId,
        RequesterName = s.Requester?.FullName ?? "",
        ReceiverId = s.ReceiverId,
        ReceiverName = s.Receiver?.FullName ?? "",
        LessonType = s.LessonType,
        RequestedCadence = s.RequestedCadence,
        OfferedCadence = s.OfferedCadence,
        TwoWayScheduleMode = s.TwoWayScheduleMode,
        TeacherId = s.LessonType == LessonType.OneWay ? s.TeacherId ?? s.ReceiverId : null,
        TeacherName = s.LessonType == LessonType.OneWay ? s.Receiver?.FullName : null,
        LearnerId = s.LessonType == LessonType.OneWay ? s.LearnerId ?? s.RequesterId : null,
        LearnerName = s.LessonType == LessonType.OneWay ? s.Requester?.FullName : null,
        OfferedSkillTitle = s.OfferedSkill?.Title ?? "",
        RequestedSkillTitle = s.RequestedSkill?.Title ?? "",
        RequestedSessionCount = s.RequestedSkill?.RequiredSessions ?? 1,
        OfferedSessionCount = s.OfferedSkill?.RequiredSessions,
        Status = s.Status,
        ScheduledDate = s.ScheduledDate,
        TimeSlotStart = s.TimeSlotStart,
        TimeSlotEnd = s.TimeSlotEnd,
        RequesterValidated = s.RequesterValidated,
        ReceiverValidated = s.ReceiverValidated,
        Schedules = s.Schedules.Select(sc => new SwapScheduleDto
        {
            Id = sc.Id,
            Track = sc.Track,
            Cadence = sc.Cadence,
            SessionCount = sc.SessionCount,
            WeekDays = sc.WeekDays,
            TimeOfDayMinutes = sc.TimeOfDayMinutes,
            SingleSessionStart = sc.SingleSessionStart,
            StartDate = sc.StartDate,
            ChangeRequestNote = sc.ChangeRequestNote,
            ChangeRequestTime = sc.ChangeRequestTime,
            ChangeRequestedById = sc.ChangeRequestedById,
            ChangeRequestedAt = sc.ChangeRequestedAt,
            Status = sc.Status,
            ProposedById = sc.ProposedById,
            ProposedAt = sc.ProposedAt
        }).ToList(),
        Sessions = s.Sessions
            .OrderBy(sess => sess.StartTime)
            .Select(sess => new SwapSessionDto
            {
                Id = sess.Id,
                Track = sess.Track,
                SessionOrder = sess.SessionOrder,
                StartTime = sess.StartTime,
                EndTime = sess.EndTime,
                Status = sess.Status,
                RequesterValidated = sess.RequesterValidated,
                ReceiverValidated = sess.ReceiverValidated
            }).ToList(),
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt ?? s.CreatedAt
    };
}
