using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using galutine.Data;
using galutine.Models;
using galutine.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// DATABASE (SQLite)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// IDENTITY CONFIGURATION
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // require confirmed email
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// EMAIL SERVICE (STUB)
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// AUTOMATIC MIGRATION
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    // Seed the first admin user with confirmed email
    await SeedAdminAsync(services);
}

// MIDDLEWARE PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// BLOCKED USER CHECK
app.Use(async (context, next) =>
{
    if (context.User?.Identity?.IsAuthenticated ?? false)
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.User);

        if (user == null || user.IsBlocked)
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            context.Response.Redirect("/Account/Login");
            return;
        }
    }

    await next();
});

// ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

// --------------------------
// Seed Admin Method
// --------------------------
async Task SeedAdminAsync(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Ensure the Admin role exists
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Define the first admin email
    var adminEmail = "admin@gmail.com";

    // Check if the user exists
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // verified email
        };
        await userManager.CreateAsync(adminUser, "Admin");
    }
    else if (!adminUser.EmailConfirmed)
    {
        adminUser.EmailConfirmed = true; // auto-confirm if not already
        await userManager.UpdateAsync(adminUser);
    }

    // Only assign admin role if email is verified
    if (adminUser.EmailConfirmed && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
