using InsuranceClaims.Data;
using InsuranceClaims.Repositories;
using InsuranceClaims.Services.Implementations;
using InsuranceClaims.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});

// Repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<OfficerRepository>();
builder.Services.AddScoped<SurveyorRepository>();
builder.Services.AddScoped<PolicyRepository>();
builder.Services.AddScoped<ClaimRepository>();
builder.Services.AddScoped<DocumentRepository>();
builder.Services.AddScoped<AssessmentRepository>();
builder.Services.AddScoped<FraudRepository>();
builder.Services.AddScoped<SettlementRepository>();
builder.Services.AddScoped<SurveyorAssignmentRepository>();  // NEW

// Services
builder.Services.AddScoped<IAccountService,    AccountService>();
builder.Services.AddScoped<IPolicyService,     PolicyService>();
builder.Services.AddScoped<IClaimService,      ClaimService>();
builder.Services.AddScoped<IDocumentService,   DocumentService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<IFraudService,      FraudService>();
builder.Services.AddScoped<ISettlementService, SettlementService>();
builder.Services.AddScoped<IAdminService,      AdminService>();

var app = builder.Build();

await DbInitializer.SeedAsync(app);

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
