using IdentityManager;
using IdentityManager.Authorize;
using IdentityManager.Data;
using IdentityManager.Helpers;
using IdentityManager.IService;
using IdentityManager.Models;
using IdentityManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // AddDefaultTokenProviders => we use this for _userManager.GeneratePasswordResetTokenAsync
builder.Services.ConfigureApplicationCookie(options =>
{
    //options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/NoAccess";
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Lockout.MaxFailedAccessAttempts = 50;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.SignIn.RequireConfirmedEmail = false;
});


builder.Services.AddScoped<INumberOfDaysForAccount, NumberOfDaysForAccount>();
builder.Services.AddScoped<IAuthorizationHandler, AdminWithMoreThan1000DaysHandler>();
builder.Services.AddScoped<IAuthorizationHandler, FirstNameAuthHandler>();

// For Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole(SD.Admin));
    options.AddPolicy("AdminAndUser", policy => policy.RequireRole(SD.Admin).RequireRole(SD.User));
    options.AddPolicy("AdminRole_CreateClaim", policy => policy.RequireRole(SD.Admin).RequireClaim("Create", "True")); // The Create is comming from db

    options.AddPolicy("AdminRole_CreateEditDeleteClaim", policy => policy
            .RequireRole(SD.Admin)
            .RequireClaim("Create", "True")
            .RequireClaim("Edit", "True")
            .RequireClaim("Delete", "True"));

    options.AddPolicy("AdminRole_CreateEditDeleteClaim_ORSuperAdminRole", policy => policy.RequireAssertion(context => new PolicyHelver().AdminRole_CreateEditDeleteClaim_ORSuperAdminRole(context)
    ));

    options.AddPolicy("OnlySuperAdminChecker", p => p.Requirements.Add(new OnlySuperAdminChecker()));


    options.AddPolicy("AdminWIthMoreTHan1000Days", p => p.Requirements.Add(new AdminWithMoreThan1000DaysRequirement(1000)));

    options.AddPolicy("FirstNameAuth", p => p.Requirements.Add(new FirstNameAuthRequirement("test4")));
});

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


app.Run();
