
using JobMatch.Data;
using JobMatch.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Services;

public static class VacancySeeder
{
    private record SkillInfo(string Name, int Weight, bool Required);

    private record JobInfo(
        string Company,
        string Website,
        string Title,
        string Description,
        string Location,
        string Url,
        SkillInfo[] Skills
    );

    private static SkillInfo R(string name, int weight = 4)
        => new(name, weight, true);

    private static SkillInfo O(string name, int weight = 2)
        => new(name, weight, false);

    public static async Task SeedAsync(AppDbContext db)
    {
        // Har bir URL individual ariza sahifasiga tegishli.
        // Skill weights — JobMatch ichki baholash tizimi.

        var jobs = new[]
        {
            new JobInfo(
                "MEMX",
                "https://memx.com",
                "Software Engineer, Core Software Engineering (International Remote Hire)",
                "6+ years of real-time server-side Java development. " +
                "Low-latency, high-throughput Linux trading systems. " +
                "US evening shift: 10 PM–6 AM EST. " +
                "Event sourcing, TCP/IP, UDP and FIX are pluses.",
                "International Remote — Uzbekistan eligible",
                "https://job-boards.greenhouse.io/memx/jobs/4474788008",
                new[]
                {
                    R("Java", 5),
                    R("Linux", 4),
                    O("Event Sourcing"),
                    O("FIX Protocol", 3),
                    O("Networking", 3)
                }
            ),

            new JobInfo(
                "Automattic",
                "https://automattic.com",
                "Experienced Software Engineer",
                "Experienced software engineer for a globally distributed " +
                "team building products such as WordPress.com and WooCommerce. " +
                "Strong debugging, engineering fundamentals, production " +
                "experience across languages and systems, and collaboration. " +
                "PHP and JavaScript are primary technologies but are not " +
                "strict prerequisites.",
                "Worldwide Remote",
                "https://job-boards.greenhouse.io/automatticcareers/jobs/5862239",
                new[]
                {
                    R("Debugging", 5),
                    R("Software Engineering", 5),
                    O("PHP", 3),
                    O("JavaScript", 3),
                    O("Git", 2)
                }
            ),

            new JobInfo(
                "Apaleo",
                "https://apaleo.com",
                "Staff Backend Engineer (.NET)",
                "8+ years of backend engineering and technical leadership. " +
                "Distributed cloud-native systems, Docker, infrastructure " +
                "as code, testing and large-scale platform ownership. " +
                "C#/.NET, PostgreSQL, Kubernetes and Kafka are nice to have.",
                "Munich / Germany or Europe Remote",
                "https://job-boards.greenhouse.io/apaleo/jobs/7728123003",
                new[]
                {
                    R("Docker", 5),
                    R("Terraform", 5),
                    R("Unit Testing", 4),
                    R("Integration Testing", 4),
                    O("C#", 3),
                    O(".NET", 3),
                    O("PostgreSQL", 3),
                    O("Kafka", 3),
                    O("Kubernetes", 3),
                    O("AWS", 3)
                }
            ),

            new JobInfo(
                "Apaleo",
                "https://apaleo.com",
                "Senior Software Engineer (.NET)",
                "5+ years of software engineering. Build .NET 10 " +
                "backend services in Docker, develop APIs and work " +
                "with Kafka or similar messaging, AWS, Kubernetes, " +
                "PostgreSQL, microservices and automated tests.",
                "Munich / Remote — regional eligibility applies",
                "https://job-boards.greenhouse.io/apaleo/jobs/7730251003",
                new[]
                {
                    R(".NET", 5),
                    R("Docker", 5),
                    R("PostgreSQL", 4),
                    R("Microservices", 4),
                    R("AWS", 4),
                    R("Unit Testing", 3),
                    O("Kafka", 3),
                    O("Kubernetes", 3)
                }
            ),

            new JobInfo(
                "Canonical",
                "https://canonical.com",
                "Python Software Engineer, Commercial Systems",
                "Develop and maintain Python-based commercial " +
                "systems for Canonical. Remote-first engineering " +
                "environment. Review the official posting for " +
                "full experience, education and location requirements.",
                "Remote — country eligibility to confirm",
                "https://job-boards.greenhouse.io/canonical/jobs/6401160",
                new[]
                {
                    R("Python", 5),
                    O("Linux", 3)
                }
            ),

            new JobInfo(
                "Canonical",
                "https://canonical.com",
                "Software Engineer (Python/Linux/Packaging)",
                "Software engineering focused on Python, Linux, " +
                "packaging and build toolchains. Experience with " +
                "compilation and cross-compilation is relevant. " +
                "Review the official posting for full requirements.",
                "Remote — country eligibility to confirm",
                "https://job-boards.greenhouse.io/canonical/jobs/2413329",
                new[]
                {
                    R("Python", 5),
                    R("Linux", 5),
                    O("Bash", 3)
                }
            )
        };

        foreach (var job in jobs)
        {
            var company = await db.Companies
                .FirstOrDefaultAsync(c => c.Name == job.Company);

            if (company == null)
            {
                company = new Company
                {
                    Name = job.Company,
                    WebsiteUrl = job.Website,
                    Description = "Technology company"
                };

                db.Companies.Add(company);
                await db.SaveChangesAsync();
            }

            var vacancy = await db.Vacancies
                .FirstOrDefaultAsync(v =>
                    v.ApplicationUrl == job.Url);

            if (vacancy == null)
            {
                vacancy = new Vacancy
                {
                    CompanyId = company.Id,
                    Title = job.Title,
                    Description = job.Description,
                    Location = job.Location,
                    ApplicationUrl = job.Url,
                    SalaryFrom = null,
                    SalaryTo = null
                };

                db.Vacancies.Add(vacancy);
                await db.SaveChangesAsync();
            }

            foreach (var item in job.Skills)
            {
                var skill = await db.Skills
                    .FirstOrDefaultAsync(s =>
                        s.Name == item.Name);

                if (skill == null)
                {
                    skill = new Skill
                    {
                        Name = item.Name
                    };

                    db.Skills.Add(skill);
                    await db.SaveChangesAsync();
                }

                var requirement = await db.VacancyRequirements
                    .FirstOrDefaultAsync(r =>
                        r.VacancyId == vacancy.Id &&
                        r.SkillId == skill.Id);

                if (requirement == null)
                {
                    db.VacancyRequirements.Add(
                        new VacancyRequirement
                        {
                            VacancyId = vacancy.Id,
                            SkillId = skill.Id,
                            Weight = item.Weight,
                            IsRequired = item.Required
                        });
                }
            }

            await db.SaveChangesAsync();

            Console.WriteLine(
                $"JobMatch: Imported {job.Company} — {job.Title}");
        }

        Console.WriteLine("JobMatch vacancy seeding completed.");
    }
}
