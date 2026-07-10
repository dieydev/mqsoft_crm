using AI_CRM.Domain.Entities;
using AI_CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AI_CRM.WebMvc.Services;
using AI_CRM.WebMvc.Filters;

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
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IEmployeeService, AI_CRM.Infrastructure.Services.EmployeeService>();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.IReportService, AI_CRM.Infrastructure.Services.ReportService>();

// Cấu hình giới hạn file upload tối đa lên 50MB (mặc định của Kestrel chỉ là 28.6MB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50MB
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50MB
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultSignInScheme = "ExternalCookie";
        options.DefaultChallengeScheme = "Cookies";
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



// Đăng ký Activity Service (Singleton để lưu data trên RAM)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AI_CRM.Application.Interfaces.ICurrentUserService, AI_CRM.WebMvc.Services.CurrentUserService>();
builder.Services.AddSingleton<IUserActivityService, UserActivityService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<UserActivityFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

if (!app.Environment.IsDevelopment())
{
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

// Gọi Seeder để tạo dữ liệu giả lập cho biểu đồ (Phần 3)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        AI_CRM.Infrastructure.Data.DbSeeder.SeedData(services);
    }
    catch (System.Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
