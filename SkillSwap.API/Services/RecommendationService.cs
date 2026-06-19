using System.Net.Http.Json;
using SkillSwap.Core.DTOs.Recommendations;
using SkillSwap.Core.DTOs.Users;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Enums;
using SkillSwap.Core.Exceptions;
using SkillSwap.Core.Interfaces.Repositories;
using SkillSwap.Core.Interfaces.Services;
using System.Text.Json.Serialization;

namespace SkillSwap.API.Services;

public class RecommendationService : IRecommendationService
{
    private const int DefaultTopK = 3;
    private readonly IUserRepository _userRepo;
    private readonly ISwapRepository _swapRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly IHttpClientFactory _httpClientFactory;

    public RecommendationService(
        IUserRepository userRepo,
        ISwapRepository swapRepo,
        IReviewRepository reviewRepo,
        IHttpClientFactory httpClientFactory)
    {
        _userRepo = userRepo;
        _swapRepo = swapRepo;
        _reviewRepo = reviewRepo;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<RecommendationsResultDto> GenerateRecommendationsAsync(string userId)
    {
        var user = await _userRepo.GetByIdWithSkillsAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var swaps = await _swapRepo.GetSwapsByUserIdAsync(userId);
        var reviewCount = await _reviewRepo.GetReviewCountAsync(userId);
        var profileText = BuildProfileText(user, swaps, reviewCount);

        var client = _httpClientFactory.CreateClient("RecommendationsService");
        if (client.BaseAddress == null)
            throw new BadRequestException("Recommendations service is not configured.");

        var serviceRequest = new RecommendationsServiceRequest
        {
            UserId = userId,
            ProfileText = profileText,
            TopK = DefaultTopK
        };
        var response = await client.PostAsJsonAsync("/recommendations", serviceRequest);
        if (!response.IsSuccessStatusCode)
        {
            // Read the actual error message sent by Python
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new BadRequestException($"Recommendations service failed. Python says: {errorContent}");
        }

        var serviceResponse = await response.Content.ReadFromJsonAsync<RecommendationsServiceResponse>();
        if (serviceResponse == null)
            throw new BadRequestException("Recommendations service returned no data.");

        if (serviceResponse.Matches.Count == 0)
        {
            return new RecommendationsResultDto
            {
                GeneratedAt = DateTime.UtcNow,
                Matches = new List<RecommendationMatchDto>()
            };
        }

        var matchIds = serviceResponse.Matches.Select(m => m.UserId).ToList();
        var matchedUsers = await _userRepo.GetByIdsWithSkillsAsync(matchIds);
        var matchedById = matchedUsers.ToDictionary(u => u.Id, u => u);

        var results = new List<RecommendationMatchDto>();
        foreach (var match in serviceResponse.Matches)
        {
            if (!matchedById.TryGetValue(match.UserId, out var matchUser))
                continue;

            var matchReviewCount = await _reviewRepo.GetReviewCountAsync(matchUser.Id);
            results.Add(new RecommendationMatchDto
            {
                UserId = matchUser.Id,
                FullName = matchUser.FullName,
                Bio = matchUser.Bio,
                Rating = matchUser.Rating,
                ReviewCount = matchReviewCount,
                Similarity = match.Similarity,
                Reason = match.Reason,
                Skills = matchUser.Skills.Select(s => new SkillSummaryDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    IsOffering = s.IsOffering,
                    CategoryName = s.Category?.Name ?? string.Empty
                }).ToList()
            });
        }

        return new RecommendationsResultDto
        {
            GeneratedAt = DateTime.UtcNow,
            Matches = results
        };
    }

    private static string BuildProfileText(AppUser user, IEnumerable<SwapRequest> swaps, int reviewCount)
    {
        var offeredSkills = user.Skills.Where(s => s.IsOffering).Select(s => s.Title).Distinct().ToList();
        var wantedSkills = user.Skills.Where(s => !s.IsOffering).Select(s => s.Title).Distinct().ToList();

        var completedSwaps = swaps
            .Where(s => s.Status == SwapStatus.Completed)
            .OrderByDescending(s => s.CreatedAt)
            .Take(10)
            .SelectMany(s => new[]
            {
                s.RequestedSkill?.Title,
                s.OfferedSkill?.Title
            })
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .ToList();

        var offeredText = offeredSkills.Count > 0 ? string.Join(", ", offeredSkills) : "None";
        var wantedText = wantedSkills.Count > 0 ? string.Join(", ", wantedSkills) : "None";
        var swapsText = completedSwaps.Count > 0 ? string.Join(", ", completedSwaps) : "None";
        var bioText = string.IsNullOrWhiteSpace(user.Bio) ? "" : user.Bio;

        return string.Join("\n", new[]
        {
            $"Name: {user.FullName}",
            $"Bio: {bioText}",
            $"Offers: {offeredText}",
            $"Wants: {wantedText}",
            $"Recent swaps: {swapsText}",
            $"Rating: {user.Rating} from {reviewCount} reviews"
        });
    }

    public async Task SeedDatabaseAsync()
    {
        Console.WriteLine("Starting background database seeding with batching...");

        var allUsers = await _userRepo.GetAllAsync(); 
        var userList = allUsers.ToList(); // Convert to list so we can index it
        int total = userList.Count;
        
        // Configuration for rate limiting
        int batchSize = 10; // Process 10 users at a time
        int cooldownSeconds = 65; // Wait 65 seconds after a batch (guarantees a minute passes)

        for (int i = 0; i < total; i++)
        {
            var user = userList[i];
            
            try
            {
                await GenerateRecommendationsAsync(user.Id);
                Console.WriteLine($"[{i + 1}/{total}] Successfully seeded user: {user.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{i + 1}/{total}] FAILED to seed user {user.Id}. Error: {ex.Message}");
            }

            // Check if we just finished a batch (and ensure we aren't at the very end of the list)
            if ((i + 1) % batchSize == 0 && (i + 1) < total)
            {
                Console.WriteLine($"\n⏳ Batch of {batchSize} complete. Cooling down for {cooldownSeconds} seconds to respect API limits...\n");
                await Task.Delay(cooldownSeconds * 1000);
            }
            else if ((i + 1) < total)
            {
                 // A tiny delay between requests in the same batch just to be safe
                 await Task.Delay(1000);
            }
        }

        Console.WriteLine("✅ Database seeding complete!");
    }
    private class RecommendationsServiceRequest
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("profile_text")]
        public string ProfileText { get; set; } = string.Empty;

        [JsonPropertyName("top_k")]
        public int TopK { get; set; }
    }

    private class RecommendationsServiceResponse
    {
        [JsonPropertyName("matches")]
        public List<RecommendationsServiceMatch> Matches { get; set; } = new();
    }

    private class RecommendationsServiceMatch
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("similarity")]
        public double Similarity { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
