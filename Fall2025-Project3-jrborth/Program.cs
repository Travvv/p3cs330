using Fall2025_Project3_jrborth.Data;
using Fall2025_Project3_jrborth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Read the connection string with placeholder from configuration
var defaultConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

// Example: register a DbContext using the constructed connection string
// Replace `ApplicationDbContext` with your actual DbContext type.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(defaultConn));

// Register Razor Pages (existing project likely already has this)
builder.Services.AddRazorPages();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IAzureOpenAIService, AzureOpenAIService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
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

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
