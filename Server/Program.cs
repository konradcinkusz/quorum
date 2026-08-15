var builder = WebApplication.CreateBuilder(args);

builder.ConfigureOpenTelemetry();

// P4: provider behind a configuration switch (PostgreSQL deployed, SQL Server locally,
// InMemory with nothing configured), schema applied by a hosted service after Kestrel is
// up so a slow migration is never read as a failed deploy.
builder.Services.AddDbContextService(builder.Configuration);
builder.Services.AddDatabaseSchemaMigration();

// P5: identity lives in authservice (ADR 0001). Quorum validates bearer tokens against
// that instance's published JWKS and holds no key material — it can verify a token and
// cannot mint one. The BFF half proxies login/register/refresh and keeps the tokens in
// HttpOnly cookies so the browser never holds them.
builder.Services.AddExternalJwtAuthentication(builder.Configuration);
builder.Services.AddBffAuthentication(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    // RequireClaim rather than RequireRole, so the policy states the claim type it reads
    // (ClaimTypes.Role — what the JWT handler's inbound mapping produces from `role`).
    // authservice's platform roles include Admin and SuperAdmin, same names as before.
    options.AddPolicy(Constants.Policies.RequireAdminRole, policy =>
    {
        policy.RequireClaim(ClaimTypes.Role, new[] { Constants.Claims.Admin, Constants.Claims.SuperAdmin });
    });
});

builder.Services.AddRazorPages();

builder.Services.AddController();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScopedServices();

builder.Services.AddTransientServices();

builder.Services.AddServiceLayer(builder.Configuration);

builder.Services.AddVersion();

builder.Services.AddDefaultHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quorum", Version = "v1" });
});

var app = builder.Build();

// Translates the service layer's exception types into API responses.
app.ConfigureCustomExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
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

// The BFF's HttpOnly access-token cookie becomes the Authorization header here, so the
// one authentication path below — bearer JWT against authservice's JWKS — serves browser
// and API callers alike.
app.UseTokenCookieBridge();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Before MapFallbackToFile, which would otherwise answer /health with index.html and a 200
// — a health probe that passes no matter what the application is doing.
app.MapDefaultEndpoints();

app.MapFallbackToFile("index.html");

app.UseSwaggerUI(c =>
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quorum v1"));

app.Run();
