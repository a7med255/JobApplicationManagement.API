using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using JobApplicationManagement.Infrastructure.Identity;

namespace JobApplicationManagement.Infrastructure.Persistence;

/// <summary>
/// Development-only, idempotent data seeder for Careerly.
/// Populates realistic Egyptian technology market demo records for full-stack UI/UX testing.
/// </summary>
public static class DevelopmentDataSeeder
{
    public const string DefaultDevPassword = "CareerlyDev123!";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("Beginning idempotent Careerly development data seeding...");

        var now = DateTime.UtcNow;

        // ── 1. Roles ─────────────────────────────────────────────────────────────
        string[] roles = ["Admin", "Recruiter", "Candidate"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ── 2. Demo Identity Users ───────────────────────────────────────────────
        var demoUsers = new[]
        {
            new { Email = "admin@careerly.local", FullName = "System Administrator", Role = "Admin" },
            new { Email = "recruiter@careerly.local", FullName = "Karim Mansour", Role = "Recruiter" },
            new { Email = "recruiter2@careerly.local", FullName = "Nour El-Din", Role = "Recruiter" },
            new { Email = "candidate@careerly.local", FullName = "Ahmed Hassan", Role = "Candidate" },
            new { Email = "candidate2@careerly.local", FullName = "Mariam Mostafa", Role = "Candidate" },
            new { Email = "candidate3@careerly.local", FullName = "Omar Khaled", Role = "Candidate" }
        };

        var userMap = new Dictionary<string, ApplicationUser>();

        foreach (var u in demoUsers)
        {
            var user = await userManager.FindByEmailAsync(u.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = u.Email,
                    Email = u.Email,
                    FullName = u.FullName,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var createResult = await userManager.CreateAsync(user, DefaultDevPassword);
                if (!createResult.Succeeded)
                {
                    logger.LogError("Failed to create seed user {Email}: {Errors}",
                        u.Email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    continue;
                }
            }

            if (!await userManager.IsInRoleAsync(user, u.Role))
            {
                await userManager.AddToRoleAsync(user, u.Role);
            }

            userMap[u.Email] = user;
        }

        // ── 3. Companies ─────────────────────────────────────────────────────────
        var demoCompanies = new[]
        {
            new Company
            {
                Name = "PayNile FinTech",
                Description = "Next-generation digital payment gateway and micro-lending infrastructure powering financial inclusion across Egypt.",
                Industry = "FinTech",
                Headquarters = "Smart Village, Building B12",
                Country = "Egypt",
                City = "Giza",
                Website = "https://paynile.example.com",
                CompanySize = "100-250",
                CreatedAt = now.AddDays(-120),
                UpdatedAt = now.AddDays(-10)
            },
            new Company
            {
                Name = "NileCloud Systems",
                Description = "Enterprise cloud migration, DevOps acceleration, and managed Kubernetes infrastructure provider in the MENA region.",
                Industry = "Cloud Services",
                Headquarters = "Sector 1, North 90th St",
                Country = "Egypt",
                City = "New Cairo",
                Website = "https://nilecloud.example.com",
                CompanySize = "50-100",
                CreatedAt = now.AddDays(-90),
                UpdatedAt = now.AddDays(-15)
            },
            new Company
            {
                Name = "MedFlow HealthTech",
                Description = "Cloud electronic health records (EHR), telemedicine diagnostics, and clinic workflow management software.",
                Industry = "Healthcare Technology",
                Headquarters = "Degla Square, Maadi",
                Country = "Egypt",
                City = "Cairo",
                Website = "https://medflow.example.com",
                CompanySize = "20-50",
                CreatedAt = now.AddDays(-80),
                UpdatedAt = now.AddDays(-5)
            },
            new Company
            {
                Name = "DeltaCommerce Digital",
                Description = "High-scale multi-vendor e-commerce platform and omni-channel retail fulfillment solutions.",
                Industry = "E-commerce",
                Headquarters = "Makram Ebeid, Nasr City",
                Country = "Egypt",
                City = "Cairo",
                Website = "https://deltacommerce.example.com",
                CompanySize = "250-500",
                CreatedAt = now.AddDays(-110),
                UpdatedAt = now.AddDays(-8)
            },
            new Company
            {
                Name = "CyberShield MENA",
                Description = "Threat intelligence, managed security operations center (SOC), and cloud security compliance engineering.",
                Industry = "Cybersecurity",
                Headquarters = "Juhayna Square, Tech Park",
                Country = "Egypt",
                City = "6th of October",
                Website = "https://cybershield.example.com",
                CompanySize = "50-100",
                CreatedAt = now.AddDays(-75),
                UpdatedAt = now.AddDays(-12)
            },
            new Company
            {
                Name = "EduSpark Learning",
                Description = "Adaptive digital learning management system (LMS), interactive STEM curriculums, and virtual tutoring platform.",
                Industry = "Education Technology",
                Headquarters = "Mossaddak Street, Dokki",
                Country = "Egypt",
                City = "Giza",
                Website = "https://eduspark.example.com",
                CompanySize = "20-50",
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now.AddDays(-4)
            },
            new Company
            {
                Name = "Apex Telecom AI",
                Description = "AI-assisted network operations, 5G analytics, and high-throughput telecommunication billing pipelines.",
                Industry = "Telecommunications",
                Headquarters = "Smart Village, Km 28 Cairo-Alex Desert Rd",
                Country = "Egypt",
                City = "Giza",
                Website = "https://apextelecom.example.com",
                CompanySize = "500+",
                CreatedAt = now.AddDays(-150),
                UpdatedAt = now.AddDays(-20)
            },
            new Company
            {
                Name = "LogiRoute Solutions",
                Description = "Automated fleet routing, real-time freight tracking, and supply chain dispatch optimization software.",
                Industry = "Logistics Tech",
                Headquarters = "Fouad Street, Downtown",
                Country = "Egypt",
                City = "Alexandria",
                Website = "https://logiroute.example.com",
                CompanySize = "50-100",
                CreatedAt = now.AddDays(-100),
                UpdatedAt = now.AddDays(-18)
            },
            new Company
            {
                Name = "AgroVance Smart Systems",
                Description = "IoT soil monitoring, precision agriculture sensors, and yield prediction machine learning models.",
                Industry = "AgriTech",
                Headquarters = "Fifth Settlement, South Academy",
                Country = "Egypt",
                City = "New Cairo",
                Website = "https://agrovance.example.com",
                CompanySize = "10-25",
                CreatedAt = now.AddDays(-50),
                UpdatedAt = now.AddDays(-2)
            },
            new Company
            {
                Name = "FinBridge Global",
                Description = "Cross-border payment clearing, multicurrency treasury rails, and regulatory compliance infrastructure.",
                Industry = "FinTech",
                Headquarters = "Gezira St, Zamalek",
                Country = "Egypt",
                City = "Cairo",
                Website = "https://finbridge.example.com",
                CompanySize = "100-250",
                CreatedAt = now.AddDays(-70),
                UpdatedAt = now.AddDays(-6)
            }
        };

        var companyMap = new Dictionary<string, Company>();

        foreach (var c in demoCompanies)
        {
            var existing = await context.Companies.FirstOrDefaultAsync(x => x.Name == c.Name);
            if (existing == null)
            {
                context.Companies.Add(c);
                companyMap[c.Name] = c;
            }
            else
            {
                companyMap[c.Name] = existing;
            }
        }
        await context.SaveChangesAsync();

        // ── 4. Recruiters ────────────────────────────────────────────────────────
        var recruiter1User = userMap.GetValueOrDefault("recruiter@careerly.local");
        var recruiter2User = userMap.GetValueOrDefault("recruiter2@careerly.local");

        var recruiterList = new List<Recruiter>();

        if (recruiter1User != null)
        {
            var r1 = await context.Recruiters.FirstOrDefaultAsync(r => r.UserId == recruiter1User.Id);
            if (r1 == null)
            {
                r1 = new Recruiter
                {
                    UserId = recruiter1User.Id,
                    Name = recruiter1User.FullName,
                    CompanyId = companyMap["PayNile FinTech"].Id
                };
                context.Recruiters.Add(r1);
            }
            recruiterList.Add(r1);
        }

        if (recruiter2User != null)
        {
            var r2 = await context.Recruiters.FirstOrDefaultAsync(r => r.UserId == recruiter2User.Id);
            if (r2 == null)
            {
                r2 = new Recruiter
                {
                    UserId = recruiter2User.Id,
                    Name = recruiter2User.FullName,
                    CompanyId = companyMap["NileCloud Systems"].Id
                };
                context.Recruiters.Add(r2);
            }
            recruiterList.Add(r2);
        }
        await context.SaveChangesAsync();

        var primaryRecruiter = recruiterList.FirstOrDefault();
        if (primaryRecruiter == null)
        {
            logger.LogWarning("No recruiter could be resolved for job seeding. Aborting remaining seeding.");
            return;
        }

        var secondaryRecruiter = recruiterList.Skip(1).FirstOrDefault() ?? primaryRecruiter;

        // ── 5. Candidates ────────────────────────────────────────────────────────
        var candidateUsers = new[]
        {
            new
            {
                Email = "candidate@careerly.local",
                Name = "Ahmed Hassan",
                Title = "Senior Full-Stack .NET & Angular Developer",
                Exp = 6,
                City = "New Cairo",
                Country = "Egypt",
                Summary = "Full-stack software engineer with 6+ years designing scalable ASP.NET Core microservices, clean RESTful APIs, and modern Angular standalone frontends."
            },
            new
            {
                Email = "candidate2@careerly.local",
                Name = "Mariam Mostafa",
                Title = "Senior QA Automation & Test Lead",
                Exp = 5,
                City = "Maadi",
                Country = "Egypt",
                Summary = "Software test automation specialist proficient in Playwright, Selenium, Postman API testing, and continuous test automation in CI/CD pipelines."
            },
            new
            {
                Email = "candidate3@careerly.local",
                Name = "Omar Khaled",
                Title = "Junior Backend Developer",
                Exp = 1,
                City = "Nasr City",
                Country = "Egypt",
                Summary = "Passionate junior .NET developer with strong grasp of C#, OOP principles, SQL Server relational modeling, and Entity Framework Core."
            }
        };

        var candidateMap = new Dictionary<string, Candidate>();

        foreach (var cu in candidateUsers)
        {
            if (userMap.TryGetValue(cu.Email, out var appUser))
            {
                var cand = await context.Candidates.FirstOrDefaultAsync(c => c.UserId == appUser.Id);
                if (cand == null)
                {
                    cand = new Candidate
                    {
                        UserId = appUser.Id,
                        Name = cu.Name,
                        CvUrl = "",
                        ProfessionalTitle = cu.Title,
                        YearsOfExperience = cu.Exp,
                        City = cu.City,
                        Country = cu.Country,
                        Summary = cu.Summary
                    };
                    context.Candidates.Add(cand);
                }
                else
                {
                    cand.ProfessionalTitle ??= cu.Title;
                    cand.YearsOfExperience ??= cu.Exp;
                    cand.City ??= cu.City;
                    cand.Country ??= cu.Country;
                    cand.Summary ??= cu.Summary;
                }
                candidateMap[cu.Email] = cand;
            }
        }
        await context.SaveChangesAsync();

        // ── 6. Skills ────────────────────────────────────────────────────────────
        string[] seedSkills =
        [
            "C#", "ASP.NET Core", "Entity Framework Core", "SQL Server", "REST API", "Git", "Docker",
            "Angular", "TypeScript", "JavaScript", "HTML", "CSS", "RxJS",
            "Manual Testing", "API Testing", "Postman", "Selenium", "Playwright", "Test Case Design",
            "Regression Testing", "JIRA", "JMeter",
            "Java", "Spring Boot", "Python", "Linux", "Kubernetes", "Azure", "CI/CD", "Redis"
        ];

        var skillMap = new Dictionary<string, Skill>(StringComparer.OrdinalIgnoreCase);

        foreach (var skillName in seedSkills)
        {
            var normalized = skillName.Trim().ToUpperInvariant();
            var existing = await context.Skills.FirstOrDefaultAsync(s => s.NormalizedName == normalized);
            if (existing == null)
            {
                var skill = new Skill
                {
                    Name = skillName.Trim(),
                    NormalizedName = normalized
                };
                context.Skills.Add(skill);
                skillMap[skillName] = skill;
            }
            else
            {
                skillMap[skillName] = existing;
            }
        }
        await context.SaveChangesAsync();

        // ── 7. Jobs Definition ───────────────────────────────────────────────────
        var jobsData = new[]
        {
            // --- 6 QA / Testing Roles ---
            new
            {
                Title = "Junior QA Engineer",
                Company = "PayNile FinTech",
                Department = "Quality Assurance",
                Category = "Quality Assurance",
                Country = "Egypt",
                City = "Giza",
                Location = "Smart Village, Building B12",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Junior,
                MinExp = 1,
                MaxExp = (int?)3,
                MinSalary = (decimal?)12000,
                MaxSalary = (decimal?)18000,
                Vacancies = 2,
                DeadlineDays = 35,
                PostedDaysAgo = 3,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "Manual Testing", "API Testing", "Postman", "SQL Server", "Git" },
                PreferredSkills = new[] { "JIRA" },
                Description = "Join our financial services QA team verifying payment gateway transactions, fraud controls, and reconciliation microservices.",
                Responsibilities = "Execute manual test scenarios for payment flows; write detailed bug reports in JIRA; perform REST API validation using Postman collections; inspect SQL Server database transaction consistency.",
                Requirements = "1+ years software testing experience; solid understanding of SDLC and STLC; hands-on API testing with Postman; basic SQL query skills (SELECT, JOIN, GROUP BY); proactive attention to detail.",
                Benefits = "Social and private medical insurance; transportation to Smart Village; annual performance bonus; certification sponsorship."
            },
            new
            {
                Title = "Automation QA Engineer",
                Company = "DeltaCommerce Digital",
                Department = "Engineering",
                Category = "Quality Assurance",
                Country = "Egypt",
                City = "Cairo",
                Location = "Nasr City",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 3,
                MaxExp = (int?)5,
                MinSalary = (decimal?)26000,
                MaxSalary = (decimal?)38000,
                Vacancies = 1,
                DeadlineDays = 40,
                PostedDaysAgo = 5,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "Playwright", "TypeScript", "API Testing", "Git" },
                PreferredSkills = new[] { "Docker", "CI/CD" },
                Description = "Lead the automated testing strategy for our high-scale consumer checkout and merchant portal interfaces.",
                Responsibilities = "Design and maintain end-to-end regression suites using Playwright with TypeScript; integrate automated tests into CI/CD pipelines; collaborate with frontend developers on testable UI components; conduct load and contract testing.",
                Requirements = "3+ years automated testing experience; strong TypeScript or JavaScript coding skills; hands-on experience building Playwright or Cypress frameworks; solid CI/CD integration knowledge.",
                Benefits = "Flexible hybrid schedule (2 days office / 3 days remote); tech allowance; medical coverage for family; gym membership."
            },
            new
            {
                Title = "Software Test Engineer",
                Company = "MedFlow HealthTech",
                Department = "Product Quality",
                Category = "Quality Assurance",
                Country = "Egypt",
                City = "Cairo",
                Location = "Degla, Maadi",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 2,
                MaxExp = (int?)4,
                MinSalary = (decimal?)20000,
                MaxSalary = (decimal?)28000,
                Vacancies = 2,
                DeadlineDays = 28,
                PostedDaysAgo = 7,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "Test Case Design", "Regression Testing", "Postman", "JIRA", "SQL Server" },
                PreferredSkills = new[] { "Manual Testing" },
                Description = "Ensure medical record accuracy, HIPAA compliance, and reliable clinical scheduling flows across our health platform.",
                Responsibilities = "Author comprehensive test matrices from functional specs; perform exploratory and negative testing on patient data entry; verify HL7/FHIR healthcare API contracts; track defect lifecycles in JIRA.",
                Requirements = "2+ years experience testing enterprise web applications; strong test design techniques (boundary value analysis, equivalence partitioning); solid Postman API testing skills; healthcare or ERP experience is a plus.",
                Benefits = "Comprehensive health insurance; continuous learning stipend; friendly team culture; modern Maadi office."
            },
            new
            {
                Title = "Senior QA Automation Lead",
                Company = "NileCloud Systems",
                Department = "Cloud Quality",
                Category = "Quality Assurance",
                Country = "Egypt",
                City = "New Cairo",
                Location = "North 90th Street",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Senior,
                MinExp = 5,
                MaxExp = (int?)8,
                MinSalary = (decimal?)45000,
                MaxSalary = (decimal?)65000,
                Vacancies = 1,
                DeadlineDays = 45,
                PostedDaysAgo = 10,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "Selenium", "Java", "CI/CD", "Docker", "Git" },
                PreferredSkills = new[] { "Kubernetes", "Linux" },
                Description = "Define enterprise test automation architecture for cloud provisioning tools, Kubernetes operators, and customer management consoles.",
                Responsibilities = "Architect parallelized test execution frameworks; mentor junior and mid-level QA engineers; oversee smoke, sanity, regression, and disaster recovery test runs in staging environments; establish quality metrics and SLAs.",
                Requirements = "5+ years in QA automation; mastery of Java or C# test frameworks (Selenium, TestNG, JUnit); solid containerization (Docker) and CI/CD pipeline skills; proven track record leading quality initiatives.",
                Benefits = "Top-tier compensation package; flexible remote days; stock option incentive; annual overseas conference pass."
            },
            new
            {
                Title = "Performance & Load Test Engineer",
                Company = "CyberShield MENA",
                Department = "Security Operations",
                Category = "Quality Assurance",
                Country = "Egypt",
                City = "6th of October",
                Location = "Tech Park",
                Workplace = WorkplaceType.Remote,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Senior,
                MinExp = 4,
                MaxExp = (int?)7,
                MinSalary = (decimal?)35000,
                MaxSalary = (decimal?)50000,
                Vacancies = 1,
                DeadlineDays = 30,
                PostedDaysAgo = 12,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "JMeter", "Postman", "Linux", "SQL Server" },
                PreferredSkills = new[] { "Docker" },
                Description = "Stress test high-throughput packet inspection pipelines, log ingestors, and client alerting webhooks under simulated DDoS loads.",
                Responsibilities = "Design and execute load, spike, stress, and endurance test plans using Apache JMeter; analyze server latency, CPU/memory profiling, and database connection pooling bottlenecks; deliver bottleneck remediation reports.",
                Requirements = "4+ years focused on performance testing; deep JMeter or k6 expertise; strong understanding of networking, HTTP/2, and Linux performance profiling; experience diagnosing SQL query concurrency locks.",
                Benefits = "100% remote work; home office setup allowance; USD pegged compensation adjustments; premium private healthcare."
            },
            new
            {
                Title = "Mobile QA Specialist",
                Company = "EduSpark Learning",
                Department = "Mobile Engineering",
                Category = "Quality Assurance",
                Country = "Egypt",
                City = "Giza",
                Location = "Dokki",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Junior,
                MinExp = 1,
                MaxExp = (int?)3,
                MinSalary = (decimal?)14000,
                MaxSalary = (decimal?)20000,
                Vacancies = 1,
                DeadlineDays = 25,
                PostedDaysAgo = 4,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "Manual Testing", "Postman", "JIRA" },
                PreferredSkills = new[] { "Git" },
                Description = "Validate iOS and Android mobile student learning applications, audio stream sync, and offline lesson downloads.",
                Responsibilities = "Execute cross-device mobile test matrices on physical Android and iOS handsets; report UI visual discrepancies, orientation glitches, and crash dumps; test offline caching and sync resolution upon reconnecting.",
                Requirements = "1+ years testing native or hybrid mobile apps; familiar with ADB commands, Xcode simulator, and test flight distributions; strong exploratory testing mindset.",
                Benefits = "Device test lab access; subsidized meals; health insurance; career path towards test automation."
            },

            // --- Backend .NET & Java Roles ---
            new
            {
                Title = ".NET Backend Developer",
                Company = "PayNile FinTech",
                Department = "Engineering",
                Category = "Software Development",
                Country = "Egypt",
                City = "Giza",
                Location = "Smart Village",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 3,
                MaxExp = (int?)6,
                MinSalary = (decimal?)30000,
                MaxSalary = (decimal?)45000,
                Vacancies = 3,
                DeadlineDays = 40,
                PostedDaysAgo = 2,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "C#", "ASP.NET Core", "Entity Framework Core", "SQL Server", "REST API" },
                PreferredSkills = new[] { "Redis", "Docker" },
                Description = "Build mission-critical payment settlement and dispute handling web APIs utilizing clean architecture and MediatR.",
                Responsibilities = "Develop high-throughput REST APIs using ASP.NET Core 8/9; implement database migrations and optimized queries with EF Core; write unit and integration tests with xUnit and Moq; collaborate with frontend engineers on OpenAPI contracts.",
                Requirements = "3+ years professional C# / .NET development; solid mastery of EF Core, SQL Server indexing, and async programming; experience with dependency injection, middleware pipelines, and JWT security.",
                Benefits = "Smart Village shuttle; medical insurance; annual bonus; technical training budget."
            },
            new
            {
                Title = "Senior C# .NET Solutions Architect",
                Company = "NileCloud Systems",
                Department = "Architecture",
                Category = "Software Development",
                Country = "Egypt",
                City = "New Cairo",
                Location = "North 90th Street",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Senior,
                MinExp = 6,
                MaxExp = (int?)10,
                MinSalary = (decimal?)55000,
                MaxSalary = (decimal?)75000,
                Vacancies = 1,
                DeadlineDays = 50,
                PostedDaysAgo = 8,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "C#", "ASP.NET Core", "Docker", "Kubernetes", "Azure" },
                PreferredSkills = new[] { "Redis", "CI/CD" },
                Description = "Lead technical architecture and distributed system design for multi-tenant SaaS cloud services.",
                Responsibilities = "Design event-driven architectures and distributed caching patterns; mentor backend developers on domain-driven design (DDD); conduct architecture reviews for scalability, resilience, and security.",
                Requirements = "6+ years enterprise .NET experience; deep understanding of distributed systems, message queues, and cloud deployment in Azure/AWS; strong communication and mentoring skills.",
                Benefits = "Executive health plan; quarterly profit share; flexible hybrid schedule; training stipend."
            },
            new
            {
                Title = "Java Spring Boot Developer",
                Company = "FinBridge Global",
                Department = "Core Banking",
                Category = "Software Development",
                Country = "Egypt",
                City = "Cairo",
                Location = "Zamalek",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 3,
                MaxExp = (int?)5,
                MinSalary = (decimal?)28000,
                MaxSalary = (decimal?)42000,
                Vacancies = 2,
                DeadlineDays = 30,
                PostedDaysAgo = 6,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "Java", "Spring Boot", "REST API", "SQL Server", "Git" },
                PreferredSkills = new[] { "Docker" },
                Description = "Develop international currency remittance settlement services and ledger validation modules.",
                Responsibilities = "Build microservices with Java 17/21 and Spring Boot; integrate with third-party banking APIs; write JUnit and Mockito test suites; optimize relational database queries.",
                Requirements = "3+ years Java development; solid Spring Boot, Spring Data JPA, and Spring Security experience; clean code habits and familiarity with Docker.",
                Benefits = "Zamalek central office location; comprehensive medical; meal vouchers; flexible start hours."
            },
            new
            {
                Title = "Junior Backend Developer",
                Company = "DeltaCommerce Digital",
                Department = "Engineering",
                Category = "Software Development",
                Country = "Egypt",
                City = "Cairo",
                Location = "Nasr City",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Junior,
                MinExp = 0,
                MaxExp = (int?)2,
                MinSalary = (decimal?)12000,
                MaxSalary = (decimal?)16000,
                Vacancies = 2,
                DeadlineDays = 30,
                PostedDaysAgo = 1,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "C#", "ASP.NET Core", "SQL Server", "Git" },
                PreferredSkills = new[] { "Entity Framework Core" },
                Description = "Ideal stepping stone for motivated junior engineers eager to grow in a fast-paced e-commerce environment.",
                Responsibilities = "Develop and maintain RESTful API endpoints; assist in database migrations; participate in code reviews and pair programming sessions; write unit tests.",
                Requirements = "Bachelor's degree in Computer Science or equivalent; strong fundamentals in OOP, C#, and relational databases; personal or graduation projects using ASP.NET Core.",
                Benefits = "Structured mentorship program; health insurance; casual work environment; rapid promotion track."
            },
            new
            {
                Title = "Python Data Backend Engineer",
                Company = "AgroVance Smart Systems",
                Department = "IoT Analytics",
                Category = "Software Development",
                Country = "Egypt",
                City = "New Cairo",
                Location = "Fifth Settlement",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 2,
                MaxExp = (int?)5,
                MinSalary = (decimal?)25000,
                MaxSalary = (decimal?)38000,
                Vacancies = 1,
                DeadlineDays = 35,
                PostedDaysAgo = 9,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "Python", "REST API", "SQL Server", "Docker", "Git" },
                PreferredSkills = new[] { "Linux" },
                Description = "Build data ingestion APIs and timeseries processing backends for IoT agricultural field telemetry.",
                Responsibilities = "Develop high-performance REST APIs with FastAPI; process incoming sensor telemetries; optimize time-series queries; deploy services in containerized Linux environments.",
                Requirements = "2+ years Python backend engineering; proficiency with FastAPI or Flask, asynchronous I/O, and relational databases; familiarity with Docker and Linux.",
                Benefits = "Hybrid flexibility; medical and life insurance; green tech impact; education allowance."
            },

            // --- Frontend & Full-Stack Roles ---
            new
            {
                Title = "Frontend Angular Developer",
                Company = "PayNile FinTech",
                Department = "Frontend",
                Category = "Software Development",
                Country = "Egypt",
                City = "Giza",
                Location = "Smart Village",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 3,
                MaxExp = (int?)5,
                MinSalary = (decimal?)28000,
                MaxSalary = (decimal?)42000,
                Vacancies = 2,
                DeadlineDays = 35,
                PostedDaysAgo = 3,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "Angular", "TypeScript", "HTML", "CSS", "RxJS", "Git" },
                PreferredSkills = new[] { "REST API" },
                Description = "Craft responsive, accessible web dashboards and merchant management portals using modern Angular signals and standalone components.",
                Responsibilities = "Implement interactive dashboards with Angular signals and RxJS; build clean reusable UI components adhering to design system tokens; collaborate with product designers on UX polishing.",
                Requirements = "3+ years professional frontend development; deep knowledge of Angular (v16+ standalone components, signals, reactive forms); strong SCSS/CSS layouts and web accessibility.",
                Benefits = "Modern equipment (MacBook/Dell Precision); medical insurance; transit support; training budget."
            },
            new
            {
                Title = "Full-Stack .NET & Angular Developer",
                Company = "MedFlow HealthTech",
                Department = "Clinical Software",
                Category = "Software Development",
                Country = "Egypt",
                City = "Cairo",
                Location = "Maadi",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Senior,
                MinExp = 5,
                MaxExp = (int?)8,
                MinSalary = (decimal?)42000,
                MaxSalary = (decimal?)60000,
                Vacancies = 1,
                DeadlineDays = 40,
                PostedDaysAgo = 7,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "C#", "ASP.NET Core", "Angular", "TypeScript", "SQL Server", "Git" },
                PreferredSkills = new[] { "Docker", "Entity Framework Core" },
                Description = "End-to-end feature ownership from database schemas and backend endpoints to Angular clinical interfaces.",
                Responsibilities = "Design and build full-stack health features; ensure sub-second query performance for clinical patient search; build reactive Angular frontend forms; write unit and integration tests.",
                Requirements = "5+ years full-stack development with .NET Core and Angular; solid architectural understanding of CQRS, REST, and state management; clean code advocate.",
                Benefits = "Premium family medical insurance; annual bonus; generous PTO; continuous learning sponsorship."
            },
            new
            {
                Title = "Junior Frontend Engineer",
                Company = "EduSpark Learning",
                Department = "Web Team",
                Category = "Software Development",
                Country = "Egypt",
                City = "Giza",
                Location = "Dokki",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Junior,
                MinExp = 1,
                MaxExp = (int?)2,
                MinSalary = (decimal?)13000,
                MaxSalary = (decimal?)18000,
                Vacancies = 1,
                DeadlineDays = 25,
                PostedDaysAgo = 2,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "JavaScript", "TypeScript", "HTML", "CSS", "Git" },
                PreferredSkills = new[] { "Angular" },
                Description = "Join our student experience team creating gamified lesson modules, quizzes, and learner progress dashboards.",
                Responsibilities = "Develop interactive UI elements; convert Figma design specs into responsive CSS/HTML; test frontend behavior across browsers; fix reported frontend defects.",
                Requirements = "1+ years JavaScript/TypeScript experience; proficiency with semantic HTML5 and CSS/SCSS; familiarity with a modern frontend framework like Angular.",
                Benefits = "Mentorship from senior frontend engineers; health insurance; casual culture; growth opportunities."
            },

            // --- DevOps, Cloud & Security Roles ---
            new
            {
                Title = "DevOps & CI/CD Engineer",
                Company = "NileCloud Systems",
                Department = "Infrastructure",
                Category = "DevOps",
                Country = "Egypt",
                City = "New Cairo",
                Location = "North 90th Street",
                Workplace = WorkplaceType.Remote,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Senior,
                MinExp = 4,
                MaxExp = (int?)7,
                MinSalary = (decimal?)40000,
                MaxSalary = (decimal?)58000,
                Vacancies = 2,
                DeadlineDays = 45,
                PostedDaysAgo = 11,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "Docker", "Kubernetes", "CI/CD", "Linux", "Git" },
                PreferredSkills = new[] { "Azure" },
                Description = "Automate deployment pipelines, maintain Kubernetes clusters, and build self-healing cloud infrastructure.",
                Responsibilities = "Manage Kubernetes deployments, Helm charts, and ingress controllers; maintain GitHub Actions / GitLab CI pipelines; monitor cluster health and configure Prometheus/Grafana alerts.",
                Requirements = "4+ years DevOps or SRE experience; deep hands-on Docker and Kubernetes knowledge; strong Linux shell scripting; experience with cloud providers.",
                Benefits = "Fully remote role; home equipment stipend; internet allowance; medical coverage."
            },
            new
            {
                Title = "Cybersecurity Analyst",
                Company = "CyberShield MENA",
                Department = "Security",
                Category = "Cybersecurity",
                Country = "Egypt",
                City = "6th of October",
                Location = "Tech Park",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 2,
                MaxExp = (int?)5,
                MinSalary = (decimal?)26000,
                MaxSalary = (decimal?)38000,
                Vacancies = 1,
                DeadlineDays = 30,
                PostedDaysAgo = 6,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "Linux", "Git", "REST API" },
                PreferredSkills = new[] { "Python" },
                Description = "Monitor security telemetry, perform vulnerability scans, and respond to incident investigations.",
                Responsibilities = "Analyze SIEM security events; conduct vulnerability assessments and penetration test coordination; audit access permissions and security configurations.",
                Requirements = "2+ years in information security; familiarity with OWASP Top 10, network protocols, and incident response frameworks; security certifications (Security+, CEH) preferred.",
                Benefits = "Certification exam reimbursement; private medical; transportation allowance; annual bonus."
            },

            // --- Data, Mobile & Telecom ---
            new
            {
                Title = "Data Analyst",
                Company = "LogiRoute Solutions",
                Department = "Business Intelligence",
                Category = "Data & Analytics",
                Country = "Egypt",
                City = "Alexandria",
                Location = "Fouad Street",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Junior,
                MinExp = 1,
                MaxExp = (int?)3,
                MinSalary = (decimal?)15000,
                MaxSalary = (decimal?)22000,
                Vacancies = 1,
                DeadlineDays = 30,
                PostedDaysAgo = 8,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "SQL Server", "Python" },
                PreferredSkills = new[] { "Git" },
                Description = "Extract delivery insights, fuel consumption trends, and driver turnaround times for logistics fleet operations.",
                Responsibilities = "Write complex SQL queries to extract and cleanse logistics datasets; build executive Power BI / Tableau dashboards; identify operational inefficiencies from delivery metrics.",
                Requirements = "1+ years experience in data analytics; strong SQL querying ability (window functions, subqueries); working knowledge of Python for data analysis (pandas); analytical problem solver.",
                Benefits = "Alexandria sea-view office; hybrid flexibility; social and medical insurance; career path to Data Science."
            },
            new
            {
                Title = "Network Operations Engineer",
                Company = "Apex Telecom AI",
                Department = "Telecom Operations",
                Category = "IT & Networking",
                Country = "Egypt",
                City = "Giza",
                Location = "Smart Village",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 2,
                MaxExp = (int?)4,
                MinSalary = (decimal?)20000,
                MaxSalary = (decimal?)30000,
                Vacancies = 2,
                DeadlineDays = 35,
                PostedDaysAgo = 5,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "Linux" },
                PreferredSkills = new[] { "Python" },
                Description = "Ensure 99.999% uptime for core fiber routing and cellular backhaul transmission lines.",
                Responsibilities = "Monitor real-time network traffic alarms; troubleshoot routing protocols (BGP, OSPF) and fiber outages; manage escalation tickets with field technician teams.",
                Requirements = "2+ years experience in telecommunications or network engineering; CCNA or CCNP certified; strong troubleshooting under pressure.",
                Benefits = "Smart Village transit; life and medical insurance; mobile plan allowance; shift differential pay."
            },
            new
            {
                Title = "Cloud Support Engineer",
                Company = "NileCloud Systems",
                Department = "Technical Support",
                Category = "IT & Networking",
                Country = "Egypt",
                City = "New Cairo",
                Location = "North 90th Street",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.EntryLevel,
                MinExp = 0,
                MaxExp = (int?)2,
                MinSalary = (decimal?)14000,
                MaxSalary = (decimal?)19000,
                Vacancies = 2,
                DeadlineDays = 30,
                PostedDaysAgo = 4,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "Linux", "Git" },
                PreferredSkills = new[] { "Docker" },
                Description = "First-line cloud technical support resolving customer issues with virtual machine provisioning and DNS configurations.",
                Responsibilities = "Triage incoming support tickets; troubleshoot Linux VM connectivity and storage volumes; document troubleshooting guides in internal knowledge base.",
                Requirements = "Bachelor's degree in IT/Computer Science; strong understanding of networking, DNS, and Linux CLI; fluent written and spoken English.",
                Benefits = "Fast-track training to DevOps engineer; private medical; hybrid rotation."
            },
            new
            {
                Title = "FinTech Product Business Analyst",
                Company = "FinBridge Global",
                Department = "Product",
                Category = "Product Management",
                Country = "Egypt",
                City = "Cairo",
                Location = "Zamalek",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 3,
                MaxExp = (int?)5,
                MinSalary = (decimal?)28000,
                MaxSalary = (decimal?)40000,
                Vacancies = 1,
                DeadlineDays = 40,
                PostedDaysAgo = 12,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "JIRA", "REST API", "SQL Server" },
                PreferredSkills = new[] { "Git" },
                Description = "Bridge the gap between banking compliance requirements and technical engineering teams.",
                Responsibilities = "Gather requirements from banking stakeholders; write unambiguous user stories and acceptance criteria in JIRA; map API data fields for payment settlements.",
                Requirements = "3+ years as a Business Analyst in FinTech or Banking; solid grasp of REST APIs and database structures; exceptional documentation skills.",
                Benefits = "Zamalek office; flexible work policy; comprehensive health; annual bonus."
            },

            // --- Additional Varied Categories & Statuses for Filter Testing ---
            new
            {
                Title = "Intern Backend Developer",
                Company = "DeltaCommerce Digital",
                Department = "Engineering",
                Category = "Software Development",
                Country = "Egypt",
                City = "Cairo",
                Location = "Nasr City",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.Internship,
                Level = ExperienceLevel.Internship,
                MinExp = 0,
                MaxExp = (int?)1,
                MinSalary = (decimal?)4000,
                MaxSalary = (decimal?)6000,
                Vacancies = 3,
                DeadlineDays = 20,
                PostedDaysAgo = 2,
                Status = JobStatus.Published,
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "C#", "SQL Server", "Git" },
                PreferredSkills = new[] { "ASP.NET Core" },
                Description = "3-month paid summer internship working alongside senior .NET engineers on e-commerce catalog services.",
                Responsibilities = "Complete assigned coding tasks; learn clean architecture best practices; participate in daily standups and sprint planning.",
                Requirements = "Computer Science university students in 3rd or 4th year; good knowledge of C# fundamentals and SQL; eager to learn.",
                Benefits = "Monthly stipend; potential full-time hire upon graduation; certificate of completion."
            },
            new
            {
                Title = "Contract Database Administrator",
                Company = "Apex Telecom AI",
                Department = "Database Operations",
                Category = "Database Administration",
                Country = "Egypt",
                City = "Giza",
                Location = "Smart Village",
                Workplace = WorkplaceType.Remote,
                Employment = EmploymentType.Contract,
                Level = ExperienceLevel.Senior,
                MinExp = 5,
                MaxExp = (int?)9,
                MinSalary = (decimal?)45000,
                MaxSalary = (decimal?)65000,
                Vacancies = 1,
                DeadlineDays = 20,
                PostedDaysAgo = 14,
                Status = JobStatus.Published,
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "SQL Server", "Linux" },
                PreferredSkills = new[] { "Python" },
                Description = "6-month contract tuning SQL Server AlwaysOn availability groups and index partition optimization for telecom billing.",
                Responsibilities = "Analyze high-wait statistics; redesign index maintenance routines; audit security permissions and backup strategies.",
                Requirements = "5+ years dedicated SQL Server DBA experience; proven track record managing terabyte-scale databases; high availability clustering expert.",
                Benefits = "Competitive contract rate; remote flexibility; contract renewal potential."
            },
            new
            {
                Title = "Draft - Lead Mobile Architect",
                Company = "PayNile FinTech",
                Department = "Mobile Engineering",
                Category = "Software Development",
                Country = "Egypt",
                City = "Giza",
                Location = "Smart Village",
                Workplace = WorkplaceType.Hybrid,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Lead,
                MinExp = 7,
                MaxExp = (int?)12,
                MinSalary = (decimal?)60000,
                MaxSalary = (decimal?)85000,
                Vacancies = 1,
                DeadlineDays = 60,
                PostedDaysAgo = 0,
                Status = JobStatus.Draft, // Testing recruiter draft view
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "TypeScript", "REST API" },
                PreferredSkills = new[] { "CI/CD" },
                Description = "Upcoming leadership role to oversee mobile architecture and security hardening across consumer and merchant mobile apps.",
                Responsibilities = "Define mobile architecture standards; enforce biometric authentication guidelines; mentor mobile leads.",
                Requirements = "7+ years mobile development experience with at least 2 years in architectural or technical leadership.",
                Benefits = "Competitive package; executive benefits."
            },
            new
            {
                Title = "Draft - Junior Cloud Operations",
                Company = "NileCloud Systems",
                Department = "Cloud Ops",
                Category = "DevOps",
                Country = "Egypt",
                City = "New Cairo",
                Location = "North 90th Street",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Junior,
                MinExp = 1,
                MaxExp = (int?)2,
                MinSalary = (decimal?)15000,
                MaxSalary = (decimal?)20000,
                Vacancies = 1,
                DeadlineDays = 45,
                PostedDaysAgo = 0,
                Status = JobStatus.Draft, // Testing recruiter draft view
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "Linux", "Docker" },
                PreferredSkills = new[] { "Git" },
                Description = "Upcoming junior position assisting with server monitoring, log rotation, and backup automation.",
                Responsibilities = "Monitor cloud health alerts; perform routine server patching; assist senior DevOps engineers.",
                Requirements = "1+ years Linux administration; hands-on Docker container knowledge.",
                Benefits = "Career growth; comprehensive medical."
            },
            new
            {
                Title = "Closed - Senior Security Specialist",
                Company = "CyberShield MENA",
                Department = "Security Operations",
                Category = "Cybersecurity",
                Country = "Egypt",
                City = "6th of October",
                Location = "Tech Park",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.Senior,
                MinExp = 5,
                MaxExp = (int?)8,
                MinSalary = (decimal?)45000,
                MaxSalary = (decimal?)60000,
                Vacancies = 1,
                DeadlineDays = -5,
                PostedDaysAgo = 45,
                Status = JobStatus.Closed, // Testing closed status
                Recruiter = primaryRecruiter,
                RequiredSkills = new[] { "Linux" },
                PreferredSkills = new[] { "Python" },
                Description = "Role successfully filled. Candidate selected and onboarding in progress.",
                Responsibilities = "Manage firewall rules; review penetration test reports.",
                Requirements = "5+ years enterprise security experience.",
                Benefits = "Full benefits package."
            },
            new
            {
                Title = "Expired - IoT Embedded Developer",
                Company = "AgroVance Smart Systems",
                Department = "Hardware Team",
                Category = "Software Development",
                Country = "Egypt",
                City = "New Cairo",
                Location = "Fifth Settlement",
                Workplace = WorkplaceType.OnSite,
                Employment = EmploymentType.FullTime,
                Level = ExperienceLevel.MidLevel,
                MinExp = 2,
                MaxExp = (int?)5,
                MinSalary = (decimal?)22000,
                MaxSalary = (decimal?)32000,
                Vacancies = 1,
                DeadlineDays = -10,
                PostedDaysAgo = 60,
                Status = JobStatus.Expired, // Testing expired status
                Recruiter = secondaryRecruiter,
                RequiredSkills = new[] { "C#", "Linux" },
                PreferredSkills = new[] { "Git" },
                Description = "Listing has expired as the application deadline has passed.",
                Responsibilities = "Firmware development for soil sensors.",
                Requirements = "C/C++ or embedded C# experience.",
                Benefits = "Standard benefits."
            }
        };

        var seededJobs = new List<Job>();

        foreach (var jd in jobsData)
        {
            var company = companyMap[jd.Company];
            var existing = await context.Jobs
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.Title == jd.Title && j.CompanyId == company.Id);

            if (existing == null)
            {
                var postedDate = jd.Status == JobStatus.Draft ? (DateTime?)null : now.AddDays(-jd.PostedDaysAgo);
                var deadline = now.AddDays(jd.DeadlineDays);

                var job = new Job
                {
                    Title = jd.Title,
                    Description = jd.Description,
                    Department = jd.Department,
                    Category = jd.Category,
                    Country = jd.Country,
                    City = jd.City,
                    Location = jd.Location,
                    WorkplaceType = jd.Workplace,
                    EmploymentType = jd.Employment,
                    ExperienceLevel = jd.Level,
                    MinimumYearsExperience = jd.MinExp,
                    MaximumYearsExperience = jd.MaxExp,
                    Responsibilities = jd.Responsibilities,
                    Requirements = jd.Requirements,
                    PreferredQualifications = jd.Description,
                    EducationRequirements = "Bachelor's degree in Computer Science, Engineering, or relevant experience",
                    Benefits = jd.Benefits,
                    MinimumSalary = jd.MinSalary,
                    MaximumSalary = jd.MaxSalary,
                    Currency = "EGP",
                    SalaryPeriod = SalaryPeriod.Monthly,
                    IsSalaryVisible = true,
                    NumberOfVacancies = jd.Vacancies,
                    ApplicationDeadline = deadline,
                    CreatedAt = now.AddDays(-jd.PostedDaysAgo - 2),
                    UpdatedAt = now.AddDays(-jd.PostedDaysAgo),
                    PostedAt = postedDate,
                    Status = jd.Status,
                    ClosedAt = jd.Status == JobStatus.Closed ? now.AddDays(-5) : null,
                    CompanyId = company.Id,
                    RecruiterId = jd.Recruiter.Id
                };

                context.Jobs.Add(job);
                await context.SaveChangesAsync();

                // Associate skills
                foreach (var skName in jd.RequiredSkills)
                {
                    if (skillMap.TryGetValue(skName, out var sk))
                    {
                        job.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = sk.Id, IsRequired = true });
                    }
                }
                foreach (var skName in jd.PreferredSkills)
                {
                    if (skillMap.TryGetValue(skName, out var sk))
                    {
                        job.JobSkills.Add(new JobSkill { JobId = job.Id, SkillId = sk.Id, IsRequired = false });
                    }
                }
                await context.SaveChangesAsync();
                seededJobs.Add(job);
            }
            else
            {
                seededJobs.Add(existing);
            }
        }

        // ── 8. Saved Jobs ────────────────────────────────────────────────────────
        var candidateAhmed = candidateMap.GetValueOrDefault("candidate@careerly.local");
        var candidateMariam = candidateMap.GetValueOrDefault("candidate2@careerly.local");
        var candidateOmar = candidateMap.GetValueOrDefault("candidate3@careerly.local");

        var publishedJobs = seededJobs.Where(j => j.Status == JobStatus.Published).ToList();

        if (candidateAhmed != null && publishedJobs.Count >= 3)
        {
            var savedJobIds = publishedJobs.Take(3).Select(j => j.Id).ToList();
            foreach (var jId in savedJobIds)
            {
                if (!await context.SavedJobs.AnyAsync(sj => sj.CandidateId == candidateAhmed.Id && sj.JobId == jId))
                {
                    context.SavedJobs.Add(new SavedJob
                    {
                        CandidateId = candidateAhmed.Id,
                        JobId = jId,
                        CreatedAt = now.AddDays(-2)
                    });
                }
            }
        }

        if (candidateMariam != null && publishedJobs.Count >= 5)
        {
            var qaJobs = publishedJobs.Where(j => j.Category == "Quality Assurance").Take(2).Select(j => j.Id).ToList();
            foreach (var jId in qaJobs)
            {
                if (!await context.SavedJobs.AnyAsync(sj => sj.CandidateId == candidateMariam.Id && sj.JobId == jId))
                {
                    context.SavedJobs.Add(new SavedJob
                    {
                        CandidateId = candidateMariam.Id,
                        JobId = jId,
                        CreatedAt = now.AddDays(-1)
                    });
                }
            }
        }
        await context.SaveChangesAsync();

        // ── 9. Job Applications & History ────────────────────────────────────────
        var dotNetJob = publishedJobs.FirstOrDefault(j => j.Title == ".NET Backend Developer");
        var fullStackJob = publishedJobs.FirstOrDefault(j => j.Title == "Full-Stack .NET & Angular Developer");
        var juniorQaJob = publishedJobs.FirstOrDefault(j => j.Title == "Junior QA Engineer");
        var autoQaJob = publishedJobs.FirstOrDefault(j => j.Title == "Automation QA Engineer");
        var juniorBackendJob = publishedJobs.FirstOrDefault(j => j.Title == "Junior Backend Developer");
        var devopsJob = publishedJobs.FirstOrDefault(j => j.Title == "DevOps & CI/CD Engineer");

        var demoApplications = new List<(Candidate? cand, Job? job, JobApplicationStatus status, string coverLetter, DateTime appliedDate)>();

        if (candidateAhmed != null && dotNetJob != null)
            demoApplications.Add((candidateAhmed, dotNetJob, JobApplicationStatus.Applied,
                "Dear Hiring Team,\n\nI am applying for the .NET Backend Developer role. With 6 years building high-throughput microservices using ASP.NET Core, EF Core, and SQL Server, I would welcome the opportunity to contribute to PayNile's payment platform.", now.AddDays(-3)));

        if (candidateAhmed != null && fullStackJob != null)
            demoApplications.Add((candidateAhmed, fullStackJob, JobApplicationStatus.Interview,
                "Dear MedFlow Team,\n\nI am very excited about MedFlow's clinical software vision. My combined background in backend C# and modern Angular standalone architectures directly aligns with your tech stack.", now.AddDays(-8)));

        if (candidateMariam != null && juniorQaJob != null)
            demoApplications.Add((candidateMariam, juniorQaJob, JobApplicationStatus.Shortlisted,
                "Dear PayNile QA Team,\n\nI have extensive experience with Postman collections, SQL data validation, and exploratory API verification for financial transactional workflows.", now.AddDays(-6)));

        if (candidateMariam != null && autoQaJob != null)
            demoApplications.Add((candidateMariam, autoQaJob, JobApplicationStatus.Interview,
                "Dear DeltaCommerce Team,\n\nI have architected Playwright and Selenium automated test suites across high-traffic checkout flows. I look forward to contributing to your automation coverage.", now.AddDays(-10)));

        if (candidateOmar != null && juniorBackendJob != null)
            demoApplications.Add((candidateOmar, juniorBackendJob, JobApplicationStatus.UnderReview,
                "Hello,\n\nAs a recent computer science graduate passionate about C# and clean backend architecture, I am eager to contribute to DeltaCommerce's backend development.", now.AddDays(-2)));

        if (candidateAhmed != null && devopsJob != null)
            demoApplications.Add((candidateAhmed, devopsJob, JobApplicationStatus.Accepted,
                "Dear NileCloud Team,\n\nI have extensive experience with containerization, Docker, and deployment automation alongside backend systems.", now.AddDays(-15)));

        var seededApplications = new List<JobApplication>();

        foreach (var appData in demoApplications)
        {
            if (appData.cand == null || appData.job == null) continue;

            var existingApp = await context.JobApplications
                .Include(ja => ja.StatusHistory)
                .Include(ja => ja.Interviews)
                .FirstOrDefaultAsync(ja => ja.CandidateId == appData.cand.Id && ja.JobId == appData.job.Id);

            if (existingApp == null)
            {
                var jobApp = new JobApplication
                {
                    CandidateId = appData.cand.Id,
                    JobId = appData.job.Id,
                    JobApplicationStatus = appData.status,
                    AppliedAt = appData.appliedDate,
                    StatusUpdatedAt = appData.appliedDate.AddDays(1),
                    CoverLetter = appData.coverLetter,
                    SubmittedCvId = null
                };
                context.JobApplications.Add(jobApp);
                await context.SaveChangesAsync();

                // Add audit history
                var recruiterUserId = primaryRecruiter.UserId;

                context.ApplicationStatusHistory.Add(new ApplicationStatusHistory
                {
                    ApplicationId = jobApp.Id,
                    OldStatus = null,
                    NewStatus = JobApplicationStatus.Applied,
                    ChangedByUserId = appData.cand.UserId,
                    ChangedAt = appData.appliedDate,
                    Note = "Candidate submitted application."
                });

                if (appData.status == JobApplicationStatus.UnderReview ||
                    appData.status == JobApplicationStatus.Shortlisted ||
                    appData.status == JobApplicationStatus.Interview ||
                    appData.status == JobApplicationStatus.Accepted)
                {
                    context.ApplicationStatusHistory.Add(new ApplicationStatusHistory
                    {
                        ApplicationId = jobApp.Id,
                        OldStatus = JobApplicationStatus.Applied,
                        NewStatus = JobApplicationStatus.UnderReview,
                        ChangedByUserId = recruiterUserId,
                        ChangedAt = appData.appliedDate.AddDays(1),
                        Note = "Recruiter reviewed profile qualifications."
                    });
                }

                if (appData.status == JobApplicationStatus.Shortlisted ||
                    appData.status == JobApplicationStatus.Interview ||
                    appData.status == JobApplicationStatus.Accepted)
                {
                    context.ApplicationStatusHistory.Add(new ApplicationStatusHistory
                    {
                        ApplicationId = jobApp.Id,
                        OldStatus = JobApplicationStatus.UnderReview,
                        NewStatus = JobApplicationStatus.Shortlisted,
                        ChangedByUserId = recruiterUserId,
                        ChangedAt = appData.appliedDate.AddDays(2),
                        Note = "Candidate shortlisted for technical screening."
                    });
                }

                if (appData.status == JobApplicationStatus.Interview)
                {
                    context.ApplicationStatusHistory.Add(new ApplicationStatusHistory
                    {
                        ApplicationId = jobApp.Id,
                        OldStatus = JobApplicationStatus.Shortlisted,
                        NewStatus = JobApplicationStatus.Interview,
                        ChangedByUserId = recruiterUserId,
                        ChangedAt = appData.appliedDate.AddDays(3),
                        Note = "Interview scheduled with hiring team."
                    });
                }

                if (appData.status == JobApplicationStatus.Accepted)
                {
                    context.ApplicationStatusHistory.Add(new ApplicationStatusHistory
                    {
                        ApplicationId = jobApp.Id,
                        OldStatus = JobApplicationStatus.Interview,
                        NewStatus = JobApplicationStatus.Accepted,
                        ChangedByUserId = recruiterUserId,
                        ChangedAt = appData.appliedDate.AddDays(5),
                        Note = "Job offer extended and accepted."
                    });
                }

                await context.SaveChangesAsync();
                seededApplications.Add(jobApp);
            }
            else
            {
                seededApplications.Add(existingApp);
            }
        }

        // ── 10. Interviews ───────────────────────────────────────────────────────
        var interviewApps = seededApplications
            .Where(a => a.JobApplicationStatus == JobApplicationStatus.Interview)
            .ToList();

        if (interviewApps.Count > 0)
        {
            var app1 = interviewApps[0];
            if (!await context.Interviews.AnyAsync(i => i.ApplicationId == app1.Id))
            {
                context.Interviews.Add(new Interview
                {
                    ApplicationId = app1.Id,
                    ScheduledAt = now.AddDays(3).Date.AddHours(14), // In 3 days at 2:00 PM UTC
                    DurationMinutes = 45,
                    InterviewType = InterviewType.Online,
                    MeetingLink = "https://meet.google.com/crly-demo-tech",
                    Location = "Google Meet Online",
                    Notes = "Technical deep-dive on clean architecture, EF Core indexing, and Angular signals.",
                    Status = InterviewStatus.Scheduled,
                    ScheduledByUserId = primaryRecruiter.UserId,
                    CreatedAt = now.AddDays(-1),
                    UpdatedAt = now.AddDays(-1)
                });
            }
        }

        if (interviewApps.Count > 1)
        {
            var app2 = interviewApps[1];
            if (!await context.Interviews.AnyAsync(i => i.ApplicationId == app2.Id))
            {
                context.Interviews.Add(new Interview
                {
                    ApplicationId = app2.Id,
                    ScheduledAt = now.AddDays(7).Date.AddHours(11), // In 7 days at 11:00 AM UTC
                    DurationMinutes = 60,
                    InterviewType = InterviewType.OnSite,
                    MeetingLink = null,
                    Location = "DeltaCommerce HQ, Makram Ebeid, Nasr City, Cairo - 4th Floor Conference Room A",
                    Notes = "On-site panel interview covering automated test architecture, Playwright fixtures, and team culture.",
                    Status = InterviewStatus.Scheduled,
                    ScheduledByUserId = secondaryRecruiter.UserId,
                    CreatedAt = now.AddDays(-2),
                    UpdatedAt = now.AddDays(-2)
                });
            }
        }
        await context.SaveChangesAsync();

        // ── 11. Notifications ────────────────────────────────────────────────────
        var notificationsToSeed = new List<Notification>();

        if (candidateAhmed != null)
        {
            notificationsToSeed.Add(new Notification
            {
                UserId = candidateAhmed.UserId,
                Type = NotificationType.InterviewScheduled,
                Title = "Interview Scheduled: Full-Stack .NET & Angular Developer",
                Message = "Your technical interview has been scheduled for Google Meet. Click to view details and meeting link.",
                IsRead = false,
                CreatedAt = now.AddHours(-4)
            });
            notificationsToSeed.Add(new Notification
            {
                UserId = candidateAhmed.UserId,
                Type = NotificationType.ApplicationAccepted,
                Title = "Congratulations! Offer Extended",
                Message = "Your application for DevOps & CI/CD Engineer has been accepted. Welcome to the NileCloud team!",
                IsRead = true,
                CreatedAt = now.AddDays(-2),
                ReadAt = now.AddDays(-1)
            });
        }

        if (candidateMariam != null)
        {
            notificationsToSeed.Add(new Notification
            {
                UserId = candidateMariam.UserId,
                Type = NotificationType.InterviewScheduled,
                Title = "On-Site Interview: Automation QA Engineer",
                Message = "DeltaCommerce has scheduled your on-site interview in Nasr City. Please review the appointment details.",
                IsRead = false,
                CreatedAt = now.AddHours(-12)
            });
            notificationsToSeed.Add(new Notification
            {
                UserId = candidateMariam.UserId,
                Type = NotificationType.CandidateShortlisted,
                Title = "Shortlisted: Junior QA Engineer",
                Message = "PayNile FinTech has shortlisted your profile for the Junior QA Engineer position.",
                IsRead = true,
                CreatedAt = now.AddDays(-3),
                ReadAt = now.AddDays(-2)
            });
        }

        if (recruiter1User != null)
        {
            notificationsToSeed.Add(new Notification
            {
                UserId = recruiter1User.Id,
                Type = NotificationType.NewApplication,
                Title = "New Candidate Application",
                Message = "Ahmed Hassan submitted an application for the .NET Backend Developer role.",
                IsRead = false,
                CreatedAt = now.AddHours(-2)
            });
            notificationsToSeed.Add(new Notification
            {
                UserId = recruiter1User.Id,
                Type = NotificationType.NewApplication,
                Title = "New Candidate Application",
                Message = "Mariam Mostafa submitted an application for the Junior QA Engineer role.",
                IsRead = true,
                CreatedAt = now.AddDays(-4),
                ReadAt = now.AddDays(-3)
            });
        }

        foreach (var notif in notificationsToSeed)
        {
            if (!await context.Notifications.AnyAsync(n => n.UserId == notif.UserId && n.Title == notif.Title))
            {
                context.Notifications.Add(notif);
            }
        }
        await context.SaveChangesAsync();

        // ── 12. Notification Preferences ─────────────────────────────────────────
        foreach (var user in userMap.Values)
        {
            if (!await context.NotificationPreferences.AnyAsync(np => np.UserId == user.Id))
            {
                context.NotificationPreferences.Add(new NotificationPreference
                {
                    UserId = user.Id,
                    InAppEnabled = true,
                    EmailEnabled = true,
                    JobAlertsEnabled = true,
                    UpdatedAt = now
                });
            }
        }
        await context.SaveChangesAsync();

        logger.LogInformation("Careerly development data seeding completed successfully.");
    }
}
