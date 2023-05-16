namespace MR.Server.Mappings;

public class IssueProfile : Profile
{
    public IssueProfile()
    {
        CreateMap<Issue, IssueDTO>();
    }
}