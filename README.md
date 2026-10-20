# 🚀 JobMatch — Smart CV Analysis & Job Matching Platform

**An intelligent job matching platform built with ASP.NET Core, Entity Framework Core and PostgreSQL.**

JobMatch helps job seekers analyze their CVs, compare their skills against job requirements, and receive personalized recommendations to improve their qualifications.

## 🛠️ Tech Stack

- **Backend:** C#, ASP.NET Core Web API
- **Database:** PostgreSQL, Entity Framework Core
- **Frontend:** HTML, CSS, JavaScript
- **CV Processing:** PdfPig
- **API Documentation:** Swagger / OpenAPI
- **Version Control:** Git & GitHub

## ✨ Key Features

- 📄 **CV Analysis** — Extract and identify technical skills from uploaded CVs.
- 🎯 **Smart Job Matching** — Calculate a weighted compatibility score between a CV and a vacancy.
- 📊 **Skill Gap Analysis** — Identify matched and missing skills.
- 💡 **Personalized Recommendations** — Prioritize missing required skills and suggest improvements.
- 🔍 **Vacancy Search** — Find vacancies based on relevant keywords.
- 🏢 **Vacancy Management** — Manage companies, vacancies and skill requirements.
- 🔗 **Apply Links** — Open external job application pages.

## 🧠 How Job Matching Works

1. The user uploads a CV.
2. The system extracts and identifies technical skills.
3. The user selects a vacancy.
4. JobMatch compares the identified skills with vacancy requirements.
5. The system calculates a weighted match percentage.
6. Missing skills and personalized recommendations are displayed.

**Matching formula:**

`Match % = (Matched Skill Weights / Total Skill Weights) × 100`

*The match score measures skill compatibility, not the probability of being hired.*

## 📁 Project Structure

```text
JobMatch/
├── Controllers/
├── Data/
├── DTOs/
├── Migrations/
├── Models/
├── Seeders/
├── Services/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── index.html
├── Program.cs
└── appsettings.example.json
```

## ⚙️ Getting Started

1. Clone the repository.
2. Install the required .NET SDK and PostgreSQL.
3. Create `appsettings.json` using `appsettings.example.json` as a template.
4. Configure your PostgreSQL connection string.
5. Restore dependencies and apply EF Core migrations.
6. Run the application.

## 👨‍💻 Developer

**Muhammadjon Yoldoshev**

.NET Backend Developer | C# | ASP.NET Core | PostgreSQL

GitHub: [@YoldoshevM](https://github.com/YoldoshevM)
