namespace MR.Server.Mappings;

public class QuarterProfile : Profile
{
    public QuarterProfile()
    {
        CreateMap<Quarter, QuarterDTO>();
    }
}
