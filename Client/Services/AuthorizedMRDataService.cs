using MR.Client.Services.Base;

namespace MR.Client.Services;

public class AuthorizedMRDataService : DataServiceBase, IAuthorizedMRDataService
{
    public AuthorizedMRDataService(HttpClient httpclient) : base(httpclient)
    {
    }
}
