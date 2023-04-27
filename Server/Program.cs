var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextService(builder.Configuration);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>
    (options => options.SignIn.RequireConfirmedAccount = true)
    .AddUserManager<MRUserManager>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();

//https://stackoverflow.com/q/70563303
//https://github.com/dotnet/AspNetCore.Docs/issues/14944
builder.Services.AddIdentityServer()
    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(opt => {
        opt.IdentityResources["openid"].UserClaims.Add("name");
        opt.ApiResources.Single().UserClaims.Add("name");
        opt.IdentityResources["openid"].UserClaims.Add("role");
        opt.ApiResources.Single().UserClaims.Add("role");
        opt.IdentityResources["openid"].UserClaims.Add("isActiveSubscription");
        opt.ApiResources.Single().UserClaims.Add("isActiveSubscription");
    });


builder.Services.AddAuthentication()
    .AddIdentityServerJwt();

//Roles not working - erroneously appears as if User is not in a Role, .NET 6 (upgrading from .Net Core 3.2)
//https://stackoverflow.com/a/73930254/4510954
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, new[] { "Admin", "SuperAdmin" });
    });
});

builder.Services.AddRazorPages();

builder.Services.AddController();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//builder.Services.AddInfrastructureAutoMapper();

builder.Services.AddScopedServices();

builder.Services.AddTransientServices();

builder.Services.AddServiceLayer();

builder.Services.AddVersion();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MR", Version = "v1" });
});

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
app.UseSwagger();

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.UseSwaggerUI(c =>
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "MR v1"));

app.Run();
