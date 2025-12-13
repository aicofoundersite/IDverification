var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<CrossSetaWeb.DataAccess.IDatabaseHelper, CrossSetaWeb.DataAccess.DatabaseHelper>();
builder.Services.AddScoped<CrossSetaWeb.Services.IUserService, CrossSetaWeb.Services.UserService>();
builder.Services.AddScoped<CrossSetaWeb.Services.IKYCService, CrossSetaWeb.Services.KYCService>();
builder.Services.AddScoped<CrossSetaWeb.Services.IBulkRegistrationService, CrossSetaWeb.Services.BulkRegistrationService>();
builder.Services.AddScoped<CrossSetaWeb.Services.IDatabaseValidationService, CrossSetaWeb.Services.DatabaseValidationService>();
builder.Services.AddScoped<CrossSetaWeb.Services.IHomeAffairsImportService, CrossSetaWeb.Services.HomeAffairsImportService>();
builder.Services.AddSingleton<CrossSetaWeb.Services.IValidationProgressService, CrossSetaWeb.Services.ValidationProgressService>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
