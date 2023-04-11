using Microsoft.AspNetCore.Authentication;
using MR.Domain.Settings;
using MR.Persistence;
using MR.Service;
using MR.Infrastructure.Extension;
using MR.Domain.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextService(builder.Configuration);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>
    (options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

builder.Services.AddRazorPages();

builder.Services.AddController();

builder.Services.AddAutoMapper();

builder.Services.AddScopedServices();

builder.Services.AddTransientServices();

builder.Services.AddSwaggerOpenAPI();

builder.Services.AddServiceLayer();

builder.Services.AddVersion();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
