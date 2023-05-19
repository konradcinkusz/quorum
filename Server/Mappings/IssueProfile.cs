namespace MR.Server.Mappings;

public class IssueProfile : Profile
{
    public IssueProfile()
    {
        CreateMap<Issue, IssueDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment))
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Email : string.Empty));
    }
}
