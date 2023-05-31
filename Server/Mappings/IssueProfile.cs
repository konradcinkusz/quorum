namespace MR.Server.Mappings;

public class IssueProfile : Profile
{
    public IssueProfile()
    {
        CreateMap<Issue, IssueAdminCreateDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment))
            .ForMember(dest => dest.ApplicationUserId, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Id : string.Empty))
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Email : string.Empty));
        
        CreateMap<Issue, IssueCreateDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment));

        CreateMap<Issue, IssueReadDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment))
            .ForMember(dest => dest.ApplicationUserId, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Id : string.Empty))
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Email : string.Empty));

    }
}
