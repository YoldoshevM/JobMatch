
using JobMatch.Data;
using JobMatch.Models;
using Microsoft.EntityFrameworkCore;

namespace JobMatch.Seeders;

public class SkillSeeder
{
    // Dastur ishga tushganda skill'larni bazaga qo'shadi.
    public static async Task SeedAsync(AppDbContext context)
    {
        // IT yo'nalishlaridagi skill'lar.
        string[] skillNames =
        {
            // Backend
            "C#", ".NET", "ASP.NET Core", "Entity Framework Core",
            "Java", "Spring Boot", "Python", "Django", "FastAPI",
            "Node.js", "Express.js", "NestJS", "PHP", "Laravel",
            "Go", "Rust", "REST API", "GraphQL", "gRPC",
            "Microservices", "RabbitMQ", "Kafka", "SignalR",
            "LINQ", "OOP", "SOLID", "JWT", "OAuth 2.0",

            // Frontend
            "HTML", "CSS", "JavaScript", "TypeScript",
            "React", "Angular", "Vue.js", "Next.js",
            "Nuxt.js", "Redux", "Tailwind CSS", "Bootstrap",
            "Sass", "Webpack", "Vite", "jQuery",
            "Responsive Design", "Material UI",

            // Database
            "SQL", "PostgreSQL", "SQL Server", "MySQL",
            "SQLite", "MongoDB", "Redis", "Oracle",
            "Elasticsearch", "Firebase", "DynamoDB",
            "Database Design", "Query Optimization",

            // AI / Machine Learning
            "Machine Learning", "Deep Learning",
            "Artificial Intelligence", "TensorFlow",
            "PyTorch", "Scikit-learn", "Pandas", "NumPy",
            "OpenCV", "NLP", "Computer Vision",
            "LLM", "LangChain", "LlamaIndex",
            "Hugging Face", "RAG", "Vector Database",
            "Prompt Engineering", "AI Agents",

            // DevOps / Cloud
            "Docker", "Kubernetes", "GitHub Actions",
            "GitLab CI", "Jenkins", "CI/CD", "Linux",
            "Nginx", "AWS", "Azure", "Google Cloud",
            "Terraform", "Ansible", "Prometheus",
            "Grafana", "Bash", "PowerShell",

            // Cybersecurity
            "Cybersecurity", "Penetration Testing",
            "Ethical Hacking", "Network Security",
            "OWASP", "Burp Suite", "Wireshark",
            "Kali Linux", "Metasploit", "SIEM",
            "Cryptography", "Vulnerability Assessment",
            "Incident Response", "SOC",

            // Mobile
            "Flutter", "Dart", "React Native",
            "Swift", "SwiftUI", "Kotlin",
            "Android", "iOS", "Jetpack Compose",
            "Xamarin", ".NET MAUI",

            // Testing
            "Unit Testing", "Integration Testing",
            "Selenium", "Playwright", "Cypress",
            "Postman", "Swagger", "xUnit", "NUnit",
            "Jest", "Pytest",

            // General
            "Git", "GitHub", "GitLab",
            "Agile", "Scrum", "Data Structures",
            "Algorithms", "Design Patterns",
            "Clean Architecture", "Blazor",
            "C++", "C", "Figma"
        };

        // Bazada mavjud skill'larni olamiz.
        var existingSkills = await context.Skills
            .Select(s => s.Name)
            .ToListAsync();

        // Bazada yo'q skill'larni qo'shamiz.
        foreach (var name in skillNames)
        {
            bool exists = existingSkills.Any(s =>
                s.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                context.Skills.Add(new Skill
                {
                    Name = name
                });
            }
        }

        // O'zgarishlarni PostgreSQL'ga saqlaymiz.
        await context.SaveChangesAsync();
    }
}
