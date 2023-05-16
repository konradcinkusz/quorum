namespace MR.Server.Mappings;

public class SignatureProfile : Profile
{
    public SignatureProfile()
    {
        CreateMap<Signature, SignatureDTO>();
    }
}