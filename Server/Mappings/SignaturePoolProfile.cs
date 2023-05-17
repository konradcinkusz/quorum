namespace MR.Server.Mappings;

public class SignaturePoolProfile : Profile
{
    public SignaturePoolProfile()
    {
        CreateMap<SignaturePool, SignaturePoolDTO>()
                        .ForMember(dest => dest.SignatureDTOs, opt => opt.MapFrom(src => src.Signatures))
                        .ForMember(dest => dest.QuarterDTO, opt => opt.MapFrom(src => src.Quarter))
                        .ForMember(dest => dest.ApplicationUserEmail, opt => opt.MapFrom(src => src.ApplicationUser.Email));
    }
}
