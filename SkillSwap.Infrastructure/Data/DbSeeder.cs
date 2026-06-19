using Bogus;
using Microsoft.AspNetCore.Identity;
using SkillSwap.Core.Entities;
using SkillSwap.Core.Enums;
using System.Security.Claims;

namespace SkillSwap.Infrastructure.Data;

public class DbSeeder
{
    private readonly AppDbContext _context;

    public DbSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();

        // Check if database is already seeded via Users
        if (_context.Users.Any()) return;

        // Fetch Categories which are seeded inherently by the EF Migration
        var categories = _context.Categories.ToList();
        if (!categories.Any()) 
        {
            return; // Migration hasn't applied properly if categories are empty
        }

        // 1. Generate Users with Pre-hashed Password
        // Using Password123! for all mock accounts
        var passwordHasher = new PasswordHasher<AppUser>();
        var dummyUser = new AppUser { UserName = "dummy" };
        var sharedHash = passwordHasher.HashPassword(dummyUser, "Password123!");

        var userFaker = new Faker<AppUser>()
            .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.UserName, f => f.Internet.UserName().Replace(".", "").Replace("_", "") + f.UniqueIndex.ToString())
            .RuleFor(u => u.NormalizedUserName, (f, u) => u.UserName.ToUpper())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.UserName))
            .RuleFor(u => u.NormalizedEmail, (f, u) => u.Email.ToUpper())
            .RuleFor(u => u.EmailConfirmed, f => true)
            .RuleFor(u => u.PasswordHash, sharedHash)
            .RuleFor(u => u.SecurityStamp, f => Guid.NewGuid().ToString())
            .RuleFor(u => u.FullName, f => f.Name.FullName())
            .RuleFor(u => u.Bio, f => f.Lorem.Paragraph(1))
            .RuleFor(u => u.TimeBalance, f => f.Random.Int(5, 50))
            .RuleFor(u => u.Rating, f => Math.Round(f.Random.Double(3.5, 5.0), 1))
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(1).ToUniversalTime());

        var users = userFaker.Generate(1000);
        
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync(); // Commit users first to get relationships correctly mapped

        // 2. Build Realistic Skills Mapping mapped to Migration Category Names
        var skillsByCategory = new Dictionary<string, string[]>
        {
            { "Technology", new[] { "React Development", "C# Mentoring", "Python Scripting", "Database Design", "AWS Deployment", "UI/UX Prototyping", "JavaScript Basics", "Docker Containers" } },
            { "Sciences", new[] { "Chemistry Tutoring", "College Physics", "Biology Basics", "Lab Experiment Guidance" } },
            { "Mathematics", new[] { "High School Math", "Statistics", "Algebra Basics", "Calculus Tutoring" } },
            { "Languages", new[] { "Spanish Conversation", "English Tutoring", "Japanese Basics", "French Advanced", "German Translation", "Mandarin Intro" } },
            { "Music", new[] { "Guitar Lessons", "Piano Basics", "Vocal Coaching", "Ableton Mixing", "Music Theory", "Drum Basics" } },
            { "Fitness", new[] { "Personal Training", "Yoga Flow", "Nutrition Planning", "Meditation Basics", "Home Workouts", "Weight Lifting" } },
            { "Art & Design", new[] { "Logo Design", "Digital Illustration", "Photoshop Basics", "Watercolors", "Video Editing", "3D Animation" } },
            { "History", new[] { "World History Tutoring", "European Civilizations", "Historical Research Methods" } },
            { "Literature", new[] { "Creative Writing", "Essay Review", "Poetry Basics", "Literary Analysis" } },
            { "Business", new[] { "SEO Strategy", "Social Media Marketing", "Accounting Basics", "Resume Writing", "Interview Prep", "Sales Pitching" } },
            { "Cooking", new[] { "Cooking Basics", "Baking", "Meal Prep Strategy", "Nutrition Recipes" } },
            { "Crafts & DIY", new[] { "Plumbing DIY", "Plant Care", "Interior Decorating", "Carpentry", "Woodworking Basics", "Sewing" } }
        };

        var skillFaker = new Faker<Skill>()
            .RuleFor(s => s.Description, f => f.Lorem.Sentence(10))
            .RuleFor(s => s.ProficiencyLevel, f => f.PickRandom<ProficiencyLevel>())
            .RuleFor(s => s.CreatedAt, f => f.Date.Recent(100).ToUniversalTime());

        var allSkills = new List<Skill>();
        var rand = new Random();

        // Generate Offered and Requested skills for each user
        foreach (var user in users)
        {
            // Users will offer anywhere from 1 to 3 items
            var offeringCount = rand.Next(1, 4); 
            // Users will request anywhere from 1 to 3 items
            var requestedCount = rand.Next(1, 4);

            for (int i = 0; i < offeringCount; i++)
            {
                var randCategory = categories[rand.Next(categories.Count)];
                if(skillsByCategory.ContainsKey(randCategory.Name)) 
                {
                    var titles = skillsByCategory[randCategory.Name];
                    var randTitle = titles[rand.Next(titles.Length)];

                    var skill = skillFaker.Generate();
                    skill.UserId = user.Id;
                    skill.CategoryId = randCategory.Id;
                    skill.Title = randTitle;
                    skill.IsOffering = true;
                    
                    allSkills.Add(skill);
                }
            }

            for (int i = 0; i < requestedCount; i++)
            {
                var randCategory = categories[rand.Next(categories.Count)];
                if(skillsByCategory.ContainsKey(randCategory.Name)) 
                {
                    var titles = skillsByCategory[randCategory.Name];
                    var randTitle = titles[rand.Next(titles.Length)];

                    var skill = skillFaker.Generate();
                    skill.UserId = user.Id;
                    skill.CategoryId = randCategory.Id;
                    skill.Title = randTitle;
                    skill.IsOffering = false;
                    
                    allSkills.Add(skill);
                }
            }
        }

        await _context.Skills.AddRangeAsync(allSkills);
        await _context.SaveChangesAsync();
    }
}