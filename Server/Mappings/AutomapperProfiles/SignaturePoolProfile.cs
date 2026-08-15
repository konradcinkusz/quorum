namespace Quorum.Server.Mappings.AutomapperProfiles;

public class SignaturePoolProfile : Profile
{
    public SignaturePoolProfile()
    {
        CreateMap<SignaturePool, SignaturePoolAdminDTO>()
                        .ForMember(dest => dest.SignatureDTOs, opt => opt.MapFrom(src => src.Signatures))
                        .ForMember(dest => dest.QuarterDTO, opt => opt.MapFrom(src => src.Quarter))
                        .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.ApplicationUser.Email));

        CreateMap<SignaturePool, UserSignaturePool>()
                        .ForMember(dest => dest.SignatureDTOs, opt => opt.MapFrom(src => src.Signatures))
                        .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Quarter.Year))
                        .ForMember(dest => dest.QuarterNumber, opt => opt.MapFrom(src => src.Quarter.QuarterNumber));
    }
}
