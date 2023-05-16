namespace MR.Server.Mappings;

public class SignaturePoolProfile : Profile
{
    public SignaturePoolProfile()
    {
        CreateMap<SignaturePool, SignaturePoolDTO>()
                        .ForMember(dest => dest.SignatureDTOs, opt => opt.MapFrom(src => src.Signatures));
    }
}
