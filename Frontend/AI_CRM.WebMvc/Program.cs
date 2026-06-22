using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Services
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IUserService, AI_CRM.Infrastructure.Services.UserService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.ICustomerService, AI_CRM.Infrastructure.Services.CustomerService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IContractService, AI_CRM.Infrastructure.Services.ContractService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IProjectService, AI_CRM.Infrastructure.Services.ProjectService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.ITaskService, AI_CRM.Infrastructure.Services.TaskService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IDocumentService, AI_CRM.Infrastructure.Services.DocumentService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IInteractionService, AI_CRM.Infrastructure.Services.InteractionService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IChatbotService, AI_CRM.Infrastructure.Services.ChatbotService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IAdminService, AI_CRM.Infrastructure.Services.AdminService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IAuthService, AI_CRM.Infrastructure.Services.AuthService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultSignInScheme = "ExternalCookie";
        // Do not set DefaultChallengeScheme here so we can support both Cookie and Google
    })
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    })
    .AddCookie("ExternalCookie")
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "dummy-client-id";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "dummy-client-secret";
        options.CallbackPath = "/signin-google";
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
