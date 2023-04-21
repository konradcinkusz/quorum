namespace MR.Server.Mappings;

public class AdminLogProfile : Profile
{
    public AdminLogProfile()
    {
        CreateMap<AdminLog, AdminLogDTO>();
        CreateMap<AdminLogDTO, AdminLog>();
    }
}

