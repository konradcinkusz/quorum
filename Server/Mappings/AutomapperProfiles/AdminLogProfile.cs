namespace MR.Server.Mappings.AutomapperProfiles;

public class AdminLogProfile : Profile
{
    public AdminLogProfile()
    {
        CreateMap<AdminLog, AdminLogDTO>();
        CreateMap<AdminLogDTO, AdminLog>();
    }
}
