namespace Quorum.Server.Mappings.AutomapperProfiles;

public class SignatureProfile : Profile
{
    public SignatureProfile()
    {
        CreateMap<Signature, SignatureDTO>()
            .ForMember(dest => dest.IssueDTO, opt => opt.MapFrom(src => src.Issue))
            .MaxDepth(1);
    }
}