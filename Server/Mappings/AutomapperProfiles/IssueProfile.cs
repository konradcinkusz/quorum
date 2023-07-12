namespace MR.Server.Mappings.AutomapperProfiles;

public class IssueProfile : Profile
{
    public IssueProfile()
    {
        CreateMap<IssueVisibilityHistory, IssueVisibilityHistoryDTO>();
        CreateMap<IssueProcessingHistory, IssueProcessingHistoryDTO>();

        CreateMap<Issue, IssueAdminCreateDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment))
            .ForMember(dest => dest.ApplicationUserId, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Id : string.Empty))
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Email : string.Empty));

        CreateMap<Issue, IssueCreateDTO>();
        CreateMap<Issue, IssueAdminCreateDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment));

        CreateMap<Issue, IssueReadDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment))
            .ForMember(dest => dest.ApplicationUserId, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Id : string.Empty))
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Email : string.Empty))
            .ForMember(dest => dest.QuarterDTOs, opt => opt.MapFrom(src => src.QuarterIssues.Select(x => x.Quarter)))
            .PreserveReferences();

        CreateMap<Issue, PublicPublishedIssueRead>()
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Email : string.Empty))
            .PreserveReferences();
    }
}

