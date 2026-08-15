namespace Quorum.Server.Mappings.AutomapperProfiles;

public class QuarterProfile : Profile
{
    public QuarterProfile()
    {
        CreateMap<Quarter, QuarterDTO>()
            .ForMember(dest => dest.QuarterWinner, opt => opt.MapFrom(src => src.QuarterIssues.SingleOrDefault(x => x.QuarterWinner.HasValue && x.QuarterWinner.Value)));
    }
}
