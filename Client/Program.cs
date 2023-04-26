using MR.Client.Features;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<RoleAuthorizationMessageHandler>();

#region Init HTTP Client
const string MRPaymentDataService = "MR.ServerAPI";
builder.Services
    .AddHttpClient<IPaymentService, PaymentService>(MRPaymentDataService, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
    .AddHttpMessageHandler<RoleAuthorizationMessageHandler>();

const string MRAdmintDataService = "MR.ServerAPI.Admin";
builder.Services
    .AddHttpClient<IAdminService, AdminService>(MRAdmintDataService, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
    .AddHttpMessageHandler<RoleAuthorizationMessageHandler>();

const string MRSubscriptionDataService = "MR.ServerAPI.Subscription";
builder.Services
    .AddHttpClient<ISubscriptionService, SubscriptionService>(MRSubscriptionDataService, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
//builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(MRPaymentDataService));
//builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(MRAuthorizedDataService));
builder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
#endregion

builder.Services.AddApiAuthorization();
builder.Services.AddAuthorizationCore(options => {
    options.AddPolicy("AdminRoles", policy =>
    {
        policy.Requirements.Add(new AnyRoleRequirement("SuperAdmin", "SuperAdmin.Admin", "Admin"));
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, AnyRoleAuthorizationHandler>();

await builder.Build().RunAsync();
