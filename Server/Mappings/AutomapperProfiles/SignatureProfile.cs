namespace MR.Server.Mappings.AutomapperProfiles;

public class SignatureProfile : Profile
{
    public SignatureProfile()
    {
        CreateMap<Signature, SignatureDTO>();
    }
}