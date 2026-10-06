using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using PolicyClaimHub.Data;
using PolicyClaimHub.Services.Claims;
using PolicyClaimHub.Services.OracleClaims;
using PolicyClaimHub.Services.Renewals;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()));
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ไม่พบ Connection String ชื่อ DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

var oracleConnectionString =
    builder.Configuration.GetConnectionString("OracleConnection");
if (!string.IsNullOrWhiteSpace(oracleConnectionString))
{
    var oracleConnectionBuilder =
        new OracleConnectionStringBuilder(oracleConnectionString);
    if (!string.IsNullOrWhiteSpace(oracleConnectionBuilder.TnsAdmin))
    {
        // ODP.NET must know where the TNS configuration files are before
        // the first Oracle connection opens.
        OracleConfiguration.TnsAdmin = oracleConnectionBuilder.TnsAdmin;
    }
}

builder.Services.AddScoped<IClaimEstimationService, ClaimEstimationService>();
builder.Services.AddScoped<IRenewalAssessmentService, RenewalAssessmentService>();
builder.Services.AddScoped<IOracleFloodClaimService, OracleFloodClaimService>();
builder.Services.AddHttpClient("NowBangkok", client =>
{
    client.BaseAddress = new Uri("https://now.bangkok.go.th/");
    client.Timeout = TimeSpan.FromSeconds(12);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("PolicyClaimHub/1.0");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();
    var estimationService = scope.ServiceProvider
        .GetRequiredService<IClaimEstimationService>();

    await dbContext.Database.MigrateAsync();
    await DbSeeder.SeedAsync(dbContext, estimationService);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
