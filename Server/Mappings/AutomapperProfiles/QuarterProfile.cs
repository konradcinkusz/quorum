namespace MR.Server.Mappings.AutomapperProfiles;

public class QuarterProfile : Profile
{
    public QuarterProfile()
    {
        CreateMap<Quarter, QuarterDTO>();
    }
}
