using Microsoft.AspNetCore.Authentication.Cookies;
using CrossSetaLogic.DataAccess;
using CrossSetaLogic.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Keep user logged in for 8 hours
    });

builder.Services.AddScoped<Supabase.Client>(provider => {
    var config = provider.GetRequiredService<IConfiguration>();
    var url = config["Supabase:Url"];
    var key = config["Supabase:Key"];
    return new Supabase.Client(url, key);
});

builder.Services.AddScoped<IDatabaseHelper, DatabaseHelper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IKYCService, KYCService>();
builder.Services.AddScoped<IBulkRegistrationService, BulkRegistrationService>();
builder.Services.AddScoped<IDatabaseValidationService, DatabaseValidationService>();
builder.Services.AddScoped<IHomeAffairsImportService, HomeAffairsImportService>();
builder.Services.AddSingleton<IValidationProgressService, ValidationProgressService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var bulkService = scope.ServiceProvider.GetRequiredService<IBulkRegistrationService>();
    var dbHelper = scope.ServiceProvider.GetRequiredService<IDatabaseHelper>();
    
    try 
    {
        // Ensure Users table and SP exist
        dbHelper.InitializeUserSchema();
        dbHelper.InitializeUserActivitySchema();

        // Simple check to avoid re-seeding if data is present
        // We catch exception in case DB is not ready or other issues
        var existingLearners = dbHelper.GetAllLearners();
        logger.LogInformation($"Current learner count: {existingLearners.Count}");
        
        if (existingLearners.Count <= 10) // Threshold for "empty" or "test data only"
        {
            logger.LogInformation("Learner count is low. Attempting to seed from LearnerData.csv");
            // Ensure wwwroot path is correct
            var seedPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads", "LearnerData.csv");
            bulkService.SeedLearners(seedPath);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during startup seeding.");
    }
}

app.Run();
