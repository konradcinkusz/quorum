var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<RoleAuthorizationMessageHandler>();

builder.Services.InitializeHTTPDataServices(new Uri(builder.HostEnvironment.BaseAddress));

builder.Services.AddApiAuthorization();
builder.Services.AddAuthorizationCore(options => {
    options.AddPolicy("AdminRoles", policy =>
    {
        policy.Requirements.Add(new AnyRoleRequirement("SuperAdmin", "SuperAdmin.Admin", "Admin"));
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, AnyRoleAuthorizationHandler>();

await builder.Build().RunAsync();
