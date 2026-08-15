using Quorum.Client.Features.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);

// The BFF session model (ADR 0001): tokens live in HttpOnly cookies the browser attaches
// on its own; the client asks /bff/auth/session who it is and never sees a token.
builder.Services.AddHttpClient<IBffAuthClient, BffAuthClient>(client => client.BaseAddress = baseAddress);
builder.Services.AddScoped<BffAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<BffAuthenticationStateProvider>());
builder.Services.AddScoped<SessionRefreshHandler>();
builder.Services.AddAuthorizationCore(options =>
{
    // The session claims carry one role per `role` claim, so the policy is a plain role
    // check — the JSON-array string parsing the old provider needed is gone with it.
    options.AddPolicy("AdminRoles", policy => policy.RequireRole("SuperAdmin", "Admin"));
});

builder.Services.InitializePrivateHTTPDataServices(baseAddress);
builder.Services.InitializePublicHTTPDataServices(baseAddress);

await builder.Build().RunAsync();
