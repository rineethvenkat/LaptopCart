using LaptopCart.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/*
 * This line registers the ApplicationDbContext with the dependency injection container.
 * It configures the DbContext to use SQL Server as the database provider, using the connection string named "LaptopCartDBConnection" from the application's configuration.
 * This allows the application to interact with the database using Entity Framework Core.
 */
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LaptopCartDBConnection")));

// This line adds ASP.NET Core Identity services to the application, specifying ApplicationUser as the user entity and IdentityRole as the role entity.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

/*
 * This block of code configures the application cookie settings for authentication.
 * It sets the login path to "/Account/Login", which means that if a user tries to access a protected resource without being authenticated, they will be redirected to this login page.
 * It also sets the access denied path to "/Account/AccessDenied", which is the page users will be redirected to if they try to access a resource they don't have permission for.
 */
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

/*
 * This block of code creates a scope for the application's services and retrieves the IServiceProvider.
 * It then calls the SeedRoles method to ensure that the specified roles ("Admin", "User", "Manager") exist in the database.
 * If any of these roles do not exist, they will be created.
 */
async Task SeedRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roleNames = { "Admin", "User", "Manager" };
    //IdentityResult roleResult;
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Create the roles and seed them to the database
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

// This block of code creates a scope for the application's services and retrieves the IServiceProvider.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRoles(services);
}       

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// session must be enabled before authentication/authorization if you rely on session during auth
app.UseSession();

// enable authentication before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=ProductView}/{id?}")
    .WithStaticAssets();


app.Run();
