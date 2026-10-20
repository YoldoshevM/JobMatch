
using JobMatch.Data;
using JobMatch.Services;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using JobMatch.Seeders;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL database ulanishi
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Service'larni ulash
builder.Services.AddScoped<CompanyService>();

// Controller'larni ulash
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

// Swagger xizmatlari
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<VacancyService>();
builder.Services.AddScoped<SkillService>();
builder.Services.AddScoped<ResumeService>();
builder.Services.AddSwaggerGen();
// Vakansiya skill talablari bilan ishlaydigan Service.
builder.Services.AddScoped<VacancyRequirementService>();
// CV va vakansiya mosligini hisoblaydigan Service.
builder.Services.AddScoped<JobMatchService>();

var app = builder.Build();


// Dastur ishga tushganda IT skill'larni bazaga qo'shamiz.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    await SkillSeeder.SeedAsync(context);

    await VacancySeeder.SeedAsync(context);
}



// Swagger faqat Development rejimida ishlaydi
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// wwwroot ichidagi index.html ni asosiy sahifa qilamiz.
app.UseDefaultFiles();

// wwwroot ichidagi HTML, CSS va JS fayllarini ochamiz.
app.UseStaticFiles();


app.UseAuthorization();

app.MapControllers();

app.Run();
