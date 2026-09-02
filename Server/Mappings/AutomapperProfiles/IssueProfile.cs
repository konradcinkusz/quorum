namespace Quorum.Server.Mappings.AutomapperProfiles;

public class IssueProfile : Profile
{
    public IssueProfile()
    {
        CreateMap<IssueVisibilityHistory, IssueVisibilityHistoryDTO>();
        CreateMap<IssueProcessingHistory, IssueProcessingHistoryDTO>();

        CreateMap<Issue, IssueAdminCreateDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment))
            .ForMember(dest => dest.ApplicationUserId, opt => opt.MapFrom(src => src.CreatedById ?? string.Empty))
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedByEmail ?? string.Empty));

        CreateMap<Issue, IssueCreateDTO>();

        CreateMap<CloudinaryFile, FileReadDTO>();

        CreateMap<Issue, IssueReadDTO>()
            .ForMember(dest => dest.InitialPayment, opt => opt.MapFrom(src => src.InitialPayment))
            .ForMember(dest => dest.ApplicationUserId, opt => opt.MapFrom(src => src.CreatedById ?? string.Empty))
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedByEmail ?? string.Empty))
            .ForMember(dest => dest.PDF, opt => opt.MapFrom(src => src.CloudinaryFileIssues != null && src.CloudinaryFileIssues.Any(x => x.CloudinaryFileIssueType == CloudinaryFileIssueType.General) ? src.CloudinaryFileIssues.Where(x => x.CloudinaryFileIssueType == CloudinaryFileIssueType.General).Select(x => x.CloudinaryFile).First() : new CloudinaryFile()))
            .PreserveReferences();

        CreateMap<Issue, IssueAdminRatingValueCalculate>();

        CreateMap<Issue, PublicPublishedIssueRead>()
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedByEmail ?? string.Empty))
            .PreserveReferences();

        CreateMap<Issue, PublicPublishedEndedIssueRead>()
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedByEmail ?? string.Empty))
            .ForMember(dest => dest.QuarterNumber, opt => opt.MapFrom(src => src.QuarterIssues.FirstOrDefault(x => x.QuarterWinner.HasValue && x.QuarterWinner.Value).Quarter.QuarterNumber))
            .ForMember(dest => dest.QuarterYear, opt => opt.MapFrom(src => src.QuarterIssues.FirstOrDefault(x => x.QuarterWinner.HasValue && x.QuarterWinner.Value).Quarter.Year))
            .PreserveReferences();

        CreateMap<Issue, IssueSignedAndSubmittedDTO>()
            .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.CreatedByEmail ?? string.Empty))
            .ForMember(dest => dest.QuarterNumber, opt => opt.MapFrom(src => src.QuarterIssues.FirstOrDefault(x => x.QuarterWinner.HasValue && x.QuarterWinner.Value).Quarter.QuarterNumber))
            .ForMember(dest => dest.QuarterYear, opt => opt.MapFrom(src => src.QuarterIssues.FirstOrDefault(x => x.QuarterWinner.HasValue && x.QuarterWinner.Value).Quarter.Year))
            .ForMember(dest => dest.PDF, opt => opt.MapFrom(src => src.CloudinaryFileIssues != null && src.CloudinaryFileIssues.Any(x => x.CloudinaryFileIssueType == CloudinaryFileIssueType.General) ? src.CloudinaryFileIssues.Where(x => x.CloudinaryFileIssueType == CloudinaryFileIssueType.General).Select(x => x.CloudinaryFile).First() : new CloudinaryFile()))
            .PreserveReferences();
    }
}

