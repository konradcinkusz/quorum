var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#region Init HTTP Client
const string MRPaymentDataService = "MR.ServerAPI";
builder.Services
    .AddHttpClient<IPaymentService, PaymentService>(MRPaymentDataService, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

const string MRAuthorizedDataService = "MR.ServerAPI.Authorized";
builder.Services
    .AddHttpClient<IAuthorizedMRDataService, AuthorizedMRDataService>(MRAuthorizedDataService, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(MRPaymentDataService));
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(MRAuthorizedDataService));
builder.Services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
#endregion

builder.Services.AddApiAuthorization();

await builder.Build().RunAsync();
