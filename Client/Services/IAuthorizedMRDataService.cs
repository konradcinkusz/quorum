namespace MR.Client.Services;

public interface IAuthorizedMRDataService
{

}
public class AuthorizedMRDataService : DataServiceBase, IAuthorizedMRDataService
{
    public AuthorizedMRDataService(HttpClient httpclient) : base(httpclient)
    {
    }
}
